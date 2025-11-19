namespace App_library_back_end.Model
{
    public class Book
    {
        public int BookID { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Publisher { get; set; }
        public int PublishYear { get; set; }
        public double Rating { get; set; }

    }
}