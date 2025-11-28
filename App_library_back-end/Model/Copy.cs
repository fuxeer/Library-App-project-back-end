namespace App_library_back_end.Model
{
    public class Copy
    {
        public int CopyID {  get; set; }
        public int BookID { get; set; }
        public string SerialNumber { get; set; } = string.Empty;
        public string CopyStatus { get; set; } = string.Empty;
    }
}
