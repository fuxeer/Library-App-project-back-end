namespace App_library_back_end.Data
{
    public class BookRepository
    {
        private readonly string _connectoinString ;
        
        public BookRepository(string connectoinString)
        {
            _connectoinString = connectoinString ;
        }

        
    }
}
