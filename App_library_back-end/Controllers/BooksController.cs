
using App_library_back_end.Model;
using App_library_back_end.Repository;
using Microsoft.AspNetCore.Mvc;

namespace App_library_back_end.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly BookRepository _repo;

        public BooksController()
        {
            _repo = new BookRepository(); // No DI required
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBooks()
        {
            var books = await _repo.GetAllBooks();
            return Ok(books);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBookById(int id)
        {
            var book = await _repo.GetBookById(id);
            return book == null ? NotFound() : Ok(book);
        }

        [HttpPost]
        public async Task<IActionResult> AddBook([FromBody] Book book)
        {
            await _repo.AddBook(book);
            return Ok(new { message = "Book added" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, [FromBody] Book book)
        {
            book.BookID = id;
            await _repo.UpdateBook(book);
            return Ok(new { message = "Book updated" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            await _repo.DeleteBook(id);
            return Ok(new { message = "Book deleted" });
        }
    }
}