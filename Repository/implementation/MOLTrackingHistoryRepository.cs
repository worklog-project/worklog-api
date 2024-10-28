using System.Data.SqlClient;
using System.Threading.Tasks;
using worklog_api.Model;

namespace worklog_api.Repository
{
    public class MOLTrackingHistoryRepository : IMOLTrackingHistoryRepository
    {
        private readonly string _connectionString;

        public MOLTrackingHistoryRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task Create(MOLTrackingHistoryModel molTracking)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Start a transaction
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Insert into MOL_Tracking_History
                        var trackingCommand = new SqlCommand(@"
                            INSERT INTO MOL_Tracking_History (ID, MOL_ID, WR_Code, Status, Additional_Info,Version, Created_By, Updated_By, Created_At, Updated_At)
                            VALUES (@ID, @MOLID, @WRCode, @Status, @AdditionalInfo,@Version, @CreatedBy, @UpdatedBy, @CreatedAt, @UpdatedAt)", connection, transaction);

                        trackingCommand.Parameters.AddWithValue("@ID", molTracking.ID);
                        trackingCommand.Parameters.AddWithValue("@MOLID", molTracking.MOLID);
                        trackingCommand.Parameters.AddWithValue("@WRCode", molTracking.WRCode);
                        trackingCommand.Parameters.AddWithValue("@Status", molTracking.Status);
                        trackingCommand.Parameters.AddWithValue("@AdditionalInfo", molTracking.AdditionalInfo);
                        trackingCommand.Parameters.AddWithValue("@Version", 1);
                        trackingCommand.Parameters.AddWithValue("@CreatedBy", molTracking.CreatedBy);
                        trackingCommand.Parameters.AddWithValue("@UpdatedBy", molTracking.UpdatedBy);
                        trackingCommand.Parameters.AddWithValue("@CreatedAt", molTracking.CreatedAt);
                        trackingCommand.Parameters.AddWithValue("@UpdatedAt", molTracking.UpdatedAt);

                        await trackingCommand.ExecuteNonQueryAsync();

                        // Update MOL status based on molTracking.Status
                        var updateCommand = new SqlCommand(@"
                            UPDATE MOL
                            SET Status = @Status,
                            Updated_At = @UpdatedAt
                            WHERE ID = @MOLID", connection, transaction);

                        updateCommand.Parameters.AddWithValue("@Status", molTracking.Status);
                        updateCommand.Parameters.AddWithValue("@MOLID", molTracking.MOLID);
                        updateCommand.Parameters.AddWithValue("@UpdatedAt", molTracking.UpdatedAt);

                        await updateCommand.ExecuteNonQueryAsync();

                        // Commit the transaction if both commands are successful
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        // Rollback the transaction if any error occurs
                        await transaction.RollbackAsync();
                        throw new Exception("Error occurred while creating MOL Tracking History and updating MOL status: " + ex.Message, ex);
                    }
                }
            }
        }

    }
}
