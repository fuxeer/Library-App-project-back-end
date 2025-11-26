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
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            var users = await _userRepository.GetAllUser();

            foreach (var u in users)
                u.Password = "";

            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUserById(int id)
        {
            var user = await _userRepository.GetUserByID(id);
            if (user == null)
                return NotFound($"No user found with ID = {id}");

            user.Password = "";
            return Ok(user);
        }

        [HttpPost]
        public async Task<ActionResult<User>> CreateUser(User newUser)
        {
            var createdUser = await _userRepository.CreateUser(newUser);
            createdUser.Password = "";

            return CreatedAtAction(nameof(GetUserById), new { id = createdUser.UserID }, createdUser);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateUser(int id, User updatedUser)
        {
            updatedUser.UserID = id; // Ensure ID matches route

            bool updated = await _userRepository.UpdateUser(id, updatedUser);
            if (!updated)
                return NotFound($"No user found with ID = {id}");

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            bool deleted = await _userRepository.DeleteUser(id);
            if (!deleted)
                return NotFound($"No user found with ID = {id}");

            return NoContent();
        }

        [HttpPost("login")]
        public async Task<ActionResult<User>> Login([FromBody] User login)
        {
            var user = await _userRepository.Login(login.UserName, login.Password);

            if (user == null)
                return Unauthorized("Invalid username or password");

            user.Password = "";
            return Ok(user);
        }
    }
}