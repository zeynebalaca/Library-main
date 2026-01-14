using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace Library.Pages
{
    public partial class DashboardAdmin : Page
    {
        private string Cs => ConfigurationManager.ConnectionStrings["Db"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null) { Response.Redirect("~/Pages/Login.aspx"); return; }
            var role = Convert.ToString(Session["Role"] ?? "");
            if (!role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("~/Pages/Dashboard.Member.aspx"); return;
            }

            if (!IsPostBack)
            {
                LoadCards();
                LoadLastReviews();
                LoadLastLoans();
            }
        }

        private void LoadCards()
        {
            using (var conn = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(@"
                SELECT
                    (SELECT COUNT(*) FROM dbo.Books)                                                AS TotalBooks,
                    (SELECT COUNT(*) FROM dbo.BookCopies WHERE Status = 'Available')                AS AvailableCopies,
                    (SELECT COUNT(*) FROM dbo.Loans WHERE ReturnDate IS NULL)                       AS ActiveLoans,
                    (
                        ISNULL((SELECT COUNT(*) FROM dbo.Loans WHERE CONVERT(date, LoanDate)  = CONVERT(date, GETDATE())),0) +
                        ISNULL((SELECT COUNT(*) FROM dbo.Loans WHERE CONVERT(date, ReturnDate)= CONVERT(date, GETDATE())),0)
                    ) AS TodayOps;", conn))
            {
                conn.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        litTotalBooks.Text = rdr["TotalBooks"].ToString();
                        litAvailableCopies.Text = rdr["AvailableCopies"].ToString();
                        litActiveLoans.Text = rdr["ActiveLoans"].ToString();
                        litTodayOps.Text = rdr["TodayOps"].ToString();
                    }
                }
            }
        }

        private void LoadLastReviews()
        {
            using (var conn = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(@"
                SELECT TOP (5)
                       r.Id,
                       b.Title   AS BookTitle,
                       u.Name    AS UserName,
                       r.Rating,
                       r.Comment,
                       r.CreatedAt
                FROM dbo.Reviews r
                JOIN dbo.Users   u ON r.UserId = u.Id
                JOIN dbo.Books   b ON r.BookId = b.Id
                ORDER BY r.CreatedAt DESC, r.Id DESC;", conn))
            using (var da = new SqlDataAdapter(cmd))
            {
                var dt = new DataTable();
                da.Fill(dt);
                phNoReviews.Visible = dt.Rows.Count == 0;
                repLastReviews.DataSource = dt;
                repLastReviews.DataBind();
            }
        }

        private void LoadLastLoans()
        {
            using (var conn = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(@"
                SELECT TOP (5)
                       l.Id,
                       b.Title     AS BookTitle,
                       u.Name      AS UserName,
                       l.LoanDate,
                       l.DueDate
                FROM dbo.Loans l
                JOIN dbo.BookCopies bc ON l.BookCopyId = bc.Id
                JOIN dbo.Books      b ON bc.BookId     = b.Id
                JOIN dbo.Users      u ON l.UserId      = u.Id
                ORDER BY l.LoanDate DESC, l.Id DESC;", conn))
            using (var da = new SqlDataAdapter(cmd))
            {
                var dt = new DataTable();
                da.Fill(dt);
                phNoLoans.Visible = dt.Rows.Count == 0;
                repLastLoans.DataSource = dt;
                repLastLoans.DataBind();
            }
        }
    }
}
