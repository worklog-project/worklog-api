using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Threading.Tasks;
using System.Text.Json;
using worklog_api.Model;
using worklog_api.Model.dto;
using worklog_api.Model.form;

namespace worklog_api.Repository.implementation
{
    public class DailyRepository : IDailyRepository
    {
        private readonly string _connectionString;    
        private readonly ILogger<DailyRepository> _logger;


        public DailyRepository(string connectionString, ILogger<DailyRepository> dailyLogger)
        {
            _connectionString = connectionString;
            _logger = dailyLogger;
        }

        public async Task<IEnumerable<EGIModel>> GetEGI(string egiName)
        {
            var egiList = new List<EGIModel>();
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = @"
                    SELECT * FROM EGI
                    WHERE EGI_Name LIKE '%' + @egi + '%'";

                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@egi", (object)egiName ?? DBNull.Value);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var egi = new EGIModel
                        {
                            ID = reader.GetGuid(reader.GetOrdinal("ID")),
                            EGI = reader.GetString(reader.GetOrdinal("EGI_Name")),

                        };
                        egiList.Add(egi);
                    }
                }
            }
            return (egiList);
        }

        public async Task<IEnumerable<CodeNumberModel>> GetCodeNumber(string codeNumber, Guid egiID)
        {
            var codeNumberList = new List<CodeNumberModel>();
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = @"
                    SELECT * FROM EGI_Code_Number
                    WHERE Code_Number LIKE '%' + @code + '%'
                    AND EGI_ID = @egiID";

                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@code", (object)codeNumber ?? DBNull.Value);
                command.Parameters.AddWithValue("@egiID", egiID);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var codeNumberModel = new CodeNumberModel
                        {
                            ID = reader.GetGuid(reader.GetOrdinal("ID")),
                            CodeNumber = reader.GetString(reader.GetOrdinal("Code_Number")),
                            EGIID = reader.GetGuid(reader.GetOrdinal("EGI_ID"))
                        };
                        codeNumberList.Add(codeNumberModel);
                    }
                }
            }
            return codeNumberList;
        }

        public async Task<DailyModel> getDailyByEgiAndCodeNumberAndDate(string egi, string codeNumber, string date)
        {
            DailyModel dailyModel = null;
            var query = @"
        SELECT id, count FROM daily_work_log WHERE cn_id = @cn AND egi_id = @egi AND date = @date;
                ";           
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var sqlCommand = new SqlCommand(query, connection);
                sqlCommand.Parameters.AddWithValue("@cn", codeNumber);
                sqlCommand.Parameters.AddWithValue("@egi", egi);
                sqlCommand.Parameters.AddWithValue("@date", SqlDbType.Date).Value = DateTime.Parse(date).Date;

                using (var reader = await sqlCommand.ExecuteReaderAsync())
                {
                    // Check if there are any rows
                    if (await reader.ReadAsync())
                    {
                        dailyModel = new DailyModel();
                        Guid id = reader.GetGuid(reader.GetOrdinal("id"));
                        int count = reader.GetInt32(reader.GetOrdinal("count"));
                        dailyModel._id = id;
                        dailyModel._count = count;
                    }
                }
            }
            return dailyModel;
        }

        public async Task insertDaily(DailyModel dailyModel)
        {
            var query = @"INSERT INTO daily_work_log 
    (ID, DATE, CN_ID, EGI_ID, COUNT, GROUP_LEADER, MECHANIC) 
VALUES (@id, @date, @cn_id, @egi_id, @count, @group_leader, @mechanic)";
            
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                try
                {
                    var command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@id", dailyModel._id);
                    command.Parameters.AddWithValue("@date", dailyModel._date);
                    command.Parameters.AddWithValue("@cn_id", dailyModel._cnId);
                    command.Parameters.AddWithValue("@egi_id",dailyModel._egiId);
                    command.Parameters.AddWithValue("@count",dailyModel._count);
                    command.Parameters.AddWithValue("@group_leader",dailyModel._groupLeader);
                    command.Parameters.AddWithValue("@mechanic",dailyModel._mechanic);
                
                    await command.ExecuteNonQueryAsync();
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e.Message);
                    throw;
                }
            }
        }
        public async Task<Guid> insertDailyDetail(FormDTO formDTO, Guid generateId)
        {
            
            // insert to daily worlog detail
            var query = @"INSERT INTO daily_work_log_detail 
    (id, hour_meter, start_time, finish_time, additional_info, form_type, daily_work_log_id) OUTPUT INSERTED.id
VALUES (@Id, @hour_meter, @start_time, @finish_time, @additional_info, @form_type, @daily_work_log_id)";
            
            // update count
            var updateCountQuery = @"UPDATE daily_work_log 
SET count = count + 1
WHERE id = @id";
            
            
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Serialize the dictionaries to JSON
                        var activitiesJson = JsonSerializer.Serialize(formDTO._activities);
                        var consumablesJson = JsonSerializer.Serialize(formDTO._consumables);
                
                        // Execute insert command
                        var insertCommand = new SqlCommand(query, connection, transaction);
                        insertCommand.Parameters.AddWithValue("@Id", generateId);
                        insertCommand.Parameters.AddWithValue("@hour_meter", formDTO._hourmeter);
                        insertCommand.Parameters.AddWithValue("@start_time", formDTO._startTime);
                        insertCommand.Parameters.AddWithValue("@finish_time", formDTO._endTime);
                        insertCommand.Parameters.AddWithValue("@additional_info", $"{{\"activities\":{activitiesJson},\"consumables\":{consumablesJson}}}");
                        insertCommand.Parameters.AddWithValue("@form_type", formDTO._formType);
                        insertCommand.Parameters.AddWithValue("@daily_work_log_id", formDTO._dailyId);

                        var id = (Guid) await insertCommand.ExecuteScalarAsync();

                        // Execute update command
                        var updateCommand = new SqlCommand(updateCountQuery, connection, transaction);
                        updateCommand.Parameters.AddWithValue("@id", formDTO._dailyId);
                
                        await updateCommand.ExecuteNonQueryAsync();
                
                        // Commit the transaction
                        await transaction.CommitAsync();
                        return id;
                    }
                    catch (Exception e)
                    {
                        // Roll back the transaction if there's an error
                        await transaction.RollbackAsync();
                        _logger.LogWarning($"Transaction failed: {e.Message}");
                        throw;
                    }
                }
            }
        }
        
    }
}
