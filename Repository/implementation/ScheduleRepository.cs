using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using worklog_api.Model;

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

                        // Insert into the schedule table
                        var scheduleCommand = new SqlCommand(@"
                        INSERT INTO schedule 
                        (ID, EGI_ID, CN_ID, Created_By, Updated_By, Created_At, Updated_At) 
                        VALUES 
                        (@ID, @EGIID, @CNID, @CreatedBy, @UpdatedBy, @CreatedAt, @UpdatedAt)", connection, transaction);

                        scheduleCommand.Parameters.AddWithValue("@ID", schedule.ID);
                        scheduleCommand.Parameters.AddWithValue("@EGIID", schedule.EGIID);
                        scheduleCommand.Parameters.AddWithValue("@CNID", schedule.CNID);
                        scheduleCommand.Parameters.AddWithValue("@CreatedBy", schedule.CreatedBy);
                        scheduleCommand.Parameters.AddWithValue("@UpdatedBy", schedule.UpdatedBy);
                        scheduleCommand.Parameters.AddWithValue("@CreatedAt", schedule.CreatedAt);
                        scheduleCommand.Parameters.AddWithValue("@UpdatedAt", schedule.UpdatedAt);

                        await scheduleCommand.ExecuteNonQueryAsync();

                        // Use the same ID to insert related records in schedule_details
                        foreach (var scheduleDetail in schedule.ScheduleDetails)
                        {
                            var detailCommand = new SqlCommand(@"
                            INSERT INTO schedule_details 
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


    }

}
