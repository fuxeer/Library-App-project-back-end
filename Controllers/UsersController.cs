using App_library_back_end.Model;
using Microsoft.AspNetCore.Mvc;

namespace App_library_back_end.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        // Hardcoded test user
        private readonly User Faisal = new User()
        {
            UserName = "Faisal",
            Password = "Password",
            Name = "Fasal",
            Type = "Borrower"
        };

        // GET /api/users
        [HttpGet]
        public IActionResult GetUser()
        {
            return Ok(Faisal); // Return the full user object as JSON
        }

        // POST /api/users/login
        [HttpPost("login")]
        public IActionResult Login([FromBody] User user)
        {
            // Simple username/password check
            if (user.UserName == Faisal.UserName && user.Password == Faisal.Password)
            {
                var result = Faisal;
                result.Password = ""; // Hide password in response
                return Ok(result);   // Return JSON object
            }

            return Unauthorized(new { message = "Invalid username or password" });
        }
    }
}
