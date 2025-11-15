/*using App_library_back_end.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace App_library_back_end.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private string HashPassword(string password)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private readonly string _connectionString;

        public UsersController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            InitializeDatabase();
        }

        private void InitializeDatabase()
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
                );";
            command.ExecuteNonQuery();
        }

        [HttpGet]
        public ActionResult<IEnumerable<User>> GetAllUsers()
        {
            var users = new List<User>();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT UserID, Name, UserName, Password, Email, DateOfBirth, Gender, PhoneNo, Address, UserType
                FROM user;
            ";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                users.Add(new User
                {
                    UserID = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    UserName = reader.GetString(2),
                    Password = reader.GetString(3),
                    Email = reader.IsDBNull(4) ? null : reader.GetString(4),
                    DateOfBirth = reader.IsDBNull(5)
                        ? default
                        : DateOnly.FromDateTime(DateTime.Parse(reader.GetString(5))),
                    Gender = reader.GetString(6),
                    PhoneNo = reader.IsDBNull(7) ? null : reader.GetString(7),
                    Address = reader.IsDBNull(8) ? null : reader.GetString(8),
                    UserType = reader.IsDBNull(9) ? null : reader.GetString(9)
                });
            }

            foreach (var u in users)
                u.Password = null;

            return Ok(users);
        }

        [HttpGet("{id}")]
        public ActionResult<User> GetUserById(int id)
        {
            User user = null;

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT UserID, Name, UserName, Password, Email, DateOfBirth, Gender, PhoneNo, Address, UserType
                FROM user
                WHERE UserID = $id;
            ";

            command.Parameters.AddWithValue("$id", id);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                user = new User
                {
                    UserID = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    UserName = reader.GetString(2),
                    Password = reader.GetString(3),
                    Email = reader.IsDBNull(4) ? null : reader.GetString(4),
                    DateOfBirth = reader.IsDBNull(5)
                        ? default
                        : DateOnly.FromDateTime(DateTime.Parse(reader.GetString(5))),
                    Gender = reader.GetString(6),
                    PhoneNo = reader.IsDBNull(7) ? null : reader.GetString(7),
                    Address = reader.IsDBNull(8) ? null : reader.GetString(8),
                    UserType = reader.IsDBNull(9) ? null : reader.GetString(9)
                };
            }

            if (user == null)
                return NotFound($"No user found with ID = {id}");

            return Ok(user);
        }

        [HttpPost]
        public ActionResult<User> CreateUser(User newUser)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();

            // !!!!!!!!!!!!!!!!!!!!!!!!!!!
            // نكتب أمر SQL لإضافة مستخدم
            // ثم نطلب آخر ID تم إدخاله (SQLite يوفّره تلقائيًا)
            command.CommandText = @"
                INSERT INTO User (Name, UserName, Password, Email, DateOfBirth, Gender, PhoneNo, Address, UserType)
                VALUES ($name, $userName, $password, $email, $dob, $gender, $phone, $address, $userType);

                SELECT last_insert_rowid();
            ";

            command.Parameters.AddWithValue("$name", newUser.Name);

            command.Parameters.AddWithValue("$userName",
                string.IsNullOrWhiteSpace(newUser.UserName) ? (object)DBNull.Value : newUser.UserName);

            string hashedPassword = HashPassword(newUser.Password);
            command.Parameters.AddWithValue("$password", hashedPassword);

            command.Parameters.AddWithValue("$email",
                string.IsNullOrWhiteSpace(newUser.Email) ? (object)DBNull.Value : newUser.Email);

            command.Parameters.AddWithValue(
                "$dob",
                newUser.DateOfBirth == default
                    ? (object)DBNull.Value
                    : newUser.DateOfBirth
                          .ToDateTime(TimeOnly.MinValue)
                          .ToString("yyyy-MM-dd")
            );

            command.Parameters.AddWithValue("$gender",
                string.IsNullOrWhiteSpace(newUser.Gender) ? (object)DBNull.Value : newUser.Gender);

            command.Parameters.AddWithValue("$phone",
                string.IsNullOrWhiteSpace(newUser.PhoneNo) ? (object)DBNull.Value : newUser.PhoneNo);

            command.Parameters.AddWithValue("$address",
                string.IsNullOrWhiteSpace(newUser.Address) ? (object)DBNull.Value : newUser.Address);

            command.Parameters.AddWithValue("$userType",
                string.IsNullOrWhiteSpace(newUser.UserType) ? (object)DBNull.Value : newUser.UserType);

            long inserted = (long)command.ExecuteScalar();
            newUser.UserID = (int)inserted;

            return CreatedAtAction(nameof(GetAllUsers), new { id = newUser.UserID }, newUser);
        }

        [HttpPut("{id}")]
        public ActionResult UdateUser(int id, User UpdatedUser)
        {
            // نتأكد أن الجدول موجود (لو موجود ما راح يسوي شيء)
            InitializeDatabase();

            //فتح اتصال بالداتا بيس
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            //نتاكد انه موجود عشان نحدثه
            var CheckCommand = connection.CreateCommand();
            CheckCommand.CommandText = "SELECT COUNT(*) FROM User WHERE UserID = $id;";
            CheckCommand.Parameters.AddWithValue("$id", id);

            long count = (long)CheckCommand.ExecuteScalar();
            if (count == 0)
                return NotFound($"No user found with ID={id}");

            //ننشئ امر التحديث
            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE User
                SET
                    Name        = $name,
                    UserName    = $userName,
                    Password    = $password,
                    Email       = $email,
                    DateOfBirth = $dob,
                    Gender      = $gender,
                    PhoneNo     = $phone,
                    Address     = $address,
                    UserType    = $userType
                WHERE UserID = $id;
            ";

            command.Parameters.AddWithValue("$id", id);
            command.Parameters.AddWithValue("$name", UpdatedUser.Name);

            command.Parameters.AddWithValue("$userName",
                string.IsNullOrWhiteSpace(UpdatedUser.UserName) ? (object)DBNull.Value : UpdatedUser.UserName);

            if (string.IsNullOrWhiteSpace(UpdatedUser.Password))
            {
                command.Parameters.AddWithValue("$password", DBNull.Value);
            }
            else
            {
                string hashedPassword = HashPassword(UpdatedUser.Password);
                command.Parameters.AddWithValue("$password", hashedPassword);
            }

            command.Parameters.AddWithValue(
                "$email",
                string.IsNullOrWhiteSpace(UpdatedUser.Email) ? (object)DBNull.Value : UpdatedUser.Email);

            command.Parameters.AddWithValue(
                "$dob",
                UpdatedUser.DateOfBirth == default
                    ? (object)DBNull.Value
                    : UpdatedUser.DateOfBirth
                          .ToDateTime(TimeOnly.MinValue)
                          .ToString("yyyy-MM-dd")
            );

            command.Parameters.AddWithValue(
                "$gender",
                string.IsNullOrWhiteSpace(UpdatedUser.Gender) ? (object)DBNull.Value : UpdatedUser.Gender);

            command.Parameters.AddWithValue(
                "$phone",
                string.IsNullOrWhiteSpace(UpdatedUser.PhoneNo) ? (object)DBNull.Value : UpdatedUser.PhoneNo);

            command.Parameters.AddWithValue(
                "$address",
                string.IsNullOrWhiteSpace(UpdatedUser.Address) ? (object)DBNull.Value : UpdatedUser.Address);

            command.Parameters.AddWithValue(
                "$userType",
                string.IsNullOrWhiteSpace(UpdatedUser.UserType) ? (object)DBNull.Value : UpdatedUser.UserType);

            command.ExecuteNonQuery();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public ActionResult DeleteUser(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            // اول شيء نتأكد إن اليوزر موجود
            var checkCommand = connection.CreateCommand();
            checkCommand.CommandText = "SELECT COUNT(*) FROM user WHERE UserID = $id;";
            checkCommand.Parameters.AddWithValue("$id", id);

            long count = (long)checkCommand.ExecuteScalar();

            if (count == 0)
                return NotFound($"No user found with ID = {id}");

            // نسوي أمر الحذف
            var deleteCommand = connection.CreateCommand();
            deleteCommand.CommandText = "DELETE FROM user WHERE UserID = $id;";
            deleteCommand.Parameters.AddWithValue("$id", id);

            deleteCommand.ExecuteNonQuery();

            return NoContent(); // 204  
        }
    }
}

*/
using App_library_back_end.Data;
using App_library_back_end.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace App_library_back_end.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserRepository _userRepository;

        public UsersController(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Initialize database (tables)
            var dbRepo = new IDbRepository(connectionString);
            dbRepo.Initialize();

            // Initialize user repository
            _userRepository = new UserRepository(connectionString);
        }

        [HttpGet]
        public ActionResult<IEnumerable<User>> GetAllUsers()
        {
            var users = _userRepository.GetAllUser();

            foreach (var u in users)
                u.Password = null;

            return Ok(users);
        }

        [HttpGet("{id}")]
        public ActionResult<User> GetUserById(int id)
        {
            var user = _userRepository.GetUserByID(id);
            if (user == null)
                return NotFound($"No user found with ID = {id}");

            user.Password = null;
            return Ok(user);
        }

        [HttpPost]
        public ActionResult<User> CreateUser(User newUser)
        {
            var createdUser = _userRepository.CreateUser(newUser);
            createdUser.Password = null;

            return CreatedAtAction(nameof(GetUserById), new { id = createdUser.UserID }, createdUser);
        }

        [HttpPut("{id}")]
        public ActionResult UpdateUser(int id, User updatedUser)
        {
            updatedUser.UserID = id; // Ensure ID matches route

            bool updated = _userRepository.UpdateUser(id, updatedUser);
            if (!updated)
                return NotFound($"No user found with ID = {id}");

            return NoContent();
        }

        [HttpDelete("{id}")]
        public ActionResult DeleteUser(int id)
        {
            bool deleted = _userRepository.DeleteUser(id);
            if (!deleted)
                return NotFound($"No user found with ID = {id}");

            return NoContent();
        }

        [HttpPost("login")]
        public ActionResult<User> Login([FromBody] User login)
        {
            var user = _userRepository.Login(login.UserName, login.Password);

            if (user == null)
                return Unauthorized("Invalid username or password");

            user.Password = null;
            return Ok(user);
        }
    }
}