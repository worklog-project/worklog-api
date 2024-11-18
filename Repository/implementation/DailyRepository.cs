using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Newtonsoft.Json;
using worklog_api.Model;
using worklog_api.Model.dto;
using JsonSerializer = System.Text.Json.JsonSerializer;

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

        public async Task<Guid> insertDaily(DailyModel dailyModel, Guid scheduleID)
        {
            var query = @"INSERT INTO daily_work_log 
    (ID, DATE, CN_ID, EGI_ID, COUNT) OUTPUT INSERTED.id
VALUES (@Id, @date, @cn_id, @egi_id, @count)";

            
            
            var updatedQuery = @"UPDATE Schedule_Detail 
    SET Daily_ID = @daily_id
    WHERE ID = @id";
            
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var command = new SqlCommand(query, connection, transaction);
                        command.Parameters.AddWithValue("@Id", dailyModel._id);
                        command.Parameters.AddWithValue("@date", dailyModel._date);
                        command.Parameters.AddWithValue("@cn_id", dailyModel._cnId);
                        command.Parameters.AddWithValue("@egi_id", dailyModel._egiId);
                        command.Parameters.AddWithValue("@count", dailyModel._count);
                        
                        Guid id = (Guid) await command.ExecuteScalarAsync();

                        var sqlCommand = new SqlCommand(updatedQuery, connection, transaction);
                        sqlCommand.Parameters.AddWithValue("@id", scheduleID);
                        sqlCommand.Parameters.AddWithValue("@daily_id", id);


                        await sqlCommand.ExecuteNonQueryAsync();

                        await transaction.CommitAsync();

                        return id;
                    }
                    catch (Exception e) {
                        _logger.LogWarning(e.Message);
                        await transaction.RollbackAsync();
                        throw;
                    }
                }

                
            }
        }


        public async Task<Guid> insertDailyDetail(DailyModel dailyModel, Guid generateId, Guid scheduleId, int counted)
        {

            // insert to daily worlog detail
            var query = @"INSERT INTO daily_work_log_detail 
    (id, hour_meter, start_time, finish_time, additional_info, form_type, daily_work_log_id, group_leader, mechanic) OUTPUT INSERTED.id
VALUES (@Id, @hour_meter, @start_time, @finish_time, @additional_info, @form_type, @daily_work_log_id, @group_leader, @mechanic)";

            // update count
            var updatedCountQuery = @"UPDATE daily_work_log 
SET count = count + 1 OUTPUT INSERTED.count
WHERE id = @id";
            
            
            // update Daily
            var updatedScheduleDetail = @"UPDATE Schedule_Detail SET Is_Done = 1 WHERE id = @id";


            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();


                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Serialize the dictionaries to JSON
                        var detailSheetJson = JsonSerializer.Serialize(dailyModel._sheetDetail);

                      
                        // Execute insert command
                        var insertCommand = new SqlCommand(query, connection, transaction);
                        insertCommand.Parameters.AddWithValue("@Id", generateId);
                        insertCommand.Parameters.AddWithValue("@hour_meter", dailyModel._hourmeter);
                        insertCommand.Parameters.AddWithValue("@start_time", dailyModel._startTime);
                        insertCommand.Parameters.AddWithValue("@finish_time", dailyModel._endTime);
                        insertCommand.Parameters.AddWithValue("@additional_info", detailSheetJson);
                        insertCommand.Parameters.AddWithValue("@form_type", dailyModel._formType);
                        insertCommand.Parameters.AddWithValue("@daily_work_log_id", dailyModel._dailyId);
                        insertCommand.Parameters.AddWithValue("@group_leader", dailyModel._groupLeader);
                        insertCommand.Parameters.AddWithValue("@mechanic", dailyModel._mechanic);

                        var id = (Guid)await insertCommand.ExecuteScalarAsync();

                        // Execute update command
                        var updateCommand = new SqlCommand(updatedCountQuery, connection, transaction);
                        updateCommand.Parameters.AddWithValue("@id", dailyModel._dailyId);

                        var count = (int) await updateCommand.ExecuteScalarAsync();

                        if (count == counted)
                        {
                            var scheduleCommand = new SqlCommand(updatedScheduleDetail, connection, transaction);
                            scheduleCommand.Parameters.AddWithValue("@id", scheduleId);
                            await scheduleCommand.ExecuteNonQueryAsync();
                        }
                        
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

//         public async Task<(IEnumerable<AllDailyWorkLogDTO> Items, int TotalCount)> GetPaginatedDailyWorkLogs(
//             int pageNumber, int pageSize, DateTime startDate, DateTime endDate)
//         {
//             // Input validation
//     if (pageNumber < 1) pageNumber = 1;
//     if (pageSize < 1) pageSize = 10;
//
//     // Construct base query with conditional WHERE clause
//     var query = @"
// WITH CountCTE AS (
//     SELECT COUNT(DISTINCT daily_work_log.id) AS TotalCount
//     FROM daily_work_log
//     JOIN dbo.EGI E ON daily_work_log.egi_id = E.ID
//     JOIN dbo.EGI_Code_Number cn ON daily_work_log.cn_id = cn.ID
//     WHERE ((@StartDate IS NULL AND @EndDate IS NULL) 
//            OR (daily_work_log.date >= @StartDate AND daily_work_log.date <= @EndDate))
// ),
// PaginatedLogs AS (
//     SELECT 
//         daily_work_log.id,
//         E.EGI_Name,
//         cn.Code_Number,
//         date
//     FROM daily_work_log 
//     JOIN dbo.EGI E ON daily_work_log.egi_id = E.ID
//     JOIN dbo.EGI_Code_Number cn ON daily_work_log.cn_id = cn.ID
//     WHERE ((@StartDate IS NULL AND @EndDate IS NULL) 
//            OR (daily_work_log.date >= @StartDate AND daily_work_log.date <= @EndDate))
//     ORDER BY date DESC
//     OFFSET @Offset ROWS 
//     FETCH NEXT @PageSize ROWS ONLY
// )
// SELECT 
//     p.*,
//     dn_detail.id as detail_id,
//     dn_detail.form_type,
//     (SELECT TotalCount FROM CountCTE) as TotalCount
// FROM PaginatedLogs p
// LEFT JOIN dbo.daily_work_log_detail dn_detail ON p.id = dn_detail.daily_work_log_id
// ORDER BY p.date DESC, p.id";
//
//     using (var connection = new SqlConnection(_connectionString))
//     {
//         await connection.OpenAsync();
//
//         using (var command = new SqlCommand(query, connection))
//         {
//             // Calculate offset
//             var offset = (pageNumber - 1) * pageSize;
//
//             command.Parameters.Add("@Offset", SqlDbType.Int).Value = offset;
//             command.Parameters.Add("@PageSize", SqlDbType.Int).Value = pageSize;
//             command.Parameters.Add("@StartDate", SqlDbType.DateTime).Value = (object?)startDate ?? DBNull.Value;
//             command.Parameters.Add("@EndDate", SqlDbType.DateTime).Value = (object?)endDate ?? DBNull.Value;
//
//             var dailyLogsDict = new Dictionary<Guid, AllDailyWorkLogDTO>();
//             int totalCount = 0;
//
//             using (var reader = await command.ExecuteReaderAsync())
//             {
//                 while (await reader.ReadAsync())
//                 {
//                     totalCount = reader.GetInt32(reader.GetOrdinal("TotalCount"));
//                     var logId = reader.GetGuid(reader.GetOrdinal("id"));
//
//                     if (!dailyLogsDict.TryGetValue(logId, out var dailyLog))
//                     {
//                         dailyLog = new AllDailyWorkLogDTO()
//                         {
//                             Id = logId,
//                             EgiName = reader.IsDBNull(reader.GetOrdinal("EGI_Name"))
//                                 ? string.Empty
//                                 : reader.GetString(reader.GetOrdinal("EGI_Name")),
//                             CodeNumber = reader.IsDBNull(reader.GetOrdinal("Code_Number"))
//                                 ? string.Empty
//                                 : reader.GetString(reader.GetOrdinal("Code_Number")),
//                             Date = reader.GetDateTime(reader.GetOrdinal("date")),
//                             FormId = new List<FormIdDTO>()
//                         };
//                         dailyLogsDict.Add(logId, dailyLog);
//                     }
//
//                     if (!reader.IsDBNull(reader.GetOrdinal("detail_id")))
//                     {
//                         var formId = new FormIdDTO
//                         {
//                             Id = reader.GetGuid(reader.GetOrdinal("detail_id")),
//                             FormType = reader.IsDBNull(reader.GetOrdinal("form_type"))
//                                 ? string.Empty
//                                 : reader.GetString(reader.GetOrdinal("form_type"))
//                         };
//                         dailyLog.FormId.Add(formId);
//                     }
//                 }
//             }
//
//             return (dailyLogsDict.Values, totalCount);
//         }


                public async Task<(IEnumerable<AllDailyWorkLogDTO> Items, int TotalCount)> GetPaginatedDailyWorkLogs(
            int pageNumber,
            int pageSize)
        {
            // Input validation
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var query = @"
WITH CountCTE AS (
    SELECT COUNT(DISTINCT daily_work_log.id) AS TotalCount
    FROM daily_work_log
    JOIN dbo.EGI E ON daily_work_log.egi_id = E.ID
    JOIN dbo.EGI_Code_Number cn ON daily_work_log.cn_id = cn.ID
),
PaginatedLogs AS (
    SELECT 
        daily_work_log.id,
        E.EGI_Name,
        cn.Code_Number,
        date
    FROM daily_work_log 
    JOIN dbo.EGI E ON daily_work_log.egi_id = E.ID
    JOIN dbo.EGI_Code_Number cn ON daily_work_log.cn_id = cn.ID
    ORDER BY date DESC
    OFFSET @Offset ROWS 
    FETCH NEXT @PageSize ROWS ONLY
)
SELECT 
    p.*,
    dn_detail.id as detail_id,
    dn_detail.form_type,
    (SELECT TotalCount FROM CountCTE) as TotalCount
FROM PaginatedLogs p
LEFT JOIN dbo.daily_work_log_detail dn_detail ON p.id = dn_detail.daily_work_log_id
ORDER BY p.date DESC, p.id";

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand(query, connection))
                {
                    // Calculate offset
                    var offset = (pageNumber - 1) * pageSize;

                    command.Parameters.Add("@Offset", SqlDbType.Int).Value = offset;
                    command.Parameters.Add("@PageSize", SqlDbType.Int).Value = pageSize;

                    var dailyLogsDict = new Dictionary<Guid, AllDailyWorkLogDTO>();
                    int totalCount = 0;

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            totalCount = reader.GetInt32(reader.GetOrdinal("TotalCount"));
                            var logId = reader.GetGuid(reader.GetOrdinal("id"));

                            if (!dailyLogsDict.TryGetValue(logId, out var dailyLog))
                            {
                                dailyLog = new AllDailyWorkLogDTO()
                                {
                                    Id = logId,
                                    EgiName = reader.IsDBNull(reader.GetOrdinal("EGI_Name"))
                                        ? string.Empty
                                        : reader.GetString(reader.GetOrdinal("EGI_Name")),
                                    CodeNumber = reader.IsDBNull(reader.GetOrdinal("Code_Number"))
                                        ? string.Empty
                                        : reader.GetString(reader.GetOrdinal("Code_Number")),
                                    Date = reader.GetDateTime(reader.GetOrdinal("date")),
                                    // GroupLeader = reader.IsDBNull(reader.GetOrdinal("group_leader"))
                                    //     ? string.Empty
                                    //     : reader.GetString(reader.GetOrdinal("group_leader")),
                                    // Mechanic = reader.IsDBNull(reader.GetOrdinal("mechanic"))
                                    //     ? string.Empty
                                    //     : reader.GetString(reader.GetOrdinal("mechanic")),
                                    FormId = new List<FormIdDTO>()
                                };
                                dailyLogsDict.Add(logId, dailyLog);
                            }

                            if (!reader.IsDBNull(reader.GetOrdinal("detail_id")))
                            {
                                var formId = new FormIdDTO
                                {
                                    Id = reader.GetGuid(reader.GetOrdinal("detail_id")),
                                    FormType = reader.IsDBNull(reader.GetOrdinal("form_type"))
                                        ? string.Empty
                                        : reader.GetString(reader.GetOrdinal("form_type"))
                                };
                                dailyLog.FormId.Add(formId);
                            }
                        }
                    }

                    return (dailyLogsDict.Values, totalCount);
                }
            }
        }

        public async Task<DailyModel> getDailyDetailById(Guid id)
        {
            DailyModel dailyModel = null;
            var query = @"SELECT * FROM daily_work_log_detail WHERE id = @id";
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sqlCommand = new SqlCommand(query, connection);
                sqlCommand.Parameters.AddWithValue("@id", id);

                using (var reader = await sqlCommand.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        
                        dailyModel = new DailyModel
                        {
                            _id = reader.GetGuid(reader.GetOrdinal("id")),
                            _sheetDetail = JsonSerializer.Deserialize<Dictionary<string, object>>(reader.GetString(reader.GetOrdinal("additional_info"))),
                            _hourmeter = reader.GetInt32(reader.GetOrdinal("hour_meter")),
                            _startTime = reader.GetTimeSpan(reader.GetOrdinal("start_time")),
                            _endTime = reader.GetTimeSpan(reader.GetOrdinal("finish_time")),
                            _formType = reader.GetString(reader.GetOrdinal("form_type")),
                            _dailyId = reader.GetGuid(reader.GetOrdinal("daily_work_log_id")),
                        };
                    }
                }
            }

            return dailyModel;
        }

        public async Task<string> GetEgiNameByID(string id)
        {

            string egiName = "";
            var query = @"SELECT EGI_Name FROM EGI WHERE id = @id";
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sqlCommand = new SqlCommand(query, connection);
                sqlCommand.Parameters.AddWithValue("@id", Guid.Parse(id));

                using (var reader = await sqlCommand.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        egiName = reader.GetString(reader.GetOrdinal("EGI_Name"));
                    }
                }
            }
            return egiName;
        }

        public async Task<ScheduleDetail> GetScheduleDetailById(string id)
        {
            var query = @"SELECT id, Is_Done, Planned_Date FROM Schedule_Detail WHERE id = @id";
            ScheduleDetail scheduleDetail = null;
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sqlCommand = new SqlCommand(query, connection);
                sqlCommand.Parameters.AddWithValue("@id", Guid.Parse(id));
                
                using (var reader = await sqlCommand.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        scheduleDetail = new ScheduleDetail()
                        {
                            ID = reader.GetGuid(reader.GetOrdinal("id")),
                            IsDone = reader.GetBoolean(reader.GetOrdinal("Is_Done")),
                            PlannedDate = reader.GetDateTime(reader.GetOrdinal("Planned_Date")),
                        };
                    }
                }
            }

            return scheduleDetail;
        }
    }
}