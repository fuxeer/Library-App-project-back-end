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
        private readonly string _connectionString;
        public RentalStatusService(string connectionString)
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

                    Console.WriteLine("[Worker] Checking overdue active rents...");
                    await MarkOverdueRents(db);

                    Console.WriteLine("[Worker] Converting reservations that reached StartDate...");
                    await ConvertReservationsToRent(db);

                    Console.WriteLine("[Worker] Processing returned copies...");
                    await ProcessReturnedCopies(db);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        // ---------------------------------------------------------
        // 1) Mark overdue rents as WaitingForReturn
        // ---------------------------------------------------------
        private async Task MarkOverdueRents(IDbConnection db)
        {
            await db.ExecuteAsync(@"
                UPDATE rent
                SET RentStatus = 'WaitingForReturn'
                WHERE RentStatus = 'Active'
                AND DueDate <= DATE('now');
            ");
        }

        // ---------------------------------------------------------
        // 2) Convert reservations → rent ONLY when StartDate has arrived
        // ---------------------------------------------------------
        private async Task ConvertReservationsToRent(IDbConnection db)
        {
            using var tran = db.BeginTransaction();

            var pendingReservations = await db.QueryAsync<dynamic>(@"
                SELECT ReserveID, BorrowerID, BookID, StartDate, DueDate
                FROM reserve
                WHERE ReserveStatus = 'PENDING'
                AND StartDate <= DATE('now');
            ", transaction: tran);

            foreach (var res in pendingReservations)
            {
                var copy = await db.QueryFirstOrDefaultAsync<dynamic>(@"
                    SELECT CopyID
                    FROM copy
                    WHERE BookID = @BookID
                    AND CopyStatus = 'AVAILABLE'
                    LIMIT 1;
                ", new { res.BookID }, tran);

                if (copy == null)
                    continue;

                // Mark copy as RENTED
                await db.ExecuteAsync(@"
                    UPDATE copy
                    SET CopyStatus = 'RENTED'
                    WHERE CopyID = @CopyID;
                ", new { copy.CopyID }, tran);

                // Create rent using the user's DueDate
                await db.ExecuteAsync(@"
                    INSERT INTO rent (BorrowerID, CopyID, ReserveID, RentDate, RentTime, DueDate, RentStatus)
                    VALUES (@BorrowerID, @CopyID, @ReserveID, DATE('now'), TIME('now'), @DueDate, 'WaitingForPickup');
                ", new
                {
                    BorrowerID = res.BorrowerID,
                    CopyID = copy.CopyID,
                    ReserveID = res.ReserveID,
                    DueDate = res.DueDate
                }, tran);

                // Mark reservation done
                await db.ExecuteAsync(@"
                    UPDATE reserve
                    SET ReserveStatus = 'COMPLETED'
                    WHERE ReserveID = @ReserveID;
                ", new { res.ReserveID }, tran);
            }

            tran.Commit();
        }

        // ---------------------------------------------------------
        // 3) Process returned copies → assign next reservation
        // ---------------------------------------------------------
        private async Task ProcessReturnedCopies(IDbConnection db)
        {
            using var tran = db.BeginTransaction();

            var returned = await db.QueryAsync<dynamic>(@"
                SELECT RentID, CopyID, ReserveID
                FROM rent
                WHERE RentStatus = 'Returned';
            ", transaction: tran);

            foreach (var rent in returned)
            {
                // Make copy available temporarily
                await db.ExecuteAsync(@"
                    UPDATE copy
                    SET CopyStatus = 'AVAILABLE'
                    WHERE CopyID = @CopyID;
                ", new { rent.CopyID }, tran);

                // Find next reservation for same book
                var res = await db.QueryFirstOrDefaultAsync<dynamic>(@"
                    SELECT ReserveID, BorrowerID, DueDate
                    FROM reserve
                    WHERE BookID = (SELECT BookID FROM copy WHERE CopyID = @CopyID)
                    AND ReserveStatus = 'PENDING'
                    ORDER BY ReserveDate ASC, ReserveTime ASC
                    LIMIT 1;
                ", new { rent.CopyID }, tran);

                if (res != null)
                {
                    // Copy becomes RENTED
                    await db.ExecuteAsync(@"
                        UPDATE copy SET CopyStatus = 'RENTED' WHERE CopyID = @CopyID;
                    ", new { rent.CopyID }, tran);

                    // Create next rent
                    await db.ExecuteAsync(@"
                        INSERT INTO rent (BorrowerID, CopyID, ReserveID, RentDate, RentTime, DueDate, RentStatus)
                        VALUES (@BorrowerID, @CopyID, @ReserveID, DATE('now'), TIME('now'), @DueDate, 'WaitingForPickup');
                    ", new
                    {
                        BorrowerID = res.BorrowerID,
                        CopyID = rent.CopyID,
                        ReserveID = res.ReserveID,
                        DueDate = res.DueDate
                    }, tran);

                    // Mark reservation done
                    await db.ExecuteAsync(@"
                        UPDATE reserve
                        SET ReserveStatus = 'COMPLETED'
                        WHERE ReserveID = @ReserveID;
                    ", new { res.ReserveID }, tran);
                }

                // Close old rent
                await db.ExecuteAsync(@"
                    UPDATE rent
                    SET RentStatus = 'Completed'
                    WHERE RentID = @RentID;
                ", new { rent.RentID }, tran);
            }

            tran.Commit();
        }
    }
}
