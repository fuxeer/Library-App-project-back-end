using Dapper;
using Microsoft.Data.Sqlite;

namespace App_library_back_end.Data
{
    public class RentService
    {
        private readonly string _connectionString;

        public RentService(string connectionString)
        {
            _connectionString = connectionString;
        }

        private SqliteConnection GetConnection()
        {
            return new SqliteConnection(_connectionString);
        }

        // -------------------------------
        // Create a normal rent (not from reserve)
        // -------------------------------
        public async Task<bool> CreateRent(int borrowerId, int copyId, string dueDate)
        {
            using var conn = GetConnection();
            int rows = await conn.ExecuteAsync(@"
                INSERT INTO rent 
                    (BorrowerID, CopyID, RentDate, RentTime, DueDate, RentStatus)
                VALUES 
                    (@BorrowerID, @CopyID, DATE('now'), TIME('now'), @DueDate, 'Rented');
            ", new
            {
                BorrowerID = borrowerId,
                CopyID = copyId,
                DueDate = dueDate
            });

            return rows > 0;
        }

        // -------------------------------
        // Convert reservation to rent
        // -------------------------------
        public async Task<bool> CreateRentFromReserve(int reserveId)
        {
            using var conn = GetConnection();
            conn.Open();
            using var tran = conn.BeginTransaction();

            // 1. Get the reservation record
            var reserve = await conn.QueryFirstOrDefaultAsync<Reserve>(@"
                SELECT * FROM reserve WHERE ReserveID = @reserveId
            ", new { reserveId }, tran);

            if (reserve == null)
            {
                tran.Rollback();
                return false;
            }

            // 2. Insert the new rent
            int rows = await conn.ExecuteAsync(@"
                INSERT INTO rent 
                    (BorrowerID, CopyID, ReserveID, RentDate, RentTime, DueDate, RentStatus)
                VALUES
                    (@BorrowerID, @CopyID, @ReserveID, DATE('now'), TIME('now'), @DueDate, 'WaitingForPickup');
            ", new
            {
                BorrowerID = reserve.BorrowerID,
                CopyID = reserve.CopyID,
                ReserveID = reserve.ReserveID,
                DueDate = reserve.DueDate
            }, tran);

            // 3. Change copy status (Reserved)
            await conn.ExecuteAsync(@"
                UPDATE copy 
                SET CopyStatus = 'Reserved' 
                WHERE CopyID = @CopyID
            ", new { reserve.CopyID }, tran);

            // 4. Update the reservation status
            await conn.ExecuteAsync(@"
                UPDATE reserve 
                SET ReserveStatus = 'ConvertedToRent'
                WHERE ReserveID = @ReserveID
            ", new { ReserveID = reserve.ReserveID }, tran);

            tran.Commit();
            return rows > 0;
        }

        // -------------------------------
        // Change rent status
        // -------------------------------
        public async Task<bool> UpdateRentStatus(int rentId, string status)
        {
            using var conn = GetConnection();
            int rows = await conn.ExecuteAsync(@"
                UPDATE rent SET RentStatus = @status 
                WHERE RentID = @rentId
            ", new { status, rentId });

            return rows > 0;
        }

        // -------------------------------
        // Change copy status
        // -------------------------------
        public async Task<bool> ChangeCopyStatus(int id, string status)
        {
            using var conn = GetConnection();
            int rows = await conn.ExecuteAsync(@"
                UPDATE copy SET CopyStatus = @status 
                WHERE CopyID = @id
            ", new { status, id });

            return rows > 0;
        }
    }
}
