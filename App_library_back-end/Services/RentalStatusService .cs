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

                    await MarkOverdueRents(db);
                    await MoveReservationsToWaitingForPickup(db);
                    await ProcessReturnedCopies(db);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        // 1) Mark overdue rents as WaitingForReturn
        private async Task MarkOverdueRents(IDbConnection db)
        {
            await db.ExecuteAsync(@"
                UPDATE rent
                SET RentStatus = 'WaitingForReturn'
                WHERE RentStatus = 'Active'
                AND DueDate <= DATE('now');
            ");
        }

        // 2) Convert reservation to WaitingForPickup when StartDate arrives
        private async Task MoveReservationsToWaitingForPickup(IDbConnection db)
        {
            await db.ExecuteAsync(@"
                UPDATE reserve
                SET ReserveStatus = 'WaitingForPickup'
                WHERE ReserveStatus = 'PENDING'
                AND StartDate <= DATE('now');
            ");
        }

        // 3) Handle returned copies → assign next reservation
        private async Task ProcessReturnedCopies(IDbConnection db)
        {
            using var tran = db.BeginTransaction();

            var returned = await db.QueryAsync<dynamic>(@"
                SELECT RentID, CopyID
                FROM rent
                WHERE RentStatus = 'Returned';
            ", transaction: tran);

            foreach (var rent in returned)
            {
                await db.ExecuteAsync(@"
                    UPDATE copy
                    SET CopyStatus = 'AVAILABLE'
                    WHERE CopyID = @CopyID;
                ", new { rent.CopyID }, tran);

                var next = await db.QueryFirstOrDefaultAsync<dynamic>(@"
                    SELECT ReserveID, BorrowerID, DueDate
                    FROM reserve
                    WHERE BookID = (SELECT BookID FROM copy WHERE CopyID = @CopyID)
                    AND ReserveStatus = 'PENDING'
                    ORDER BY ReserveDate ASC, ReserveTime ASC
                    LIMIT 1;
                ", new { rent.CopyID }, tran);

                if (next != null)
                {
                    await db.ExecuteAsync(@"
                        UPDATE reserve
                        SET ReserveStatus = 'WaitingForPickup'
                        WHERE ReserveID = @ReserveID;
                    ", new { next.ReserveID }, tran);

                    await db.ExecuteAsync(@"
                        UPDATE copy SET CopyStatus = 'Reserved'
                        WHERE CopyID = @CopyID;
                    ", new { rent.CopyID }, tran);
                }

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
