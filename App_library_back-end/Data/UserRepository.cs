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
            
            connection.Query<User>()
        }
    }
}
