using Microsoft.Data.Sqlite;

namespace App_library_back_end.Data
{
    public class IDbRepesitory
    {
        public void Initialize (string connectionString)
        {
            using var Connection = new SqliteConnection(connectionString);
            Connection.Open();

            var command = Connection.CreateCommand();

            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS user (
                    UserID INTEGER PRIMARY KEY AUTOINCREMENT,
	                Name VARCHAR NOT NULL,
	                UserName VARCHAR NOT NULL UNIQUE,
	                Password VARCHAR NOT NULL,
	                Email VARCHAR,
	                DateOfBirth DATE,
	                Gender VARCHAR,
	                PhoneNo VARCHAR,
	                Address VARCHAR,
	                UserType CHAR
                );
";
            command.ExecuteNonQuery();


        }
    }
}
