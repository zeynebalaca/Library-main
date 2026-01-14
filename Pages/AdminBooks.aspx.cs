using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Library.Pages
{
    public partial class AdminBooks : Page
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
            {
                BindCategories();
                BindBooks();
            }
        }

        private void BindCategories()
        {
            using (var conn = new SqlConnection(Cs))
            using (var cmd = new SqlCommand("SELECT Id, Name FROM dbo.Categories ORDER BY Name;", conn))
            using (var da = new SqlDataAdapter(cmd))
            {
                var dt = new DataTable();
                da.Fill(dt);
                ddlCategory.DataSource = dt;
                ddlCategory.DataTextField = "Name";
                ddlCategory.DataValueField = "Id";
                ddlCategory.DataBind();
                ddlCategory.Items.Insert(0, new ListItem("-- seçin --", ""));
            }
        }

        private void BindBooks()
        {
            using (var conn = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(@"
SELECT b.Id, b.Title, b.Author, b.ISBN, b.CategoryId, c.Name AS CategoryName, b.CoverPath
FROM dbo.Books b
LEFT JOIN dbo.Categories c ON c.Id = b.CategoryId
ORDER BY b.Id DESC;", conn))
            using (var da = new SqlDataAdapter(cmd))
            {
                var dt = new DataTable();
                da.Fill(dt);
                repBooks.DataSource = dt;
                repBooks.DataBind();
            }
        }

        // =============================
        // KAPAK GÖRSELİ (ASPX ÇAĞIRIYOR)
        // =============================
        protected string CoverSrc(object path)
        {
            var url = Convert.ToString(path ?? "").Trim();
            if (string.IsNullOrEmpty(url))
                return ResolveUrl("~/Content/placeholder-book.png");

            return url.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                ? url
                : ResolveUrl(url);
        }

        // =============================
        // REPEATER SİLME BUTONU
        // =============================
        protected void repBooks_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "delete")
            {
                int id = Convert.ToInt32(e.CommandArgument);
                try
                {
                    DeleteBookDb(id);
                    Info("Silindi.");
                    BindBooks();
                }
                catch (Exception ex)
                {
                    Error("Silme hatası: " + ex.Message);
                }
            }
        }

        // =============================
        // KAYDET (EKLE / GÜNCELLE)
        // =============================
        protected void btnSave_Click(object sender, EventArgs e)
        {
            int id = 0;
            int.TryParse(hidEditId.Value, out id);

            string title = txtTitle.Text.Trim();
            string author = txtAuthor.Text.Trim();
            string isbn = txtIsbn.Text.Trim();
            int? categoryId = string.IsNullOrEmpty(ddlCategory.SelectedValue)
                ? null
                : (int?)Convert.ToInt32(ddlCategory.SelectedValue);

            try
            {
                // ISBN TEKRAR KONTROLÜ (SADECE EKLEMEDE)
                if (id == 0 && IsIsbnExists(isbn))
                {
                    Error("Bu ISBN numarasına ait kitap mevcut.");
                    return;
                }

                string coverPath = null;

                // GÖRSEL ZORUNLULUĞU (SADECE EKLEMEDE)
                if (id == 0 && !fuCover.HasFile)
                {
                    Error("Kitap görseli eklemek zorunludur.");
                    return;
                }

                if (fuCover.HasFile)
                {
                    string folder = Server.MapPath("~/Uploads/Books");
                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);

                    string ext = Path.GetExtension(fuCover.FileName).ToLower();
                    string fileName = Guid.NewGuid().ToString("N") + ext;
                    string path = Path.Combine(folder, fileName);
                    fuCover.SaveAs(path);
                    coverPath = "/Uploads/Books/" + fileName;
                }

                if (id == 0)
                    InsertBookDb(title, author, isbn, categoryId, coverPath);
                else
                    UpdateBookDb(id, title, author, isbn, categoryId, coverPath);

                Success("Kaydedildi.");
                BindBooks();
            }
            catch (Exception ex)
            {
                Error("Kaydetme hatası: " + ex.Message);
            }
        }

        // =============================
        // ISBN VAR MI?
        // =============================
        private bool IsIsbnExists(string isbn)
        {
            using (var conn = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM dbo.Books WHERE ISBN = @isbn", conn))
            {
                cmd.Parameters.AddWithValue("@isbn", isbn);
                conn.Open();
                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        private void InsertBookDb(string title, string author, string isbn, int? categoryId, string coverPath)
        {
            using (var conn = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(@"
INSERT INTO dbo.Books(Title, Author, ISBN, CategoryId, CoverPath)
VALUES(@t,@a,@i,@c,@p);", conn))
            {
                cmd.Parameters.AddWithValue("@t", (object)title ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@a", (object)author ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@i", (object)isbn ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@c", (object)categoryId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@p", (object)coverPath ?? DBNull.Value);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private void UpdateBookDb(int id, string title, string author, string isbn, int? categoryId, string coverPath)
        {
            using (var conn = new SqlConnection(Cs))
            {
                string sql = @"
UPDATE dbo.Books
SET Title=@t, Author=@a, ISBN=@i, CategoryId=@c"
                + (coverPath != null ? ", CoverPath=@p" : "")
                + " WHERE Id=@id;";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@t", (object)title ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@a", (object)author ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@i", (object)isbn ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@c", (object)categoryId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@id", id);

                    if (coverPath != null)
                        cmd.Parameters.AddWithValue("@p", (object)coverPath ?? DBNull.Value);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void DeleteBookDb(int id)
        {
            using (var conn = new SqlConnection(Cs))
            using (var cmd = new SqlCommand("DELETE FROM dbo.Books WHERE Id=@id;", conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private void Success(string m)
        {
            lblMsg.CssClass = "alert alert-success";
            lblMsg.Text = m;
        }

        private void Info(string m)
        {
            lblMsg.CssClass = "alert alert-info";
            lblMsg.Text = m;
        }

        private void Error(string m)
        {
            lblMsg.CssClass = "alert alert-danger";
            lblMsg.Text = m;
        }
    }
}
