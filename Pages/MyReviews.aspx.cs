using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Library.Pages
{
    public partial class MyReviews : Page
    {
        private string Cs => ConfigurationManager.ConnectionStrings["Db"].ConnectionString;
        private string ApiBase => Convert.ToString(ConfigurationManager.AppSettings["ApiBaseUrl"] ?? "").Trim().TrimEnd('/');

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null) { Response.Redirect("~/Pages/Login.aspx"); return; }
            if (!IsPostBack) BindReviews();
        }

        // API DTO (UpdatedAt yok)
        class ReviewDto
        {
            public int ReviewId { get; set; }
            public int BookId { get; set; }
            public string BookTitle { get; set; }
            public int Rating { get; set; }
            public string Comment { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        private void BindReviews()
        {
            try
            {
                DataTable dt = string.IsNullOrEmpty(ApiBase) ? GetReviewsFromDb() : GetReviewsFromApi();
                phEmpty.Visible = dt.Rows.Count == 0;
                repReviews.DataSource = dt;
                repReviews.DataBind();
            }
            catch (Exception ex)
            {
                Error("Yorumlar yüklenemedi: " + ex.Message);
            }
        }

        // --- API (opsiyonel) ---
        private DataTable GetReviewsFromApi()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(ApiBase);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var resp = client.GetAsync("/api/reviews/mine").Result;
                resp.EnsureSuccessStatusCode();

                var json = resp.Content.ReadAsStringAsync().Result;
                var list = new JavaScriptSerializer().Deserialize<ReviewDto[]>(json);

                var dt = new DataTable();
                dt.Columns.Add("ReviewId", typeof(int));
                dt.Columns.Add("BookId", typeof(int));
                dt.Columns.Add("Title", typeof(string));
                dt.Columns.Add("Rating", typeof(int));
                dt.Columns.Add("Comment", typeof(string));
                dt.Columns.Add("CreatedAt", typeof(DateTime));

                foreach (var r in list)
                    dt.Rows.Add(r.ReviewId, r.BookId, r.BookTitle, r.Rating, r.Comment, r.CreatedAt);

                return dt;
            }
        }

        // --- DB ---
        private DataTable GetReviewsFromDb()
        {
            int userId = Convert.ToInt32(Session["UserId"]);
            using (var conn = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(@"
SELECT r.Id AS ReviewId, r.BookId, b.Title, r.Rating, r.Comment, r.CreatedAt
FROM dbo.Reviews r
JOIN dbo.Books b ON b.Id = r.BookId
WHERE r.UserId = @uid
ORDER BY r.CreatedAt DESC, r.Id DESC;", conn))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.AddWithValue("@uid", userId);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // Repeater render
        protected void repReviews_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem) return;

            var row = (DataRowView)e.Item.DataItem;

            ((Literal)e.Item.FindControl("litBookTitle")).Text = Convert.ToString(row["Title"]);
            ((Literal)e.Item.FindControl("litComment")).Text = Server.HtmlEncode(Convert.ToString(row["Comment"] ?? ""));
            ((Literal)e.Item.FindControl("litStars")).Text = GetStars(Convert.ToInt32(row["Rating"]));

            DateTime created = row["CreatedAt"] == DBNull.Value ? DateTime.MinValue : (DateTime)row["CreatedAt"];
            ((Literal)e.Item.FindControl("litDates")).Text = created == DateTime.MinValue ? "" : ("Oluşturma: " + created.ToString("dd.MM.yyyy HH:mm"));
        }

        private string GetStars(int rating)
        {
            if (rating < 0) rating = 0; if (rating > 5) rating = 5;
            return new string('★', rating).PadRight(5, '☆');
        }

        // Sadece SİL komutu postback ile gelir
        protected void repReviews_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "delete")
            {
                int reviewId = Convert.ToInt32(e.CommandArgument);
                try
                {
                    if (string.IsNullOrEmpty(ApiBase)) DeleteReviewDb(reviewId);
                    else DeleteReviewApi(reviewId);

                    Success("Yorum silindi.");
                    BindReviews();
                }
                catch (Exception ex)
                {
                    Error("Silme sırasında hata: " + ex.Message);
                }
            }
        }

        // KAYDET → DB güncelle
        protected void btnSave_Click(object sender, EventArgs e)
        {
            int reviewId = 0; int.TryParse(hidEditReviewId.Value, out reviewId);
            int rating = 5; int.TryParse(ddlRating.SelectedValue, out rating);
            string comment = (txtComment.Text ?? "").Trim();

            try
            {
                if (string.IsNullOrEmpty(ApiBase)) UpdateReviewDb(reviewId, rating, comment);
                else UpdateReviewApi(reviewId, rating, comment);

                Success("Yorum güncellendi.");
                BindReviews();
            }
            catch (Exception ex)
            {
                Error("Güncelleme sırasında hata: " + ex.Message);
            }
        }

        // --- API update/delete (opsiyonel) ---
        private void UpdateReviewApi(int reviewId, int rating, string comment)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(ApiBase);
                var payload = new { rating = rating, comment = comment };
                string json = new JavaScriptSerializer().Serialize(payload);
                var req = new HttpRequestMessage(HttpMethod.Put, "/api/reviews/" + reviewId);
                req.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var resp = client.SendAsync(req).Result;
                resp.EnsureSuccessStatusCode();
            }
        }
        private void DeleteReviewApi(int reviewId)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(ApiBase);
                var req = new HttpRequestMessage(HttpMethod.Delete, "/api/reviews/" + reviewId);
                var resp = client.SendAsync(req).Result;
                resp.EnsureSuccessStatusCode();
            }
        }

        // --- DB update/delete ---
        private void UpdateReviewDb(int reviewId, int rating, string comment)
        {
            int userId = Convert.ToInt32(Session["UserId"]);
            using (var conn = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(@"
UPDATE dbo.Reviews
   SET Rating=@r, Comment=@c
 WHERE Id=@id AND UserId=@uid;", conn))
            {
                cmd.Parameters.AddWithValue("@r", rating);
                cmd.Parameters.AddWithValue("@c", (object)comment ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@id", reviewId);
                cmd.Parameters.AddWithValue("@uid", userId);
                conn.Open();
                int n = cmd.ExecuteNonQuery();
                if (n == 0) throw new InvalidOperationException("Kayıt bulunamadı veya yetkiniz yok.");
            }
        }

        private void DeleteReviewDb(int reviewId)
        {
            int userId = Convert.ToInt32(Session["UserId"]);
            using (var conn = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(@"DELETE FROM dbo.Reviews WHERE Id=@id AND UserId=@uid;", conn))
            {
                cmd.Parameters.AddWithValue("@id", reviewId);
                cmd.Parameters.AddWithValue("@uid", userId);
                conn.Open();
                int n = cmd.ExecuteNonQuery();
                if (n == 0) throw new InvalidOperationException("Kayıt bulunamadı veya yetkiniz yok.");
            }
        }

        private void Success(string msg) { lblMsg.CssClass = "alert alert-success"; lblMsg.Text = msg; }
        private void Error(string msg) { lblMsg.CssClass = "alert alert-danger"; lblMsg.Text = msg; }
    }
}
