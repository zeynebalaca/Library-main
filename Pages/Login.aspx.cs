using System;
using System.Net.Http;
using System.Web.UI;
using Newtonsoft.Json;

namespace Library.Pages
{
    public partial class Login : Page
    {
        private const string ApiLoginUrl = "https://localhost:44327/api/simplelogin";
        private const string ApiRegisterUrl = "https://localhost:44327/api/simpleregister";

        private const string AdminDashboardUrl = "~/Pages/DashboardAdmin.aspx";
        private const string MemberDashboardUrl = "~/Pages/DashboardMember.aspx";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack && Session["UserId"] != null)
            {
                string role = Convert.ToString(Session["Role"] ?? "Member");
                Response.Redirect(
                    role.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                        ? AdminDashboardUrl
                        : MemberDashboardUrl,
                    true
                );
            }
        }

        // =========================
        // LOGIN
        // =========================
        protected void btnLogin_Click(object sender, EventArgs e)
        {
            lblMessage.Text = "";

            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (email == "" || password == "")
            {
                ShowError(lblMessage, "E-posta ve şifre zorunludur.");
                return;
            }

            try
            {
                using (var client = new HttpClient())
                {
                    var res = client.PostAsJsonAsync(ApiLoginUrl, new
                    {
                        Email = email,
                        Password = password
                    }).Result;

                    if (!res.IsSuccessStatusCode)
                    {
                        ShowError(lblMessage, "E-posta veya şifre hatalı.");
                        return;
                    }

                    dynamic user = JsonConvert.DeserializeObject(
                        res.Content.ReadAsStringAsync().Result);

                    Session["UserId"] = (int)user.UserId;
                    Session["Name"] = (string)user.Name;
                    Session["Role"] = (string)user.Role;

                    Response.Redirect(
                        ((string)user.Role).Equals("Admin", StringComparison.OrdinalIgnoreCase)
                            ? AdminDashboardUrl
                            : MemberDashboardUrl,
                        true
                    );
                }
            }
            catch (Exception ex)
            {
                ShowError(lblMessage, "Giriş hatası: " + ex.Message);
            }
        }

        // =========================
        // REGISTER
        // =========================
        protected void btnRegister_Click(object sender, EventArgs e)
        {
            lblRegisterMessage.Text = "";

            string name = txtRegName.Text.Trim();
            string email = txtRegEmail.Text.Trim();
            string password = txtRegPassword.Text.Trim();

            if (name == "" || email == "" || password == "")
            {
                ShowError(lblRegisterMessage, "Tüm alanlar zorunludur.");
                return;
            }

            try
            {
                using (var client = new HttpClient())
                {
                    var res = client.PostAsJsonAsync(ApiRegisterUrl, new
                    {
                        Name = name,
                        Email = email,
                        Password = password
                    }).Result;

                    if (res.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        ShowError(lblRegisterMessage, "Bu e-posta zaten kayıtlı.");
                        return;
                    }

                    if (!res.IsSuccessStatusCode)
                    {
                        ShowError(lblRegisterMessage, "Kayıt başarısız.");
                        return;
                    }

                    dynamic user = JsonConvert.DeserializeObject(
                        res.Content.ReadAsStringAsync().Result);

                    // 🔥 KAYIT SONRASI OTOMATİK LOGIN
                    Session["UserId"] = (int)user.UserId;
                    Session["Name"] = (string)user.Name;
                    Session["Role"] = (string)user.Role;

                    Response.Redirect(MemberDashboardUrl, true);
                }
            }
            catch (Exception ex)
            {
                ShowError(lblRegisterMessage, "Kayıt hatası: " + ex.Message);
            }
        }

        // =========================
        // UI helper
        // =========================
        private void ShowError(System.Web.UI.WebControls.Label lbl, string msg)
        {
            lbl.CssClass = "alert alert-danger";
            lbl.Text = msg;
        }
    }
}
