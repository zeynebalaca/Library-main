using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;

namespace Library.Pages
{
    public partial class AdminCategories : Page
    {
        // API URL (portunu doğru yazdığın değer)
        private readonly string ApiUrl = "https://localhost:44327/api/categories";

        protected async void Page_Load(object sender, EventArgs e)
        {
            var role = Convert.ToString(Session["Role"] ?? "");
            if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("~/Pages/Login.aspx");
                return;
            }

            if (!IsPostBack)
                await BindCategories();
        }

        // ==========================================
        // GET - KATEGORI LİSTELEME
        // ==========================================
        private async Task BindCategories()
        {
            using (var client = new HttpClient())
            {
                var res = await client.GetAsync(ApiUrl);

                if (!res.IsSuccessStatusCode)
                {
                    Error("API kategori listesini alamadı.");
                    return;
                }

                var json = await res.Content.ReadAsStringAsync();
                var list = JsonConvert.DeserializeObject<List<CategoryDto>>(json);

                repCategories.DataSource = list;
                repCategories.DataBind();
            }
        }

        // ==========================================
        // POST - KATEGORİ EKLEME
        // ==========================================
        protected async void btnAdd_Click(object sender, EventArgs e)
        {
            string name = txtNewCategory.Text.Trim();

            if (string.IsNullOrEmpty(name))
            {
                Error("Kategori adı boş olamaz.");
                return;
            }

            var dto = new CategoryDto { Name = name };

            using (var client = new HttpClient())
            {
                var res = await client.PostAsJsonAsync(ApiUrl, dto);

                // Duplicate durumunu özel kontrol et
                if (res.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    Error("Bu kategori zaten mevcut.");
                    return;
                }

                if (!res.IsSuccessStatusCode)
                {
                    Error("API kategori ekleme hatası.");
                    return;
                }

                Success("Kategori eklendi.");
                txtNewCategory.Text = "";
                await BindCategories();
            }
        }

        // ==========================================
        // DELETE - KATEGORİ SİLME
        // ==========================================
        protected async void repCategories_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "delete")
            {
                int id = Convert.ToInt32(e.CommandArgument);

                using (var client = new HttpClient())
                {
                    var res = await client.DeleteAsync($"{ApiUrl}/{id}");

                    if (res.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        Error("Bu kategori başka tablolar tarafından kullanılıyor, silinemez.");
                        lblMsg.CssClass = "alert alert-danger d-block";

                        return;
                    }

                    if (!res.IsSuccessStatusCode)
                    {
                        Error("API kategori silme hatası.");
                        return;
                    }

                    Success("Kategori silindi.");
                    await BindCategories();
                }
            }
        }

        // ==========================================
        // PUT - KATEGORİ GÜNCELLEME
        // ==========================================
        protected async void btnSave_Click(object sender, EventArgs e)
        {
            int id = int.Parse(hidEditId.Value);
            string newName = txtEditName.Text.Trim();

            if (string.IsNullOrEmpty(newName))
            {
                Error("Geçerli bir kategori adı girin.");
                return;
            }

            var dto = new CategoryDto { Name = newName };

            using (var client = new HttpClient())
            {
                var res = await client.PutAsJsonAsync($"{ApiUrl}/{id}", dto);

                if (res.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    Error("Bu kategori zaten mevcut.");
                    return;
                }

                if (!res.IsSuccessStatusCode)
                {
                    Error("API kategori güncelleme hatası.");
                    return;
                }

                Success("Kategori güncellendi.");
                await BindCategories();
            }
        }

        // ==========================================
        // MESAJ FONKSİYONLARI
        // ==========================================
        private void Success(string m)
        {
            lblMsg.CssClass = "alert alert-success";
            lblMsg.Text = m;
        }

        private void Error(string m)
        {
            lblMsg.CssClass = "alert alert-danger";
            lblMsg.Text = m;
        }
    }

    // API’den gelen DTO
    public class CategoryDto
    {
        public int Id { get; set; }     // GET sırasında lazım
        public string Name { get; set; }
    }
}
