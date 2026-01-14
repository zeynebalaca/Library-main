using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace Library.Pages
{
    public partial class BookDetails : Page
    {
        private string Cs => ConfigurationManager.ConnectionStrings["Db"].ConnectionString;
        private int BookId => int.TryParse(Request["id"], out var id) ? id : 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            // oturum zorunlu
            if (Session["UserId"] == null)
            {
                Response.Redirect("~/Pages/Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack)
            {
                // rating seçenekleri
                ddlRating.Items.Clear();
                for (int i = 5; i >= 1; i--) ddlRating.Items.Add(i.ToString());

                LoadBook();
                LoadReviews();

                // 🔴 SADECE EKLENEN KONTROL
                int userId = Convert.ToInt32(Session["UserId"]);
                if (UserAlreadyReviewed(BookId, userId))
                {
                    pnlReviewForm.Visible = false;
                    lblReviewMsg.Text = "Bu kitaba zaten yorum yaptınız.";
                    lblReviewMsg.CssClass = "text-warning";
                }

                var role = Convert.ToString(Session["Role"] ?? "");
                pnlReviewForm.Visible =
                    pnlReviewForm.Visible &&
                    !role.Equals("Admin", StringComparison.OrdinalIgnoreCase);

                pnlAdminNote.Visible =
                    role.Equals("Admin", StringComparison.OrdinalIgnoreCase);
            }
        }

        private void LoadBook()
        {
            if (BookId <= 0) { pnlContent.Visible = false; pnlNotFound.Visible = true; return; }

            using (var conn = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(@"
SELECT b.Id, b.ISBN, b.Title, b.Author, b.[Year], b.CategoryId, b.AvgRating, b.CoverPath,
       c.Name AS CategoryName,
       (SELECT COUNT(*) FROM dbo.BookCopies WHERE BookId=b.Id) AS TotalCopies,
       (SELECT COUNT(*) FROM dbo.BookCopies WHERE BookId=b.Id AND Status='Available') AS AvailCopies
FROM dbo.Books b
LEFT JOIN dbo.Categories c ON c.Id = b.CategoryId
WHERE b.Id = @id;", conn))
            {
                cmd.Parameters.AddWithValue("@id", BookId);
                conn.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    if (!rdr.Read())
                    {
                        pnlContent.Visible = false; pnlNotFound.Visible = true; return;
                    }

                    litTitle.Text = Convert.ToString(rdr["Title"]);
                    litAuthor.Text = Convert.ToString(rdr["Author"]);
                    litYear.Text = Convert.ToString(rdr["Year"]);
                    litCategory.Text = Convert.ToString(rdr["CategoryName"]);

                    var path = Convert.ToString(rdr["CoverPath"] ?? "").Trim();
                    if (string.IsNullOrEmpty(path))
                        path = "~/Content/placeholder-book.png";

                    imgCover.Src = ResolveUrl(path);
                    imgCover.Attributes["onerror"] =
                        "this.onerror=null;this.src='" + ResolveUrl("~/Content/placeholder-book.png") + "';";

                    var avg = rdr["AvgRating"] == DBNull.Value ? 0.0 : Convert.ToDouble(rdr["AvgRating"]);
                    litAvg.Text = avg.ToString("0.0");
                    stars.InnerHtml = Stars((int)Math.Round(avg));

                    litCopies.Text = Convert.ToString(rdr["TotalCopies"]);
                    litAvail.Text = Convert.ToString(rdr["AvailCopies"]);

                    int avail = Convert.ToInt32(rdr["AvailCopies"]);
                    btnBorrow.Enabled = avail > 0;
                    btnBorrow.CssClass = avail > 0
                        ? "btn btn-primary btn-sm"
                        : "btn btn-secondary btn-sm disabled";

                    pnlContent.Visible = true;
                    pnlNotFound.Visible = false;
                }
            }
        }

        private void LoadReviews()
        {
            using (var conn = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(@"
SELECT TOP (20)
       r.Id,
       u.Name AS UserName,
       r.Rating,
       r.Comment,
       r.CreatedAt
FROM dbo.Reviews r
JOIN dbo.Users u ON u.Id = r.UserId
WHERE r.BookId = @id
ORDER BY r.CreatedAt DESC, r.Id DESC;", conn))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.AddWithValue("@id", BookId);
                var dt = new DataTable();
                da.Fill(dt);
                phNoReviews.Visible = dt.Rows.Count == 0;
                repReviews.DataSource = dt;
                repReviews.DataBind();
            }
        }

        protected void btnBorrow_Click(object sender, EventArgs e)
        {
            lblBorrowMsg.CssClass = ""; lblBorrowMsg.Text = "";

            int userId = Convert.ToInt32(Session["UserId"]);

            using (var conn = new SqlConnection(Cs))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        int copyId;
                        using (var get = new SqlCommand(@"
SELECT TOP(1) Id FROM dbo.BookCopies WITH (UPDLOCK, ROWLOCK)
WHERE BookId=@bid AND Status='Available'
ORDER BY Id ASC;", conn, tx))
                        {
                            get.Parameters.AddWithValue("@bid", BookId);
                            var obj = get.ExecuteScalar();
                            if (obj == null)
                            {
                                Info(lblBorrowMsg, "Müsait kopya kalmadı.");
                                tx.Rollback(); LoadBook(); return;
                            }
                            copyId = Convert.ToInt32(obj);
                        }

                        using (var up = new SqlCommand(
                            "UPDATE dbo.BookCopies SET Status='Loaned' WHERE Id=@cid;",
                            conn, tx))
                        {
                            up.Parameters.AddWithValue("@cid", copyId);
                            up.ExecuteNonQuery();
                        }

                        using (var ins = new SqlCommand(@"
INSERT INTO dbo.Loans (UserId, BookCopyId, LoanDate, ReturnDate)
VALUES (@uid, @cid, GETDATE(), NULL);
",
                            conn, tx))
                        {
                            ins.Parameters.AddWithValue("@uid", userId);
                            ins.Parameters.AddWithValue("@cid", copyId);
                            ins.ExecuteNonQuery();
                        }

                        tx.Commit();
                        Success(lblBorrowMsg, "Ödünç alma başarılı. İyi okumalar!");
                        LoadBook();
                    }
                    catch (Exception ex)
                    {
                        try { tx.Rollback(); } catch { }
                        Danger(lblBorrowMsg, "İşlem başarısız: " + ex.Message);
                    }
                }
            }
        }

        protected void btnAddReview_Click(object sender, EventArgs e)
        {
            lblReviewMsg.CssClass = ""; lblReviewMsg.Text = "";

            int userId = Convert.ToInt32(Session["UserId"]);

            // 🔴 SADECE EKLENEN GÜVENLİK
            if (UserAlreadyReviewed(BookId, userId))
            {
                Danger(lblReviewMsg, "Bu kitaba zaten yorum yaptınız.");
                pnlReviewForm.Visible = false;
                return;
            }

            int rating = Math.Max(1, Math.Min(5, Convert.ToInt32(ddlRating.SelectedValue)));
            string comment = (txtComment.Text ?? "").Trim();
            if (comment.Length == 0)
            {
                Danger(lblReviewMsg, "Yorum boş olamaz.");
                return;
            }

            using (var conn = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(@"
INSERT INTO dbo.Reviews (UserId, BookId, Rating, Comment, CreatedAt)
VALUES (@u, @b, @r, @c, GETDATE());

UPDATE dbo.Books
SET AvgRating = CAST((
    SELECT AVG(CAST(Rating AS float)) FROM dbo.Reviews WHERE BookId=@b
) AS decimal(4,2))
WHERE Id=@b;", conn))
            {
                cmd.Parameters.AddWithValue("@u", userId);
                cmd.Parameters.AddWithValue("@b", BookId);
                cmd.Parameters.AddWithValue("@r", rating);
                cmd.Parameters.AddWithValue("@c", comment);

                conn.Open();
                cmd.ExecuteNonQuery();

                Success(lblReviewMsg, "Yorum eklendi.");
                pnlReviewForm.Visible = false;
                txtComment.Text = "";
                LoadBook();
                LoadReviews();
            }
        }

        // 🔴 SADECE EKLENEN METOT
        private bool UserAlreadyReviewed(int bookId, int userId)
        {
            using (var conn = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM dbo.Reviews WHERE BookId=@b AND UserId=@u", conn))
            {
                cmd.Parameters.AddWithValue("@b", bookId);
                cmd.Parameters.AddWithValue("@u", userId);
                conn.Open();
                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        protected string Stars(int n)
        {
            n = Math.Max(0, Math.Min(5, n));
            return new string('★', n).Replace("★", "<span class='star'>★</span>") +
                   new string('☆', 5 - n).Replace("☆", "<span class='star muted'>☆</span>");
        }

        private void Success(System.Web.UI.WebControls.Label lbl, string msg)
        {
            lbl.CssClass = "ms-2 text-success";
            lbl.Text = msg;
        }
        private void Danger(System.Web.UI.WebControls.Label lbl, string msg)
        {
            lbl.CssClass = "ms-2 text-danger";
            lbl.Text = msg;
        }
        private void Info(System.Web.UI.WebControls.Label lbl, string msg)
        {
            lbl.CssClass = "ms-2 text-info";
            lbl.Text = msg;
        }
    }
}
