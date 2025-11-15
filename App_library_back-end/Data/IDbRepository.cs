
using Microsoft.Data.Sqlite;

namespace App_library_back_end.Data
{
    public class IDbRepository
    {
        public string ConnectionString { get; private set; } = "Data Source=library.db";

        public void Initialize()
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS user (
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
                );
            ";
            command.ExecuteNonQuery();
        }
    }
}
