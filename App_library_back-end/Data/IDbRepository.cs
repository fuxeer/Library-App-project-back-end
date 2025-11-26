
using Microsoft.Data.Sqlite;

namespace App_library_back_end.Data
{
    public class IDbRepository
    {
        private readonly string _connectionString;
        public IDbRepository(string connection) 
        {
            _connectionString = connection;
        }
        public void Initialize ()
        {
            using var connection = new SqliteConnection(_connectionString);
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
    PublishYear INTEGER,
    Rating REAL
);

CREATE TABLE IF NOT EXISTS reserve (
    ReserveID INTEGER PRIMARY KEY AUTOINCREMENT,
    BorrowerID INTEGER,
    BookID INTEGER,
    ReserveDate TEXT,
    ReserveTime TEXT,
    StartDate TEXT,
    DueDate TEXT,
    ReserveStatus TEXT,
    FOREIGN KEY (BorrowerID) REFERENCES user(UserID),
    FOREIGN KEY (BookID) REFERENCES book(BookID)
);

CREATE TABLE IF NOT EXISTS copy (
    CopyID INTEGER PRIMARY KEY AUTOINCREMENT,
    BookID INTEGER,
    SerialNumber TEXT,
    CopyStatus TEXT,
    FOREIGN KEY (BookID) REFERENCES book(BookID)
);

CREATE TABLE IF NOT EXISTS rent (
    RentID INTEGER PRIMARY KEY AUTOINCREMENT,
    BorrowerID INTEGER,
    CopyID INTEGER,
    RentDate TEXT,
    RentTime TEXT,
    DueDate TEXT,
    RentStatus TEXT,
    FOREIGN KEY (BorrowerID) REFERENCES user(UserID),
    FOREIGN KEY (CopyID) REFERENCES copy(CopyID)
);

CREATE TABLE IF NOT EXISTS log (
    LogID INTEGER PRIMARY KEY AUTOINCREMENT,
    LibrarianID INTEGER,
    LogDate TEXT,
    LogTime TEXT,
    LogType TEXT,
    FOREIGN KEY (LibrarianID) REFERENCES user(UserID)
);

                CREATE UNIQUE INDEX IF NOT EXISTS idx_book_title_author ON book(Title, Author);

        /*Seed books safely
        INSERT OR IGNORE INTO book (Title, Author, Description, Category, Publisher, PublishYear, Rating) VALUES
        ('The Silent Forest', 'Emma Johnson', 'A thrilling journey into a mysterious forest.', 'Thriller', 'Maple Press', 2015, 4.2),
        ('Ocean Whispers', 'Liam Smith', 'A story of love and discovery by the sea.', 'Romance', 'BlueWave', 2018, 3.8),
        ('Quantum Secrets', 'Olivia Brown', 'Exploring the world of quantum physics for beginners.', 'Science', 'Nova Books', 2020, 4.7),
        ('The Last Kingdom', 'Noah Davis', 'A tale of medieval kingdoms and epic battles.', 'Historical Fiction', 'Royal Press', 2016, 4.1),
        ('Starlight Dreams', 'Ava Martinez', 'A young adult fantasy adventure in a magical land.', 'Fantasy', 'Skyline Publishing', 2019, 4.5),
        ('The Hidden Path', 'Ethan Wilson', 'A detective uncovers secrets in a quiet town.', 'Mystery', 'Mystic House', 2017, 4.0),
        ('Digital Frontier', 'Sophia Taylor', 'The rise of AI and technology in society.', 'Technology', 'TechWorld', 2021, 4.3),
        ('Whispers of the Past', 'Mason Anderson', 'A gripping story of memory and identity.', 'Drama', 'Heritage Press', 2014, 3.9),
        ('The Art of Cooking', 'Isabella Thomas', 'A complete guide to modern cuisine.', 'Cooking', 'Gourmet Books', 2015, 4.6),
        ('Celestial Nights', 'Lucas Moore', 'Romance and mystery under the stars.', 'Romance', 'Starry Ink', 2018, 4.2),
        ('The Forgotten Library', 'Charlotte Martin', 'A journey through lost books and hidden knowledge.', 'Fantasy', 'Maple Press', 2016, 4.4),
        ('Infinite Horizons', 'Henry Lee', 'Science fiction adventures across the galaxy.', 'Science Fiction', 'Nova Books', 2020, 4.8),
        ('Shadows of Time', 'Amelia Perez', 'A historical mystery spanning centuries.', 'Historical Fiction', 'Royal Press', 2017, 4.0),
        ('The Last Voyage', 'Benjamin White', 'A sea adventure filled with danger and courage.', 'Adventure', 'BlueWave', 2019, 3.9),
        ('Mindful Living', 'Mia Harris', 'Techniques and tips for a balanced life.', 'Self-Help', 'Wellness House', 2021, 4.5),
        ('Frozen Secrets', 'Daniel Clark', 'Mystery in the Arctic with hidden agendas.', 'Mystery', 'Mystic House', 2018, 4.1),
        ('Beyond the Stars', 'Harper Lewis', 'A young adult sci-fi adventure.', 'Science Fiction', 'Skyline Publishing', 2019, 4.6),
        ('Garden of Dreams', 'Alexander Walker', 'A touching drama about family and hope.', 'Drama', 'Heritage Press', 2016, 4.0),
        ('The Enchanted Forest', 'Evelyn Hall', 'Fantasy story of magic and wonder.', 'Fantasy', 'Starry Ink', 2017, 4.7),
        ('Secrets of the Mind', 'William Allen', 'Exploring psychology and human behavior.', 'Science', 'Nova Books', 2020, 4.3),
        ('The Lost Empire', 'Abigail Young', 'Epic historical battles and intrigue.', 'Historical Fiction', 'Royal Press', 2015, 4.2),
        ('Waves of Freedom', 'James King', 'Romance set on a tropical island.', 'Romance', 'BlueWave', 2018, 3.8),
        ('Cyber Shadows', 'Emily Scott', 'Thriller about hacking and cyber crime.', 'Thriller', 'TechWorld', 2021, 4.4),
        ('The Silent Melody', 'Oliver Adams', 'Drama about love, loss, and music.', 'Drama', 'Heritage Press', 2017, 4.1),
        ('Mystic Rivers', 'Sofia Baker', 'Fantasy adventure along magical rivers.', 'Fantasy', 'Maple Press', 2016, 4.5),
        ('Cooking with Passion', 'Jackson Gonzalez', 'Recipes for home chefs.', 'Cooking', 'Gourmet Books', 2019, 4.3),
        ('The Timekeeper', 'Ella Nelson', 'A thrilling sci-fi about time travel.', 'Science Fiction', 'Nova Books', 2020, 4.7),
        ('Hidden Truths', 'Sebastian Carter', 'Mystery unraveling dark family secrets.', 'Mystery', 'Mystic House', 2018, 4.0),
        ('Aurora Nights', 'Victoria Mitchell', 'Romance and adventure under the aurora.', 'Romance', 'Starry Ink', 2017, 4.2),
        ('Legends of the Realm', 'Matthew Perez', 'Epic fantasy with dragons and kings.', 'Fantasy', 'Skyline Publishing', 2015, 4.6),
        ('The Final Chapter', 'Grace Roberts', 'Thriller that keeps you on edge.', 'Thriller', 'Maple Press', 2019, 4.1),
        ('Galactic Quest', 'Henry Turner', 'Sci-fi exploration of distant planets.', 'Science Fiction', 'Nova Books', 2021, 4.8),
        ('Whispering Shadows', 'Chloe Phillips', 'Mystery in a sleepy town.', 'Mystery', 'Mystic House', 2016, 4.0),
        ('Romance in Paris', 'Jack Campbell', 'A love story in the city of lights.', 'Romance', 'BlueWave', 2018, 4.3),
        ('The Inventor''s Legacy', 'Lily Parker', 'Thriller and science adventure combined.', 'Thriller', 'TechWorld', 2020, 4.5),
        ('Dreams of Flight', 'Ethan Edwards', 'Drama about ambition and dreams.', 'Drama', 'Heritage Press', 2017, 4.2),
        ('The Crystal Cave', 'Avery Collins', 'Fantasy adventure in a magical cave.', 'Fantasy', 'Starry Ink', 2016, 4.7),
        ('Mind Over Matter', 'Ella Hughes', 'Self-help for mental strength and resilience.', 'Self-Help', 'Wellness House', 2021, 4.6),
        ('The Forgotten City', 'Logan Ward', 'Mystery and adventure in ancient ruins.', 'Mystery', 'Mystic House', 2018, 4.1),
        ('Celestial Dreams', 'Zoe Morris', 'Romance with a touch of fantasy.', 'Romance', 'Skyline Publishing', 2019, 4.4),
        ('Ancient Battles', 'Liam Cook', 'Historical fiction about legendary wars.', 'Historical Fiction', 'Royal Press', 2015, 4.2),
        ('Tech Revolution', 'Madison Bell', 'Technology and future innovations.', 'Technology', 'TechWorld', 2021, 4.7),
        ('The Silent Witness', 'Alexander Reed', 'Crime thriller with suspense.', 'Thriller', 'Maple Press', 2017, 4.1),
        ('Beyond Horizons', 'Aubrey Cooper', 'Science fiction adventures across galaxies.', 'Science Fiction', 'Nova Books', 2020, 4.8),
        ('Lost in the Woods', 'Ella Murphy', 'Mystery of a missing person in the forest.', 'Mystery', 'Mystic House', 2018, 4.0),
        ('Love Under the Moon', 'Mason Rivera', 'Romance set during magical nights.', 'Romance', 'BlueWave', 2017, 4.3),
        ('The Wizard''s Apprentice', 'Isla Foster', 'Fantasy tale of learning magic.', 'Fantasy', 'Starry Ink', 2016, 4.5),
        ('Healthy Living', 'James Brooks', 'Self-help guide for healthy lifestyle.', 'Self-Help', 'Wellness House', 2021, 4.6),
        ('Pirates of the Unknown', 'Harper Sanders', 'Adventure on the high seas.', 'Adventure', 'BlueWave', 2019, 4.2),
        ('The Quantum Enigma', 'Lucas Gray', 'Exploring mysteries of quantum physics.', 'Science', 'Nova Books', 2020, 4.7),
        ('Echoes of the Past', 'Charlotte Rivera', 'Historical fiction with secrets revealed.', 'Historical Fiction', 'Royal Press', 2015, 4.1),
        ('Moonlit Serenade', 'Benjamin Foster', 'Romance under moonlight.', 'Romance', 'Starry Ink', 2018, 4.4)
            */"

;
            command.ExecuteNonQuery();
        }
    }
}
