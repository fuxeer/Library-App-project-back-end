using Dapper;
using Microsoft.Data.Sqlite;

namespace App_library_back_end.Data
{
    public class ReservationService
    {
        private readonly string _ConnectionString;

        public ReservationService(string connectionString)
        {
            _ConnectionString = connectionString;
        }

        public bool CanReserve(int bookId, string startDate, string dueDate)
        {
            using var Db = new SqliteConnection(_ConnectionString);
            Db.Open();

            // Count total available copies
            int totalCopies = Db.ExecuteScalar<int>(@"
        SELECT COUNT(*) 
        FROM copy 
        WHERE BookID = @BookID 
        
    ", new { BookID = bookId });

            if (totalCopies == 0) return false;

            // Count overlapping rents
            int overlappingRents = Db.ExecuteScalar<int>(@"
        SELECT COUNT(DISTINCT r.CopyID)
        FROM rent r
        JOIN copy c ON c.CopyID = r.CopyID
        WHERE c.BookID = @BookID
        AND NOT (
            DATE(r.DueDate) < DATE(@StartDate)
            OR
            DATE(r.RentDate) > DATE(@DueDate)
        );
    ", new { BookID = bookId, StartDate = startDate, DueDate = dueDate });

            // Count overlapping reservations
            int overlappingReservations = Db.ExecuteScalar<int>(@"
        SELECT COUNT(*)
        FROM reserve rs
        WHERE rs.BookID = @BookID
        AND rs.ReserveStatus = 'PENDING'
        AND NOT (
            DATE(rs.DueDate) < DATE(@StartDate)
            OR
            DATE(rs.StartDate) > DATE(@DueDate)
        );
    ", new { BookID = bookId, StartDate = startDate, DueDate = dueDate });

            int blocked = overlappingRents + overlappingReservations;

            return blocked < totalCopies;
        }


        public bool AddReservation(int userId, int bookId, string startDate, string dueDate)
        {
            if (!CanReserve(bookId, startDate, dueDate))
                return false;

            using var Db = new SqliteConnection(_ConnectionString);
            Db.Open();

            int row = Db.Execute(@"
                INSERT INTO reserve 
                    (BorrowerID, BookID, StartDate, DueDate, ReserveStatus, ReserveDate, ReserveTime)
                VALUES 
                    (@UserID, @BookID, @Start, @Due, 'PENDING', DATE('now'), TIME('now'));
            ", new
            {
                UserID = userId,
                BookID = bookId,
                Start = startDate,
                Due = dueDate
            });

            return row > 0;
        }

        public List<(DateTime Start, DateTime End)> GetAvailableRanges(int bookId, int daysAhead = 90)
        {
            using var Db = new SqliteConnection(_ConnectionString);
            Db.Open();

            // Total available copies
            int totalCopies = Db.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM copy WHERE BookID = @BookID AND CopyStatus='AVAILABLE'",
                new { BookID = bookId });

            if (totalCopies == 0) return new List<(DateTime, DateTime)>();

            // Blocked periods from rents and reservations
            var blockedPeriods = Db.Query<(DateTime Start, DateTime End)>(@"
                SELECT r.RentDate AS Start, r.DueDate AS End
                FROM rent r
                JOIN copy c ON c.CopyID = r.CopyID
                WHERE c.BookID = @BookID

                UNION ALL

                SELECT rs.StartDate AS Start, rs.DueDate AS End
                FROM reserve rs
                WHERE rs.BookID = @BookID AND rs.ReserveStatus='PENDING';
            ", new { BookID = bookId }).ToList();

            // Count blocked copies per day
            var dateCounts = new Dictionary<DateTime, int>();
            DateTime today = DateTime.Today;
            DateTime endDate = today.AddDays(daysAhead);

            foreach (var period in blockedPeriods)
            {
                for (DateTime d = period.Start; d <= period.End; d = d.AddDays(1))
                {
                    if (!dateCounts.ContainsKey(d)) dateCounts[d] = 0;
                    dateCounts[d]++;
                }
            }

            // Build available ranges
            var availableRanges = new List<(DateTime Start, DateTime End)>();
            DateTime? rangeStart = null;

            for (DateTime d = today; d <= endDate; d = d.AddDays(1))
            {
                int blocked = dateCounts.ContainsKey(d) ? dateCounts[d] : 0;

                if (blocked < totalCopies)
                {
                    if (rangeStart == null) rangeStart = d; // start new range
                }
                else
                {
                    if (rangeStart != null)
                    {
                        availableRanges.Add((rangeStart.Value, d.AddDays(-1)));
                        rangeStart = null;
                    }
                }
            }

            if (rangeStart != null)
                availableRanges.Add((rangeStart.Value, endDate));

            return availableRanges;
        }
    }
}
