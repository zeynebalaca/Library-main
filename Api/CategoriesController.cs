using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Http;

namespace Library.Api
{
    public class CategoriesController : ApiController
    {
        private string Cs => ConfigurationManager.ConnectionStrings["Db"].ConnectionString;

        // =====================================
        // GET /api/categories  → Tüm kategoriler
        // =====================================
        [HttpGet]
        public IHttpActionResult GetAll()
        {
            var list = new List<object>();

            using (var conn = new SqlConnection(Cs))
            using (var cmd = new SqlCommand("SELECT Id, Name FROM dbo.Categories ORDER BY Name;", conn))
            {
                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        list.Add(new { Id = r.GetInt32(0), Name = r.GetString(1) });
                }
            }

            return Ok(list);
        }

        // =====================================
        // POST /api/categories  → Yeni kategori ekle
        // =====================================
        [HttpPost]
        public IHttpActionResult Create([FromBody] CategoryDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Kategori adı boş olamaz.");

            try
            {
                using (var conn = new SqlConnection(Cs))
                using (var cmd = new SqlCommand(
                    "INSERT INTO dbo.Categories(Name) VALUES(@n);", conn))
                {
                    cmd.Parameters.AddWithValue("@n", dto.Name.Trim());
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                return Ok(new { message = "Kategori eklendi." });
            }
            catch (SqlException ex)
            {
                // SQL Error 2627 / 2601 = Unique constraint violation (Duplicate Name)
                if (ex.Number == 2627 || ex.Number == 2601)
                {
                    return Content(
                        System.Net.HttpStatusCode.Conflict,
                        new { error = "Bu kategori zaten mevcut." }
                    );
                }

                // Diğer SQL hataları
                return InternalServerError(ex);
            }
        }

        // =====================================
        // PUT /api/categories/{id}  → Kategori güncelle
        // =====================================
        [HttpPut]
        public IHttpActionResult Update(int id, [FromBody] CategoryDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Geçerli bir ad girin.");

            try
            {
                using (var conn = new SqlConnection(Cs))
                using (var cmd = new SqlCommand(
                    "UPDATE dbo.Categories SET Name=@n WHERE Id=@id;", conn))
                {
                    cmd.Parameters.AddWithValue("@n", dto.Name.Trim());
                    cmd.Parameters.AddWithValue("@id", id);

                    conn.Open();
                    int affected = cmd.ExecuteNonQuery();

                    if (affected == 0)
                        return NotFound();
                }

                return Ok(new { message = "Kategori güncellendi." });
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601)
                {
                    return Content(
                        System.Net.HttpStatusCode.Conflict,
                        new { error = "Bu kategori zaten mevcut." }
                    );
                }

                return InternalServerError(ex);
            }
        }

        // =====================================
        // DELETE /api/categories/{id} → Kategori sil
        // =====================================
        [HttpDelete]
        public IHttpActionResult Delete(int id)
        {
            try
            {
                using (var conn = new SqlConnection(Cs))
                using (var cmd = new SqlCommand(
                    "DELETE FROM dbo.Categories WHERE Id=@id;", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                return Ok(new { message = "Kategori silindi." });
            }
            catch (SqlException ex)
            {
                
                if (ex.Number == 547)  // foreign key violation
                {
                    return Content(
                        System.Net.HttpStatusCode.Conflict,
                        new { error = "Bu kategori kullanıldığı için silinemez." }
                    );
                }

                return InternalServerError(ex);
            }
        }
    }

    public class CategoryDto
    {
        public string Name { get; set; }
    }
}
