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

                var command = new SqlCommand(@"
                    INSERT INTO MOL_Tracking_History (ID, MOL_ID, WR_Code, Status, Additional_Info)
                    VALUES (@ID, @MOLID, @WRCode, @Status, @AdditionalInfo)", connection);

                command.Parameters.AddWithValue("@ID", molTracking.ID);
                command.Parameters.AddWithValue("@MOLID", molTracking.MOLID);
                command.Parameters.AddWithValue("@WRCode", molTracking.WRCode);
                command.Parameters.AddWithValue("@Status", molTracking.Status);
                command.Parameters.AddWithValue("@AdditionalInfo", molTracking.AdditionalInfo);

                await command.ExecuteNonQueryAsync();
            }
        }
    }
}
