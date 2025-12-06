namespace App_library_back_end.Model
{
    public class PartialUserUpdate
    {
        public int UserID { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? PhoneNo { get; set; }
        public string? Address { get; set; }
    }
}
