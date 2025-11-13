namespace App_library_back_end.Model
{
    public class User
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string UserName { get; set; } =string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? PhoneNo { get; set; }
        public string? Type { get; set; }

      

    }
    
}
