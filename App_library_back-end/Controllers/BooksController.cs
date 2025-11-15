/*using App_library_back_end.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
namespace App_library_back_end.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly string _connectionString;

        public BooksController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            InitializeDatabase();
        }
        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS book (
                    BookID INTEGER PRIMARY KEY AUTOINCREMENT,
	                Title VARCHAR NOT NULL,
	                Author VARCHAR NOT NULL,
	                Description TEXT,
	                Category VARCHAR,
	                Publisher VARCHAR,
	                PublishYear YEAR
                );
            ";
            command.ExecuteNonQuery();
        }
        
        [HttpGet]
        public ActionResult<IEnumerable<Book>> GetAllBooks()
        {
            var books = new List<Book>();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT BookID, Title, Author, Description, Category, Publisher, PublishYear
                FROM book;
            ";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                books.Add(new Book
                {
                    BookID = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Author = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                    Category = reader.IsDBNull(4) ? null : reader.GetString(4),
                    Publisher = reader.IsDBNull(5) ? null : reader.GetString(5),
                    PublishYear = reader.IsDBNull(6) ? 0 : reader.GetInt32(6)
                });
            }

            return Ok(books);
        }

    }
}
*/
