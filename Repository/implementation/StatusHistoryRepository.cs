using System.Data.SqlClient;
using System.Threading.Tasks;
using worklog_api.Model;

namespace worklog_api.Repository.implementation
{
    public class StatusHistoryRepository : IStatusHistoryRepository
    {
        private readonly string _connectionString;

        public StatusHistoryRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task Create(StatusHistoryModel statusHistory)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Begin a transaction
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Insert the status history record
                        var insertHistoryCommand = new SqlCommand(@"
                            INSERT INTO Status_History_MOL 
                            (ID, MOL_ID, Status, Remark, Version, Created_By, Updated_By, Created_At, Updated_At)
                            VALUES 
                            (@ID, @MOLID, @Status, @Remark, @Version, @CreatedBy, @UpdatedBy, @CreatedAt, @UpdatedAt)", connection, transaction);

                        insertHistoryCommand.Parameters.AddWithValue("@ID", statusHistory.ID);
                        insertHistoryCommand.Parameters.AddWithValue("@MOLID", statusHistory.MOLID);
                        insertHistoryCommand.Parameters.AddWithValue("@Remark", statusHistory.Remark);
                        insertHistoryCommand.Parameters.AddWithValue("@Status", statusHistory.Status);
                        insertHistoryCommand.Parameters.AddWithValue("@Version", statusHistory.Version);
                        insertHistoryCommand.Parameters.AddWithValue("@CreatedBy", statusHistory.CreatedBy);
                        insertHistoryCommand.Parameters.AddWithValue("@UpdatedBy", statusHistory.UpdatedBy);
                        insertHistoryCommand.Parameters.AddWithValue("@CreatedAt", statusHistory.CreatedAt);
                        insertHistoryCommand.Parameters.AddWithValue("@UpdatedAt", statusHistory.UpdatedAt);

                        await insertHistoryCommand.ExecuteNonQueryAsync();

                        // Update the MOL status based on statusHistory.Status
                        var updateMolStatusCommand = new SqlCommand(@"
                            UPDATE MOL 
                            SET Status = @Status, Updated_By = @UpdatedBy, Updated_At = @UpdatedAt 
                            WHERE ID = @MOLID", connection, transaction);

                        updateMolStatusCommand.Parameters.AddWithValue("@Status", statusHistory.Status);
                        updateMolStatusCommand.Parameters.AddWithValue("@UpdatedBy", statusHistory.UpdatedBy);
                        updateMolStatusCommand.Parameters.AddWithValue("@UpdatedAt", statusHistory.UpdatedAt);
                        updateMolStatusCommand.Parameters.AddWithValue("@MOLID", statusHistory.MOLID);

                        await updateMolStatusCommand.ExecuteNonQueryAsync();

                        // Commit the transaction if all commands are successful
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        // Rollback the transaction if an error occurs
                        transaction.Rollback();
                        // Optionally, log the exception or rethrow it for higher-level handling
                        throw new Exception("An error occurred while creating status history and updating MOL status.", ex);
                    }
                }
            }
        }

    }
}
