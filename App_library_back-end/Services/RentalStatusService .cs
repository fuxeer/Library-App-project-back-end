using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Hosting;
namespace App_library_back_end.Services
{
    public class RentalStatusService : BackgroundService
    {

        public readonly String _connectionString;

        public RentalStatusService(String connectionString)
        {
            _connectionString = connectionString;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested) 
            {
                try
                {
                    using var db = new SqliteConnection(_connectionString);
                    await db.OpenAsync(stoppingToken);

                    Console.WriteLine("[Worker] Checking overdue rents...");
                    await UpdateRentToWaitingForReturn(db);

                    Console.WriteLine("[Worker] Handling returned copies...");
                    await HandleReturnedCopies(db);

                    Console.WriteLine("[Worker] Converting reservations to rent...");
                    await ConvertReservationsToRent(db);

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

                await Task.Delay(TimeSpan.FromMinutes(1) , stoppingToken);
            }
        }

        private async Task UpdateRentToWaitingForReturn(IDbConnection db)
        {
            string sql = @"
                UPDATE rent
                SET RentStatus = 'WaitingForReturn'
                WHERE DueDate <= DATE('now')
                AND RentStatus = 'Active';
            ";
            await db.ExecuteAsync(sql);
        }

        // 2) Handle returned copies → assign next reservation
        private async Task HandleReturnedCopies(IDbConnection db)
        {
            using var tran = db.BeginTransaction();

            // Get returned rents
            var returnedRents = await db.QueryAsync<dynamic>(@"
                SELECT RentID, CopyID, BookID
                FROM rent
                WHERE RentStatus = 'Returned';
            ", transaction: tran);

            foreach (var rent in returnedRents)
            {
                // Make copy available
                await db.ExecuteAsync(
                    "UPDATE copy SET CopyStatus = 'AVAILABLE' WHERE CopyID = @CopyID;",
                    new { rent.CopyID }, tran
                );

                // Get earliest reservation for the book
                var reservation = await db.QueryFirstOrDefaultAsync<dynamic>(@"
                    SELECT ReserveID, BorrowerID
                    FROM reserve
                    WHERE BookID = @BookID
                    AND ReserveStatus = 'PENDING'
                    ORDER BY ReserveDate ASC, ReserveTime ASC
                    LIMIT 1;
                ", new { rent.BookID }, tran);

                if (reservation != null)
                {
                    // Assign rent (WaitingForPickup)
                    await db.ExecuteAsync(@"
                        INSERT INTO rent (BorrowerID, CopyID, RentDate, RentTime, DueDate, RentStatus)
                        VALUES (@BorrowerID, @CopyID, DATE('now'), TIME('now'), DATE('now', '+14 day'), 'WaitingForPickup');
                    ", new { BorrowerID = reservation.BorrowerID, CopyID = rent.CopyID }, tran);

                    // Mark reservation as Completed
                    await db.ExecuteAsync(
                        "UPDATE reserve SET ReserveStatus = 'COMPLETED' WHERE ReserveID = @ReserveID;",
                        new { reservation.ReserveID }, tran
                    );
                }

                // Mark old rent fully processed
                await db.ExecuteAsync(
                    "UPDATE rent SET RentStatus = 'Completed' WHERE RentID = @RentID;",
                    new { rent.RentID }, tran
                );
            }

            tran.Commit();
        }

        // 3) Convert reservations → rents when copies are available
        private async Task ConvertReservationsToRent(IDbConnection db)
        {
            using var tran = db.BeginTransaction();

            // Get all available copies
            var availableCopies = await db.QueryAsync<dynamic>(@"
                SELECT CopyID, BookID
                FROM copy
                WHERE CopyStatus = 'AVAILABLE';
            ", transaction: tran);

            foreach (var copy in availableCopies)
            {
                // Get earliest reservation for this book
                var reservation = await db.QueryFirstOrDefaultAsync<dynamic>(@"
                    SELECT ReserveID, BorrowerID
                    FROM reserve
                    WHERE BookID = @BookID
                    AND ReserveStatus = 'PENDING'
                    ORDER BY ReserveDate ASC, ReserveTime ASC
                    LIMIT 1;
                ", new { copy.BookID }, tran);

                if (reservation == null) continue;

                // Mark copy as reserved
                await db.ExecuteAsync(
                    "UPDATE copy SET CopyStatus = 'RENTED' WHERE CopyID = @CopyID;",
                    new { copy.CopyID }, tran
                );

                // Create rent (WaitingForPickup)
                await db.ExecuteAsync(@"
                    INSERT INTO rent (BorrowerID, CopyID, RentDate, RentTime, DueDate, RentStatus)
                    VALUES (@BorrowerID, @CopyID, DATE('now'), TIME('now'), DATE('now', '+14 day'), 'WaitingForPickup');
                ", new { BorrowerID = reservation.BorrowerID, CopyID = copy.CopyID }, tran);

                // Mark reservation completed
                await db.ExecuteAsync(
                    "UPDATE reserve SET ReserveStatus = 'COMPLETED' WHERE ReserveID = @ReserveID;",
                    new { reservation.ReserveID }, tran
                );
            }

            tran.Commit();
        }
    }
}
