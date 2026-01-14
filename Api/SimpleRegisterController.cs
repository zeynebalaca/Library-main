using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Http;

namespace Library.Api
{
    public class SimpleRegisterController : ApiController
    {
        private string Cs => ConfigurationManager.ConnectionStrings["Db"].ConnectionString;

        // POST: /api/simpleregister
        [HttpPost]
        public IHttpActionResult Post(RegisterRequest model)
        {
            if (model == null ||
                string.IsNullOrEmpty(model.Name) ||
                string.IsNullOrEmpty(model.Email) ||
                string.IsNullOrEmpty(model.Password))
            {
                return BadRequest("Eksik bilgi");
            }

            using (var conn = new SqlConnection(Cs))
            {
                conn.Open();

                // E-posta var mı?
                using (var check = new SqlCommand(
                    "SELECT COUNT(*) FROM dbo.Users WHERE LOWER(Email)=LOWER(@e)", conn))
                {
                    check.Parameters.AddWithValue("@e", model.Email);
                    int exists = (int)check.ExecuteScalar();
                    if (exists > 0)
                        return Content(System.Net.HttpStatusCode.Conflict, "Bu e-posta zaten kayıtlı");
                }

                // Kayıt
                using (var cmd = new SqlCommand(@"
INSERT INTO dbo.Users (Name, Email, Password, Role)
VALUES (@n, @e, @p, 'Member');
SELECT SCOPE_IDENTITY();", conn))
                {
                    cmd.Parameters.AddWithValue("@n", model.Name);
                    cmd.Parameters.AddWithValue("@e", model.Email);
                    cmd.Parameters.AddWithValue("@p", model.Password);

                    int newId = Convert.ToInt32(cmd.ExecuteScalar());

                    return Ok(new
                    {
                        UserId = newId,
                        Name = model.Name,
                        Role = "Member"
                    });
                }
            }
        }
    }

    public class RegisterRequest
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
