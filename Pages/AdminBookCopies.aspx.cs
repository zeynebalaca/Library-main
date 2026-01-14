using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Library.Pages
{
    public partial class AdminBookCopies : Page
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
                BindBooks();
        }

        private void BindBooks()
        {
            using (var conn = new SqlConnection(Cs))
            using (var cmd = new SqlCommand("SELECT Id, Title FROM dbo.Books ORDER BY Title;", conn))
            using (var da = new SqlDataAdapter(cmd))
            {
                var dt = new DataTable();
                da.Fill(dt);
                ddlBooks.DataSource = dt;
                ddlBooks.DataTextField = "Title";
                ddlBooks.DataValueField = "Id";
                ddlBooks.DataBind();
                ddlBooks.Items.Insert(0, new ListItem("-- Kitap Seçin --", ""));
            }
        }

        protected void btnList_Click(object sender, EventArgs e)
        {
            int bookId = string.IsNullOrEmpty(ddlBooks.SelectedValue) ? 0 : Convert.ToInt32(ddlBooks.SelectedValue);
            if (bookId == 0)
            {
                Error("Lütfen bir kitap seçin.");
                phTable.Visible = false;
                return;
            }
            BindCopies(bookId);
        }

        private void BindCopies(int bookId)
        {
            using (var conn = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(
                "SELECT Id, BookId, Status FROM dbo.BookCopies WHERE BookId=@b ORDER BY Id;", conn))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.AddWithValue("@b", bookId);
                var dt = new DataTable();
                da.Fill(dt);
                repCopies.DataSource = dt;
                repCopies.DataBind();
                phTable.Visible = true;
            }
        }

        protected void btnAdd_Click(object sender, EventArgs e)
        {
            int bookId = string.IsNullOrEmpty(ddlBooks.SelectedValue) ? 0 : Convert.ToInt32(ddlBooks.SelectedValue);
            if (bookId == 0)
            {
                Error("Önce bir kitap seçin.");
                return;
            }

            try
            {
                using (var conn = new SqlConnection(Cs))
                using (var cmd = new SqlCommand(
                    "INSERT INTO dbo.BookCopies(BookId, Status) VALUES(@b, 'Available');", conn))
                {
                    cmd.Parameters.AddWithValue("@b", bookId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                Success("Kopya eklendi.");
                BindCopies(bookId);
            }
            catch (Exception ex)
            {
                Error("Ekleme hatası: " + ex.Message);
            }
        }

        protected void repCopies_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "delete")
            {
                int id = Convert.ToInt32(e.CommandArgument);
                int bookId = string.IsNullOrEmpty(ddlBooks.SelectedValue) ? 0 : Convert.ToInt32(ddlBooks.SelectedValue);
                try
                {
                    using (var conn = new SqlConnection(Cs))
                    using (var cmd = new SqlCommand(
                        "DELETE FROM dbo.BookCopies WHERE Id=@id AND Status='Available';", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        conn.Open();
                        int n = cmd.ExecuteNonQuery();
                        if (n == 0)
                            Error("Kopya silinemedi (muhtemelen ödünçte).");
                        else
                            Success("Kopya silindi.");
                    }
                    BindCopies(bookId);
                }
                catch (Exception ex)
                {
                    Error("Silme hatası: " + ex.Message);
                }
            }
        }

        /* ---- Görsel yardımcılar ---- */
        protected string StatusBadge(object status)
        {
            string s = Convert.ToString(status ?? "").ToLower();
            if (s == "available") return "<span class='badge bg-success'>Uygun</span>";
            if (s == "loaned") return "<span class='badge bg-warning text-dark'>Ödünçte</span>";
            if (s == "lost") return "<span class='badge bg-danger'>Kayıp</span>";
            return "<span class='badge bg-secondary'>" + s + "</span>";
        }

        protected bool CanDelete(object status)
        {
            string s = Convert.ToString(status ?? "");
            return s.Equals("Available", StringComparison.OrdinalIgnoreCase);
        }

        protected string GetDeleteCss(object status)
        {
            return CanDelete(status)
                ? "btn btn-sm btn-outline-danger"
                : "btn btn-sm btn-outline-secondary";
        }

        /* ---- UI helpers ---- */
        private void Success(string m) { lblMsg.CssClass = "alert alert-success"; lblMsg.Text = m; }
        private new void Error(string m) { lblMsg.CssClass = "alert alert-danger"; lblMsg.Text = m; }
    }
}
