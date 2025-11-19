
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