using System;

namespace App_library_back_end.Model
{
    public class User
    {
        public int UserID { get; set; }
        public string? Name { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Email { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? PhoneNo { get; set; }
        public string? Address { get; set; }
        public string? UserType { get; set; }

    }
}

