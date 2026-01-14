using System;
using System.Web.UI;

namespace Library.Master
{
    public partial class SiteMaster : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Oturumdan kullanıcı adı ve rol oku
            var name = Convert.ToString(Session["Name"] ?? "");
            var role = Convert.ToString(Session["Role"] ?? "");

            lblUserName.Text = string.IsNullOrEmpty(name) ? "" : name;

            // Admin menüsünün görünürlüğü
            adminMenu.Visible = role.Equals("Admin", StringComparison.OrdinalIgnoreCase);
        }

        protected void btnLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("~/Pages/Login.aspx");
        }
    }
}
