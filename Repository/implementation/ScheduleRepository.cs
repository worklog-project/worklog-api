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

        public async Task<List<ScheduleDTO>> GetScheduleDetailsByMonth(DateTime scheduleMonth)
        {
            var schedules = new List<ScheduleDTO>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Query to retrieve schedules by ScheduleMonth
                var scheduleCommand = new SqlCommand(@"
                    SELECT ID, EGI_ID, CN_ID, Schedule_Month, Created_By, Updated_By, Created_At, Updated_At 
                    FROM schedule 
                    WHERE YEAR(Schedule_Month) = @Year AND MONTH(Schedule_Month) = @Month", connection);

                scheduleCommand.Parameters.AddWithValue("@Year", scheduleMonth.Year);
                scheduleCommand.Parameters.AddWithValue("@Month", scheduleMonth.Month);

                using (var reader = await scheduleCommand.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var schedule = new ScheduleDTO
                        {
                            EGIID = reader.GetGuid(reader.GetOrdinal("EGI_ID")),
                            CNID = reader.GetGuid(reader.GetOrdinal("CN_ID")),
                            ScheduleMonth = reader.GetDateTime(reader.GetOrdinal("Schedule_Month")),
                            ScheduleDetails = new List<ScheduleDetailDTO>()
                        };

                        // Retrieve the schedule details for each schedule
                        var scheduleId = reader.GetGuid(reader.GetOrdinal("ID"));
                        await LoadScheduleDetails(schedule, scheduleId, connection);

                        schedules.Add(schedule);
                    }
                }
            }

            return schedules;
        }

        private async Task LoadScheduleDetails(ScheduleDTO schedule, Guid scheduleId, SqlConnection connection)
        {
            // Query to retrieve schedule details for a specific schedule ID
            var detailCommand = new SqlCommand(@"
                SELECT ID, Planned_Date, Is_Done, Daily_ID, Created_By, Updated_By, Created_At, Updated_At 
                FROM schedule_detail 
                WHERE Schedule_ID = @ScheduleID", connection);

            detailCommand.Parameters.AddWithValue("@ScheduleID", scheduleId);

            using (var reader = await detailCommand.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var scheduleDetail = new ScheduleDetailDTO
                    {
                        PlannedDate = reader.GetDateTime(reader.GetOrdinal("Planned_Date")).ToString("yyyy-MM-dd"),
                    };

                    schedule.ScheduleDetails.Add(scheduleDetail);
                }
            }
        }



    }

}
