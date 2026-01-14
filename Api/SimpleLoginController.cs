using System.Configuration;
using System.Data.SqlClient;
using System.Web.Http;

namespace Library.Api
{
    public class SimpleLoginController : ApiController
    {
        private string Cs => ConfigurationManager.ConnectionStrings["Db"].ConnectionString;

        // POST: /api/simplelogin
        [HttpPost]
        public IHttpActionResult Post(LoginRequest model)
        {
            if (model == null || string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
                return BadRequest("Eksik bilgi");

            using (var conn = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(@"
                SELECT TOP 1 Id, Name, Role
                FROM dbo.Users
                WHERE LOWER(Email) = LOWER(@e) AND Password = @p", conn))
            {
                cmd.Parameters.AddWithValue("@e", model.Email);
                cmd.Parameters.AddWithValue("@p", model.Password);

                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    if (!r.Read())
                        return Unauthorized();

                    return Ok(new
                    {
                        UserId = r.GetInt32(0),
                        Name = r.GetString(1),
                        Role = r.GetString(2)
                    });
                }
            }
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
