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

        public IEnumerable<User> GetAllUser()
        {
            using var connection = new SqliteConnection(_connectionString);


           return connection.Query<User>("SELECT * FROM user");
        }

        public User CreateUser(User newuser)
        {
            using var connection = new SqliteConnection(_connectionString);
            string sql = @"
                INSERT INTO user 
                (Name, UserName, Password, Email, DateOfBirth, Gender, PhoneNo, Address, UserType)
                VALUES 
                (@Name, @UserName, @Password, @Email, @DateOfBirth, @Gender, @PhoneNo, @Address, @UserType);
                SELECT last_insert_rowid();
            ";
            var id = connection.ExecuteScalar<long>(sql, newuser);
            newuser.UserID = (int)id;

            return newuser;
        }

        public User? GetUserByID(int id)
        {
            using var connection = new SqliteConnection(_connectionString);

            return connection.QueryFirstOrDefault<User>(
                "SELECT * FROM user WHERE UserID = @Id;",
                new { Id = id });
        }

        public bool UpdateUser(int Id , User updatedUser)
        {
            using var connection = new SqliteConnection(_connectionString);
            var sql  = @"
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

            int rows = connection.Execute(sql, updatedUser);
            return rows > 0;
        }

        public bool DeleteUser(int id) 
        {
            using var connectoin = new SqliteConnection(_connectionString);
            int rows = connectoin.Execute("DELETE FROM user WHERE UserID = @Id;", new { Id = id });

            return rows > 0;
        }

        public User? Login(string username, string password)
        {
            using var connection = new SqliteConnection(_connectionString);

            string sql = "SELECT * FROM user WHERE UserName = @UserName AND Password = @Password;";

            return connection.QueryFirstOrDefault<User>(sql , new {UserName = username , Password = password});
        }
    }
}
