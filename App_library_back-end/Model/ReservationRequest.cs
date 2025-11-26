namespace App_library_back_end.Model
{
    public class ReservationRequest
    {
        public int UserId { get; set; }
        public int BookId { get; set; }
        public string StartDate { get; set; } = string.Empty;
        public string DueDate { get; set; } = string.Empty;
    }
}
