using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace Library.Pages
{
    public partial class Books : Page
    {
        private string Cs => ConfigurationManager.ConnectionStrings["Db"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Oturum kontrolü
            if (Session["UserId"] == null)
            {
                Response.Redirect("~/Pages/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadCategories();
                LoadBooks(); // varsayılan: tüm kitaplar (son eklenenler önce)
            }
        }

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            LoadBooks();
        }

        private void LoadCategories()
        {
            ddlCategory.Items.Clear();
            ddlCategory.Items.Add(new System.Web.UI.WebControls.ListItem("Kategori (hepsi)", "0"));

            using (var conn = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(@"
                SELECT Id, Name
                FROM dbo.Categories
                ORDER BY Name ASC;", conn))
            using (var da = new SqlDataAdapter(cmd))
            {
                var dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow r in dt.Rows)
                {
                    ddlCategory.Items.Add(
                        new System.Web.UI.WebControls.ListItem(
                            Convert.ToString(r["Name"]),
                            Convert.ToString(r["Id"])
                        )
                    );
                }
            }
        }

        private void LoadBooks()
        {
            string q = (txtQ.Text ?? "").Trim();
            int catId = 0;
            int.TryParse(ddlCategory.SelectedValue ?? "0", out catId);

            using (var conn = new SqlConnection(Cs))
            using (var cmd = new SqlCommand())
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.Connection = conn;

                // Başlık + Yazar araması, kategori filtresi
                cmd.CommandText = @"
SELECT b.Id, b.Title, b.Author, b.CoverPath
FROM dbo.Books b
WHERE (@q = N'' OR b.Title LIKE @qLike OR b.Author LIKE @qLike)
  AND (@cat = 0 OR b.CategoryId = @cat)
ORDER BY b.Id DESC;";

                cmd.Parameters.AddWithValue("@q", q);
                cmd.Parameters.AddWithValue("@qLike", "%" + q + "%");
                cmd.Parameters.AddWithValue("@cat", catId);

                var dt = new DataTable();
                da.Fill(dt);

                phNoBooks.Visible = dt.Rows.Count == 0;
                repBooks.DataSource = dt;
                repBooks.DataBind();
            }
        }

        // Repeater içinde kapak yolunu güvenle hesapla
        protected string CoverSrc(object coverPathObj)
        {
            var url = Convert.ToString(coverPathObj ?? "").Trim();
            if (string.IsNullOrEmpty(url))
                return ResolveUrl("~/Content/placeholder-book.png");

            // Mutlak URL (http/https) veya yerel sanal yol olabilir — aynen döndür.
            return url.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                ? url
                : ResolveUrl(url);
        }
    }
}
