using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Library.Pages
{
    public partial class MyLoans : Page
    {
        private string Cs => ConfigurationManager.ConnectionStrings["Db"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null)
            {
                Response.Redirect("~/Pages/Login.aspx");
                return;
            }
            if (!IsPostBack) LoadLoans();
        }

        private void LoadLoans()
        {
            int userId = Convert.ToInt32(Session["UserId"]);

            using (var conn = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(@"
SELECT l.Id          AS LoanId,
       b.Id          AS BookId,
       b.Title       AS Title,
       l.LoanDate,
       l.DueDate,
       l.ReturnDate
FROM dbo.Loans l
JOIN dbo.BookCopies bc ON bc.Id = l.BookCopyId
JOIN dbo.Books b      ON b.Id  = bc.BookId
WHERE l.UserId = @uid
ORDER BY ISNULL(l.ReturnDate, l.DueDate) DESC, l.Id DESC;", conn))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.AddWithValue("@uid", userId);
                var dt = new DataTable();
                da.Fill(dt);

                phNoLoans.Visible = dt.Rows.Count == 0;
                repLoans.DataSource = dt;
                repLoans.DataBind();
            }
        }

        protected void repLoans_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem) return;

            var lnkBook = (HyperLink)e.Item.FindControl("lnkBook");  // <-- artık HyperLink
            var litLoan = (Literal)e.Item.FindControl("litLoan");
            var litDue = (Literal)e.Item.FindControl("litDue");
            var litStatus = (Literal)e.Item.FindControl("litStatus");
            var btnReturn = (Button)e.Item.FindControl("btnReturn");

            var hidBookId = (HiddenField)e.Item.FindControl("hidBookId");
            var hidLoanDate = (HiddenField)e.Item.FindControl("hidLoanDate");
            var hidDueDate = (HiddenField)e.Item.FindControl("hidDueDate");
            var hidReturn = (HiddenField)e.Item.FindControl("hidReturnDate");

            int bookId = Convert.ToInt32(hidBookId.Value);

            // Başlık + detay linki
            lnkBook.Text = Convert.ToString(DataBinder.Eval(e.Item.DataItem, "Title"));
            lnkBook.NavigateUrl = ResolveUrl("~/Pages/BookDetails.aspx?id=" + bookId);

            // Tarihler
            DateTime loanDate, dueDate;
            DateTime.TryParse(hidLoanDate.Value, out loanDate);
            DateTime.TryParse(hidDueDate.Value, out dueDate);

            litLoan.Text = loanDate == default ? "-" : loanDate.ToString("dd.MM.yyyy");
            litDue.Text = dueDate == default ? "-" : dueDate.ToString("dd.MM.yyyy");

            bool isReturned = !string.IsNullOrEmpty(hidReturn.Value) &&
                              !hidReturn.Value.Equals("null", StringComparison.OrdinalIgnoreCase);

            if (isReturned)
            {
                litStatus.Text = "<span class='badge bg-success'>İade edildi</span>";
                btnReturn.Visible = false;
            }
            else
            {
                if (dueDate != default && dueDate.Date < DateTime.Now.Date)
                    litStatus.Text = "<span class='badge bg-danger'>Gecikmiş</span>";
                else
                    litStatus.Text = "<span class='badge bg-warning text-dark'>Devam ediyor</span>";

                btnReturn.Visible = true;
            }
        }

        protected void repLoans_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "return")
            {
                int loanId = Convert.ToInt32(e.CommandArgument);
                DoReturn(loanId);
                LoadLoans();
            }
        }

        private void DoReturn(int loanId)
        {
            int userId = Convert.ToInt32(Session["UserId"]);
            lblMsg.CssClass = ""; lblMsg.Text = "";

            using (var conn = new SqlConnection(Cs))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        int copyId = 0;
                        DateTime? returnDate = null;

                        // Kayıt doğrulama + kilitleme
                        using (var get = new SqlCommand(@"
SELECT TOP(1) l.BookCopyId, l.ReturnDate
FROM dbo.Loans l WITH (UPDLOCK, ROWLOCK)
WHERE l.Id=@lid AND l.UserId=@uid;", conn, tx))
                        {
                            get.Parameters.AddWithValue("@lid", loanId);
                            get.Parameters.AddWithValue("@uid", userId);
                            using (var rdr = get.ExecuteReader())
                            {
                                if (!rdr.Read())
                                {
                                    Info("Kayıt bulunamadı.");
                                    tx.Rollback(); return;
                                }
                                copyId = Convert.ToInt32(rdr["BookCopyId"]);
                                if (rdr["ReturnDate"] != DBNull.Value)
                                    returnDate = Convert.ToDateTime(rdr["ReturnDate"]);
                            }
                        }

                        if (returnDate.HasValue)
                        {
                            Info("Bu ödünç zaten iade edilmiş.");
                            tx.Rollback(); return;
                        }

                        // Loan'ı iade et
                        using (var upLoan = new SqlCommand(@"
UPDATE dbo.Loans SET ReturnDate = GETDATE()
WHERE Id=@lid AND ReturnDate IS NULL;", conn, tx))
                        {
                            upLoan.Parameters.AddWithValue("@lid", loanId);
                            upLoan.ExecuteNonQuery();
                        }

                        // Kopyayı müsait yap
                        using (var upCopy = new SqlCommand(@"
UPDATE dbo.BookCopies SET Status='Available' WHERE Id=@cid;", conn, tx))
                        {
                            upCopy.Parameters.AddWithValue("@cid", copyId);
                            upCopy.ExecuteNonQuery();
                        }

                        tx.Commit();
                        Success("İade işlemi tamamlandı.");
                    }
                    catch (Exception ex)
                    {
                        try { tx.Rollback(); } catch { }
                        Danger("İşlem başarısız: " + ex.Message);
                    }
                }
            }
        }

        private void Success(string msg) { lblMsg.CssClass = "alert alert-success"; lblMsg.Text = msg; }
        private void Danger(string msg) { lblMsg.CssClass = "alert alert-danger"; lblMsg.Text = msg; }
        private void Info(string msg) { lblMsg.CssClass = "alert alert-info"; lblMsg.Text = msg; }
    }
}
