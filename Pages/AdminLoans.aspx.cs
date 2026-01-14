using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Library.Pages
{
    public partial class AdminLoans : Page
    {
        private string Cs => ConfigurationManager.ConnectionStrings["Db"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            var role = Convert.ToString(Session["Role"] ?? "");
            if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("~/Pages/Login.aspx");
                return;
            }

            if (!IsPostBack)
                BindLoans();
        }

        protected void btnFilter_Click(object sender, EventArgs e) => BindLoans();

        private void BindLoans()
        {
            using (var conn = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(BuildSql(out var p), conn))
            using (var da = new SqlDataAdapter(cmd))
            {
                foreach (var pr in p) cmd.Parameters.Add(pr);

                var dt = new DataTable();
                da.Fill(dt);
                repLoans.DataSource = dt;
                repLoans.DataBind();
            }
        }

        // Loans(Id, UserId, BookCopyId, LoanDate, DueDate, ReturnDate) — Users tablosunda "Name" ve "Email" var.
        private string BuildSql(out SqlParameter[] parameters)
        {
            var sb = new StringBuilder(@"
SELECT
    l.Id,
    u.[Name]  AS UserName,
    u.Email   AS UserEmail,
    b.Title   AS BookTitle,
    bc.Id     AS CopyId,
    l.LoanDate,
    l.DueDate,
    l.ReturnDate
FROM dbo.Loans l
JOIN dbo.Users      u  ON u.Id  = l.UserId
JOIN dbo.BookCopies bc ON bc.Id = l.BookCopyId
JOIN dbo.Books      b  ON b.Id  = bc.BookId
WHERE 1=1 ");

            var list = new System.Collections.Generic.List<SqlParameter>();

            string user = (txtUser.Text ?? "").Trim();
            if (!string.IsNullOrEmpty(user))
            {
                sb.Append(" AND (u.[Name] LIKE @user OR u.Email LIKE @user) ");
                list.Add(new SqlParameter("@user", "%" + user + "%"));
            }

            string book = (txtBook.Text ?? "").Trim();
            if (!string.IsNullOrEmpty(book))
            {
                sb.Append(" AND (b.Title LIKE @book) ");
                list.Add(new SqlParameter("@book", "%" + book + "%"));
            }

            string status = Convert.ToString(ddlStatus.SelectedValue ?? "");
            if (status == "ongoing")
                sb.Append(" AND l.ReturnDate IS NULL ");
            else if (status == "overdue")
                sb.Append(" AND l.ReturnDate IS NULL AND l.DueDate < GETDATE() ");
            else if (status == "returned")
                sb.Append(" AND l.ReturnDate IS NOT NULL ");

            string d = Convert.ToString(ddlDate.SelectedValue ?? "");
            if (d == "last7")
                sb.Append(" AND l.LoanDate >= DATEADD(day,-7, CAST(GETDATE() AS date)) ");
            else if (d == "thismonth")
                sb.Append(" AND l.LoanDate >= DATEFROMPARTS(YEAR(GETDATE()),MONTH(GETDATE()),1) ");
            else if (d == "prevmonth")
                sb.Append(@"
AND l.LoanDate >= DATEFROMPARTS(YEAR(DATEADD(month,-1,GETDATE())),MONTH(DATEADD(month,-1,GETDATE())),1)
AND l.LoanDate <  DATEFROMPARTS(YEAR(GETDATE()),MONTH(GETDATE()),1) ");

            sb.Append(" ORDER BY ISNULL(l.ReturnDate, l.DueDate) DESC, l.Id DESC; ");

            parameters = list.ToArray();
            return sb.ToString();
        }

        protected void repLoans_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int loanId = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "return")
            {
                try
                {
                    ReturnLoan(loanId);
                    Success("İade alındı.");
                }
                catch (Exception ex)
                {
                    Error("İade sırasında hata: " + ex.Message);
                }
                BindLoans();
            }
            else if (e.CommandName == "extend")
            {
                try
                {
                    ExtendLoan(loanId, 7);
                    Success("Süre 7 gün uzatıldı.");
                }
                catch (Exception ex)
                {
                    Error("Süre uzatma sırasında hata: " + ex.Message);
                }
                BindLoans();
            }
        }

        // ReturnDate = now, ilgili kopya Available
        private void ReturnLoan(int loanId)
        {
            using (var conn = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(@"
UPDATE dbo.Loans
   SET ReturnDate = GETDATE()
 WHERE Id = @id AND ReturnDate IS NULL;

UPDATE bc
   SET bc.Status = 'Available'
  FROM dbo.BookCopies bc
  JOIN dbo.Loans l ON l.BookCopyId = bc.Id
 WHERE l.Id = @id AND l.ReturnDate IS NOT NULL;", conn))
            {
                cmd.Parameters.AddWithValue("@id", loanId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // DueDate += @days (ReturnDate boşsa)
        // DueDate computed column olduğu için DueDate UPDATE edilemez.
        // Süre uzatma: LoanDate'i geri al -> DueDate (LoanDate+14) otomatik ileri gider.
        private void ExtendLoan(int loanId, int days)
        {
            using (var conn = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(@"
UPDATE dbo.Loans
   SET LoanDate = DATEADD(day, +@d, LoanDate)
 WHERE Id=@id AND ReturnDate IS NULL;", conn))
            {
                cmd.Parameters.AddWithValue("@id", loanId);
                cmd.Parameters.AddWithValue("@d", days);

                conn.Open();
                int n = cmd.ExecuteNonQuery();
                if (n == 0) throw new InvalidOperationException("İade edilmiş veya bulunamadı.");
            }
        }


        /* ---- Görsel yardımcılar ---- */
        protected bool IsReturned(object returnDate)
            => !(returnDate == null || returnDate == DBNull.Value);

        protected string StatusBadge(object returnDate, object dueDate)
        {
            bool returned = IsReturned(returnDate);
            if (returned) return "<span class='badge bg-success status-badge'>İade edildi</span>";

            DateTime due = (dueDate == DBNull.Value || dueDate == null) ? DateTime.MinValue : (DateTime)dueDate;
            if (due != DateTime.MinValue && due.Date < DateTime.Now.Date)
                return "<span class='badge bg-danger status-badge'>Gecikmiş</span>";

            return "<span class='badge bg-warning text-dark status-badge'>Devam ediyor</span>";
        }

        // .aspx tarafındaki kırmızı alt çizgileri yok etmek için:
        protected string GetReturnCss(object returnDate)
            => IsReturned(returnDate) ? "btn btn-sm btn-outline-secondary"
                                      : "btn btn-sm btn-outline-success";

        protected string GetExtendCss(object returnDate)
            => IsReturned(returnDate) ? "btn btn-sm btn-outline-secondary ms-1"
                                      : "btn btn-sm btn-outline-primary ms-1";

        /* ---- UI helpers ---- */
        private void Success(string m) { lblMsg.CssClass = "alert alert-success"; lblMsg.Text = m; }
        private new void Error(string m) { lblMsg.CssClass = "alert alert-danger"; lblMsg.Text = m; }
    }
}
