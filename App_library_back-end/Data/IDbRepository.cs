using Microsoft.Data.Sqlite;

namespace App_library_back_end.Data
{
    public class IDbRepository
    {
        public void Initialize (string connectionString)
        {
            using var Connection = new SqliteConnection(connectionString);
            Connection.Open();

            var command = Connection.CreateCommand();

            command.CommandText = @"CREATE TABLE IF NOT EXISTS user (
                    UserID INTEGER PRIMARY KEY AUTOINCREMENT,
	                Name VARCHAR NOT NULL,
	                UserName VARCHAR NOT NULL UNIQUE,
	                Password VARCHAR NOT NULL,
	                Email VARCHAR,
	                DateOfBirth TEXT,
	                Gender VARCHAR,
	                PhoneNo VARCHAR,
	                Address VARCHAR,
	                UserType CHAR
                );
                CREATE TABLE IF NOT EXISTS book (
                    BookID INTEGER PRIMARY KEY AUTOINCREMENT,
	                Title VARCHAR NOT NULL,
	                Author VARCHAR NOT NULL,
	                Description TEXT,
	                Category VARCHAR,
	                Publisher VARCHAR,
	                PublishYear INTEGER
                );";
            command.ExecuteNonQuery();
        }
    }
}
