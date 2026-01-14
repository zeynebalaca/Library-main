using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace Library.Pages
{
    public partial class DashboardMember : Page
    {
        private string Cs => ConfigurationManager.ConnectionStrings["Db"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Zorunlu oturum
            if (Session["UserId"] == null)
            {
                Response.Redirect("~/Pages/Login.aspx");
                return;
            }

            // Admin ise Admin Dashboard'a yönlendir (istersen kaldırabilirsin)
            var role = Convert.ToString(Session["Role"] ?? "");
            if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("~/Pages/DashboardAdmin.aspx");
                return;
            }

            if (!IsPostBack)
            {
                litMemberName.Text = Convert.ToString(Session["Name"]);
                LoadNewBooks();
                LoadTopRated();
                LoadMyActiveLoans();
            }
        }

        private void LoadNewBooks()
        {
            using (var conn = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(@"
                SELECT TOP (5) Id, Title
                FROM dbo.Books
                ORDER BY Id DESC;", conn))   // ⬅️ sadece en son eklenenler (Id büyükten küçüğe)
            using (var da = new SqlDataAdapter(cmd))
            {
                var dt = new DataTable();
                da.Fill(dt);
                phNoNewBooks.Visible = dt.Rows.Count == 0;
                repNewBooks.DataSource = dt;
                repNewBooks.DataBind();
            }
        }

        private void LoadTopRated()
        {
            using (var conn = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(@"
                SELECT TOP (5) Id, Title, ISNULL(AvgRating,0) AS AvgRating
                FROM dbo.Books
                ORDER BY ISNULL(AvgRating,0) DESC, Title ASC;", conn))
            using (var da = new SqlDataAdapter(cmd))
            {
                var dt = new DataTable();
                da.Fill(dt);
                phNoTopRated.Visible = dt.Rows.Count == 0;
                repTopRated.DataSource = dt;
                repTopRated.DataBind();
            }
        }

        private void LoadMyActiveLoans()
        {
            int userId = Convert.ToInt32(Session["UserId"]);

            using (var conn = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(@"
                SELECT l.Id,
                       b.Title AS BookTitle,
                       l.DueDate
                FROM dbo.Loans l
                JOIN dbo.BookCopies bc ON l.BookCopyId = bc.Id
                JOIN dbo.Books      b  ON bc.BookId    = b.Id
                WHERE l.UserId = @uid AND l.ReturnDate IS NULL
                ORDER BY l.DueDate ASC, l.Id DESC;", conn))
            {
                cmd.Parameters.AddWithValue("@uid", userId);

                using (var da = new SqlDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    phNoLoans.Visible = dt.Rows.Count == 0;
                    repMyLoans.DataSource = dt;
                    repMyLoans.DataBind();
                }
            }
        }
    }
}
