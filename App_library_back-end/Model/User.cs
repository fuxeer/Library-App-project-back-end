using System;

namespace App_library_back_end.Model
{
    public class User
    {
        public int UserID { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public string Address { get; set; }
        public string UserType { get; set; }
        public string PhoneNo { get; set; }



    }
}

