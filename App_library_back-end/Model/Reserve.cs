public class Reserve
{
    public int ReserveID { get; set; }
    public int BorrowerID { get; set; }
    public int BookID { get; set; }
    public int? CopyID { get; set; } // Nullable until a copy is assigned
    public string StartDate { get; set; } = string.Empty; // Stored as TEXT in SQLite
    public string DueDate { get; set; } = string.Empty;   // Stored as TEXT
    public string ReserveStatus { get; set; } = string.Empty; // PENDING, COMPLETED, ConvertedToRent
    public string ReserveDate { get; set; } = string.Empty;   // Date the reservation was made
    public string ReserveTime { get; set; } = string.Empty;   // Time the reservation was made
}
