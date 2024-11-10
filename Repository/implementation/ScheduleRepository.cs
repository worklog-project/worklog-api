using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using worklog_api.Model;
using worklog_api.Model.dto;

namespace worklog_api.Repository.implementation
{
    public class ScheduleRepository : IScheduleRepository
    {

        private readonly string _connectionString;

        public ScheduleRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task Create(Schedule schedule)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Generate a new GUID for the schedule ID if not already set
                        schedule.ID = schedule.ID == Guid.Empty ? Guid.NewGuid() : schedule.ID;
                        Console.WriteLine("Schedule ID: " + schedule.ID);

                        // Insert into the schedule table
                        var scheduleCommand = new SqlCommand(@"
                        INSERT INTO schedule 
                        (ID, EGI_ID, CN_ID, Schedule_Month, Created_By, Updated_By, Created_At, Updated_At) 
                        VALUES 
                        (@ID, @EGIID, @CNID, @ScheduleMonth, @CreatedBy, @UpdatedBy, @CreatedAt, @UpdatedAt)", connection, transaction);

                        scheduleCommand.Parameters.AddWithValue("@ID", schedule.ID);
                        scheduleCommand.Parameters.AddWithValue("@EGIID", schedule.EGIID);
                        scheduleCommand.Parameters.AddWithValue("@CNID", schedule.CNID);
                        scheduleCommand.Parameters.AddWithValue("@ScheduleMonth", schedule.ScheduleMonth); // Add ScheduleMonth to the insert command (1/2)
                        scheduleCommand.Parameters.AddWithValue("@CreatedBy", schedule.CreatedBy);
                        scheduleCommand.Parameters.AddWithValue("@UpdatedBy", schedule.UpdatedBy);
                        scheduleCommand.Parameters.AddWithValue("@CreatedAt", schedule.CreatedAt);
                        scheduleCommand.Parameters.AddWithValue("@UpdatedAt", schedule.UpdatedAt);

                        await scheduleCommand.ExecuteNonQueryAsync();

                        // Use the same ID to insert related records in schedule_details
                        foreach (var scheduleDetail in schedule.ScheduleDetails)
                        {
                            var detailCommand = new SqlCommand(@"
                            INSERT INTO schedule_detail
                            (ID, Schedule_ID, Planned_Date, Is_Done, Daily_ID, Created_By, Updated_By, Created_At, Updated_At) 
                            VALUES 
                            (@ID, @ScheduleID, @PlannedDate, @IsDone, @DailyID, @CreatedBy, @UpdatedBy, @CreatedAt, @UpdatedAt)", connection, transaction);

                            scheduleDetail.ID = Guid.NewGuid(); // Generate a new GUID for each schedule detail
                            detailCommand.Parameters.AddWithValue("@ID", scheduleDetail.ID);
                            detailCommand.Parameters.AddWithValue("@ScheduleID", schedule.ID); // Use the retrieved Schedule ID
                            detailCommand.Parameters.AddWithValue("@PlannedDate", scheduleDetail.PlannedDate);
                            detailCommand.Parameters.AddWithValue("@IsDone", scheduleDetail.IsDone);
                            detailCommand.Parameters.AddWithValue("@DailyID", scheduleDetail.DailyID);
                            detailCommand.Parameters.AddWithValue("@CreatedBy", scheduleDetail.CreatedBy);
                            detailCommand.Parameters.AddWithValue("@UpdatedBy", scheduleDetail.UpdatedBy);
                            detailCommand.Parameters.AddWithValue("@CreatedAt", scheduleDetail.CreatedAt);
                            detailCommand.Parameters.AddWithValue("@UpdatedAt", scheduleDetail.UpdatedAt);

                            await detailCommand.ExecuteNonQueryAsync();
                        }

                        // Commit the transaction if all commands succeed
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        // Rollback the transaction if any command fails
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
        public async Task<Schedule> GetScheduleDetailsByMonth(DateTime scheduleMonth, Guid? egiId = null, Guid? cnId = null)
        {
            Schedule schedule = null;

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Base query to retrieve a single schedule by ScheduleMonth, EGI_ID, and CN_ID if specified, with joins to EGI and CN tables
                var query = @"
                SELECT TOP 1 
                s.ID, s.EGI_ID, s.CN_ID, s.Schedule_Month, s.Created_By, s.Updated_By, s.Created_At, s.Updated_At,
                e.EGI_Name AS EGI_Name, c.Code_Number AS CN_Name
                FROM schedule s
                LEFT JOIN EGI e ON s.EGI_ID = e.ID
                LEFT JOIN EGI_Code_Number c ON s.CN_ID = c.ID
                WHERE YEAR(s.Schedule_Month) = @Year AND MONTH(s.Schedule_Month) = @Month";

                // Append additional filters if EGI_ID or CN_ID are provided
                if (egiId.HasValue)
                {
                    query += " AND s.EGI_ID = @EGIID";
                }
                if (cnId.HasValue)
                {
                    query += " AND s.CN_ID = @CNID";
                }

                var scheduleCommand = new SqlCommand(query, connection);
                scheduleCommand.Parameters.AddWithValue("@Year", scheduleMonth.Year);
                scheduleCommand.Parameters.AddWithValue("@Month", scheduleMonth.Month);

                if (egiId.HasValue)
                {
                    scheduleCommand.Parameters.AddWithValue("@EGIID", egiId.Value);
                }
                if (cnId.HasValue)
                {
                    scheduleCommand.Parameters.AddWithValue("@CNID", cnId.Value);
                }

                using (var reader = await scheduleCommand.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        schedule = new Schedule
                        {
                            ID = reader.GetGuid(reader.GetOrdinal("ID")),
                            EGIID = reader.GetGuid(reader.GetOrdinal("EGI_ID")),
                            CNID = reader.GetGuid(reader.GetOrdinal("CN_ID")),
                            ScheduleMonth = reader.GetDateTime(reader.GetOrdinal("Schedule_Month")),
                            CreatedBy = reader.GetString(reader.GetOrdinal("Created_By")),
                            UpdatedBy = reader.GetString(reader.GetOrdinal("Updated_By")),
                            CreatedAt = reader.GetDateTime(reader.GetOrdinal("Created_At")),
                            UpdatedAt = reader.GetDateTime(reader.GetOrdinal("Updated_At")),
                            ScheduleDetails = new List<ScheduleDetail>(),

                            // Assuming that EGI and CN names are included in the Schedule object
                            Egi = reader.IsDBNull(reader.GetOrdinal("EGI_Name")) ? null : reader.GetString(reader.GetOrdinal("EGI_Name")),
                            CodeNumber = reader.IsDBNull(reader.GetOrdinal("CN_Name")) ? null : reader.GetString(reader.GetOrdinal("CN_Name"))
                        };

                        // Retrieve the schedule details for the specific schedule ID
                        var scheduleId = reader.GetGuid(reader.GetOrdinal("ID"));
                        await LoadScheduleDetails(schedule, scheduleId, connection);
                    }
                }
            }

            return schedule;
        }

        private async Task LoadScheduleDetails(Schedule schedule, Guid scheduleId, SqlConnection connection)
        {
            // Query to retrieve schedule details for a specific schedule ID
            var detailCommand = new SqlCommand(@"
                SELECT ID,Schedule_ID, Planned_Date, Is_Done, Daily_ID, Created_By, Updated_By, Created_At, Updated_At 
                FROM schedule_detail 
                WHERE Schedule_ID = @ScheduleID", connection);

            detailCommand.Parameters.AddWithValue("@ScheduleID", scheduleId);

            // Open a new reader for schedule details
            using (var reader = await detailCommand.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var scheduleDetail = new ScheduleDetail
                    {
                        ID = reader.GetGuid(reader.GetOrdinal("ID")),
                        ScheduleID = reader.GetGuid(reader.GetOrdinal("Schedule_ID")),
                        IsDone = reader.GetBoolean(reader.GetOrdinal("Is_Done")),
                        DailyID = reader.GetGuid(reader.GetOrdinal("Daily_ID")),
                        PlannedDate = reader.GetDateTime(reader.GetOrdinal("Planned_Date")),
                        CreatedBy = reader.GetString(reader.GetOrdinal("Created_By")),
                        UpdatedBy = reader.GetString(reader.GetOrdinal("Updated_By")),
                        CreatedAt = reader.GetDateTime(reader.GetOrdinal("Created_At")),
                        UpdatedAt = reader.GetDateTime(reader.GetOrdinal("Updated_At"))
                    };

                    schedule.ScheduleDetails.Add(scheduleDetail);
                }
            }
        }

        public async Task UpdateScheduleDetails(Guid scheduleId, List<ScheduleDetail> updatedDetails)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Start a transaction to ensure all changes are committed together
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Fetch existing details for the schedule
                        var existingDetails = new Dictionary<Guid, ScheduleDetail>();
                        var getExistingDetailsCommand = new SqlCommand(
                            @"SELECT ID, Planned_Date, Is_Done, Daily_ID, Created_By, Updated_By, Created_At, Updated_At 
                              FROM schedule_detail 
                              WHERE Schedule_ID = @ScheduleID", connection, transaction);
                        getExistingDetailsCommand.Parameters.AddWithValue("@ScheduleID", scheduleId);

                        using (var reader = await getExistingDetailsCommand.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var detail = new ScheduleDetail
                                {
                                    ID = reader.GetGuid(reader.GetOrdinal("ID")),
                                    PlannedDate = reader.GetDateTime(reader.GetOrdinal("Planned_Date")),
                                    IsDone = reader.GetBoolean(reader.GetOrdinal("Is_Done")),
                                    DailyID = reader.GetGuid(reader.GetOrdinal("Daily_ID")),
                                    CreatedBy = reader.GetString(reader.GetOrdinal("Created_By")),
                                    UpdatedBy = reader.GetString(reader.GetOrdinal("Updated_By")),
                                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("Created_At")),
                                    UpdatedAt = reader.GetDateTime(reader.GetOrdinal("Updated_At"))
                                };
                                existingDetails[detail.ID] = detail;
                            }
                        }

                        // Determine which details to add, update, or delete
                        var updatedDetailsDict = updatedDetails.ToDictionary(d => d.ID, d => d);

                        // Update or Insert
                        foreach (var detail in updatedDetails)
                        {
                            if (existingDetails.ContainsKey(detail.ID))
                            {
                                // Update existing detail
                                var updateCommand = new SqlCommand(
                                    @"UPDATE schedule_detail SET Planned_Date = @PlannedDate, Is_Done = @IsDone, 
                                      Daily_ID = @DailyID, Updated_By = @UpdatedBy, Updated_At = @UpdatedAt 
                                      WHERE ID = @ID", connection, transaction);
                                updateCommand.Parameters.AddWithValue("@ID", detail.ID);
                                updateCommand.Parameters.AddWithValue("@PlannedDate", detail.PlannedDate);
                                updateCommand.Parameters.AddWithValue("@IsDone", detail.IsDone);
                                updateCommand.Parameters.AddWithValue("@DailyID", detail.DailyID);
                                updateCommand.Parameters.AddWithValue("@UpdatedBy", detail.UpdatedBy);
                                updateCommand.Parameters.AddWithValue("@UpdatedAt", detail.UpdatedAt);

                                await updateCommand.ExecuteNonQueryAsync();
                            }
                            else
                            {
                                // Insert new detail
                                var insertCommand = new SqlCommand(
                                    @"INSERT INTO schedule_detail (ID, Schedule_ID, Planned_Date, Is_Done, Daily_ID, Created_By, Created_At, Updated_By, Updated_At) 
                                        VALUES (@ID, @ScheduleID, @PlannedDate, @IsDone,@DailyID, @CreatedBy, @CreatedAt, @UpdatedBy, @UpdatedAt)",
                                    connection, transaction);
                                insertCommand.Parameters.AddWithValue("@ID", Guid.NewGuid());
                                insertCommand.Parameters.AddWithValue("@ScheduleID", scheduleId);
                                insertCommand.Parameters.AddWithValue("@PlannedDate", detail.PlannedDate);
                                insertCommand.Parameters.AddWithValue("@IsDone", detail.IsDone);
                                insertCommand.Parameters.AddWithValue("@DailyID", Guid.Empty);
                                insertCommand.Parameters.AddWithValue("@CreatedBy", detail.CreatedBy);
                                insertCommand.Parameters.AddWithValue("@CreatedAt", detail.CreatedAt);
                                insertCommand.Parameters.AddWithValue("@UpdatedBy", detail.UpdatedBy);
                                insertCommand.Parameters.AddWithValue("@UpdatedAt", detail.UpdatedAt);

                                await insertCommand.ExecuteNonQueryAsync();
                            }
                        }

                        // Delete records that are no longer in the updated details
                        foreach (var existingDetail in existingDetails)
                        {
                            if (!updatedDetailsDict.ContainsKey(existingDetail.Key))
                            {
                                var deleteCommand = new SqlCommand(
                                    "DELETE FROM schedule_detail WHERE ID = @ID", connection, transaction);
                                deleteCommand.Parameters.AddWithValue("@ID", existingDetail.Key);
                                await deleteCommand.ExecuteNonQueryAsync();
                            }
                        }

                        // Commit the transaction if all commands succeed
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        // Rollback if any command fails
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }



    }

}
