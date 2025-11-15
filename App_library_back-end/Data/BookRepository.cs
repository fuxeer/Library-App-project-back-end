
using App_library_back_end.Data;
using App_library_back_end.Model;
using Dapper;
using Microsoft.Data.Sqlite;

namespace App_library_back_end.Repository
{
    public class BookRepository
    {
        private readonly IDbRepository _db;

        public BookRepository()
        {
            _db = new IDbRepository();
            _db.Initialize(); // Ensure tables exist
        }

        private SqliteConnection GetConnection() => new SqliteConnection(_db.ConnectionString);

        public async Task<IEnumerable<Book>> GetAllBooks()
        {
            using var conn = GetConnection();
            return await conn.QueryAsync<Book>("SELECT * FROM book");
        }

        public async Task<Book?> GetBookById(int id)
        {
            using var conn = GetConnection();
            return await conn.QueryFirstOrDefaultAsync<Book>("SELECT * FROM book WHERE BookID=@id", new { id });
        }

        public async Task<int> AddBook(Book book)
        {
            using var conn = GetConnection();
            var sql = @"INSERT INTO book (Title, Author, Description, Category, Publisher, PublishYear)
                        VALUES (@Title, @Author, @Description, @Category, @Publisher, @PublishYear);";
            return await conn.ExecuteAsync(sql, book);
        }

        public async Task<int> UpdateBook(Book book)
        {
            using var conn = GetConnection();
            var sql = @"UPDATE book SET 
                        Title=@Title, Author=@Author, Description=@Description, Category=@Category, 
                        Publisher=@Publisher, PublishYear=@PublishYear
                        WHERE BookID=@BookID";
            return await conn.ExecuteAsync(sql, book);
        }

        public async Task<int> DeleteBook(int id)
        {
            using var conn = GetConnection();
            return await conn.ExecuteAsync("DELETE FROM book WHERE BookID=@id", new { id });
        }
    }
}