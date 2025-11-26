using Microsoft.Data.Sqlite;
using Dapper;

using App_library_back_end.Model;

namespace App_library_back_end.Data
{
    public class UserRepository
    {
        private readonly string _connectionString;

        public UserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<User>> GetAllUser()
        {
            using var connection = new SqliteConnection(_connectionString);


            return await connection.QueryAsync<User>("SELECT * FROM user");
        }

        public async Task<User> CreateUser(User newuser)
        {
            using var connection = new SqliteConnection(_connectionString);
            string sql = @"
                INSERT INTO user 
                (Name, UserName, Password, Email, DateOfBirth, Gender, PhoneNo, Address, UserType)
                VALUES 
                (@Name, @UserName, @Password, @Email, @DateOfBirth, @Gender, @PhoneNo, @Address, @UserType);
                SELECT last_insert_rowid();
            ";
            var id = await connection.ExecuteScalarAsync<long>(sql, newuser);
            newuser.UserID = (int)id;

            return newuser;
        }

        public async Task<User?> GetUserByID(int id)
        {
            using var connection = new SqliteConnection(_connectionString);

            return await connection.QueryFirstOrDefaultAsync<User>(
                "SELECT * FROM user WHERE UserID = @Id;",
                new { Id = id });
        }

        public async Task<bool> UpdateUser(int Id, User updatedUser)
        {
            using var connection = new SqliteConnection(_connectionString);
            var sql = @"
                UPDATE user SET
                    Name = @Name,
                    UserName = @UserName,
                    Password = @Password,
                    Email = @Email,
                    DateOfBirth = @DateOfBirth,
                    Gender = @Gender,
                    PhoneNo = @PhoneNo,
                    Address = @Address,
                    UserType = @UserType
                WHERE UserID = @UserID;
            ";

            int rows = await connection.ExecuteAsync(sql, updatedUser);
            return rows > 0;
        }

        public async Task<bool> DeleteUser(int id)
        {
            using var connectoin = new SqliteConnection(_connectionString);
            int rows = await connectoin.ExecuteAsync("DELETE FROM user WHERE UserID = @Id;", new { Id = id });

            return rows > 0;
        }

        public async Task<User?> Login(string username, string password)
        {
            using var connection = new SqliteConnection(_connectionString);

            string sql = "SELECT * FROM user WHERE UserName = @UserName AND Password = @Password;";

            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { UserName = username, Password = password });
        }
    }
}