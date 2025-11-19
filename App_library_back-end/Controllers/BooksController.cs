
using App_library_back_end.Data;
using App_library_back_end.Model;
using App_library_back_end.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace App_library_back_end.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly BookRepository _BookRepositroy;

        public BooksController(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            
            var dbRepo = new IDbRepository(connectionString);
            dbRepo.Initialize();

            _BookRepositroy = new BookRepository(connectionString);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBooks()
        {
            var books = await _BookRepositroy.GetAllBooks();
            return Ok(books);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBookById(int id)
        {
            var book = await _BookRepositroy.GetBookById(id);
            return book == null ? NotFound() : Ok(book);
        }

        [HttpPost]
        public async Task<IActionResult> AddBook([FromBody] Book book)
        {
            await _BookRepositroy.AddBook(book);
            return Ok(new { message = "Book added" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, [FromBody] Book book)
        {
            book.BookID = id;
            await _BookRepositroy.UpdateBook(book);
            return Ok(new { message = "Book updated" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            await _BookRepositroy.DeleteBook(id);
            return Ok(new { message = "Book deleted" });
        }
    }
}