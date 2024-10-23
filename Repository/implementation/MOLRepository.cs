using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using worklog_api.Model;

namespace worklog_api.Repository
{
    public class MOLRepository : IMOLRepository
    {
        private readonly string _connectionString;

        public MOLRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<MOLModel>> GetAll(int pageNumber, int pageSize, string sortBy, string sortDirection, DateTime? startDate, DateTime? endDate)
        {
            var molList = new List<MOLModel>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Calculate the number of rows to skip
                int offset = (pageNumber - 1) * pageSize;

                // Build dynamic SQL query based on input parameters
                var query = new StringBuilder(@"
                    SELECT * FROM MOL
                    WHERE 1 = 1");

                // Add date filtering based on available parameters
                if (startDate.HasValue && endDate.HasValue)
                {
                    query.Append(" AND Tanggal BETWEEN @StartDate AND @EndDate");
                }
                else if (startDate.HasValue)
                {
                    query.Append(" AND Tanggal >= @StartDate");
                }
                else if (endDate.HasValue)
                {
                    query.Append(" AND Tanggal <= @EndDate");
                }

                // Add sorting
                if (!string.IsNullOrEmpty(sortBy))
                {
                    query.Append($" ORDER BY {sortBy} {sortDirection}");
                }
                else
                {
                    query.Append(" ORDER BY Tanggal DESC"); // Default sorting by date in descending order
                }

                // Add pagination using OFFSET and FETCH
                query.Append(" OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY");

                var command = new SqlCommand(query.ToString(), connection);

                // Add parameters for pagination
                command.Parameters.AddWithValue("@Offset", offset);
                command.Parameters.AddWithValue("@PageSize", pageSize);

                // Add parameters for date filtering
                if (startDate.HasValue)
                {
                    command.Parameters.AddWithValue("@StartDate", startDate.Value);
                }

                if (endDate.HasValue)
                {
                    command.Parameters.AddWithValue("@EndDate", endDate.Value);
                }

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var mol = new MOLModel
                        {
                            ID = reader.GetGuid(reader.GetOrdinal("ID")),
                            KodeNumber = reader.GetString(reader.GetOrdinal("Kode_Number")),
                            Tanggal = reader.GetDateTime(reader.GetOrdinal("Tanggal")),
                            WorkOrder = reader.GetString(reader.GetOrdinal("WO")),
                            HourMeter = reader.GetInt32(reader.GetOrdinal("HM")),
                            KodeKomponen = reader.GetString(reader.GetOrdinal("Kode_Komponen")),
                            PartNumber = reader.GetString(reader.GetOrdinal("Part_Number")),
                            Description = reader.GetString(reader.GetOrdinal("Description")),
                            Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                            Categories = reader.GetString(reader.GetOrdinal("Categories")),
                            Remark = reader.GetString(reader.GetOrdinal("Remark")),
                            RequestBy = reader.GetString(reader.GetOrdinal("Request_By")),
                            Status = reader.GetString(reader.GetOrdinal("Status")),
                            Version = reader.GetInt32(reader.GetOrdinal("Version"))
                        };

                        molList.Add(mol);
                    }
                }
            }

            return molList;
        }


        public async Task<MOLModel> GetById(Guid id)
        {
            MOLModel mol = null;
            List<MOLTrackingHistoryModel> trackingHistoryList = new List<MOLTrackingHistoryModel>();
            List<StatusHistoryModel> statusHistoryList = new List<StatusHistoryModel>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Get MOL details
                var command = new SqlCommand("SELECT * FROM MOL WHERE ID = @ID", connection);
                command.Parameters.AddWithValue("@ID", id);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        mol = new MOLModel
                        {
                            ID = reader.GetGuid(reader.GetOrdinal("ID")),
                            KodeNumber = reader.GetString(reader.GetOrdinal("Kode_Number")),
                            Tanggal = reader.GetDateTime(reader.GetOrdinal("Tanggal")),
                            WorkOrder = reader.GetString(reader.GetOrdinal("WO")),
                            HourMeter = reader.GetInt32(reader.GetOrdinal("HM")),
                            KodeKomponen = reader.GetString(reader.GetOrdinal("Kode_Komponen")),
                            PartNumber = reader.GetString(reader.GetOrdinal("Part_Number")),
                            Description = reader.GetString(reader.GetOrdinal("Description")),
                            Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                            Categories = reader.GetString(reader.GetOrdinal("Categories")),
                            Remark = reader.GetString(reader.GetOrdinal("Remark")),
                            RequestBy = reader.GetString(reader.GetOrdinal("Request_By")),
                            Status = reader.GetString(reader.GetOrdinal("Status")),
                            Version = reader.GetInt32(reader.GetOrdinal("Version"))
                        };
                    }
                }

                if (mol != null)
                {
                    // Get MOL Tracking History
                    var trackingCommand = new SqlCommand(@"
                        SELECT * FROM MOL_Tracking_History 
                        WHERE MOL_ID = @MOL_ID", connection);
                    trackingCommand.Parameters.AddWithValue("@MOL_ID", mol.ID);

                    using (var trackingReader = await trackingCommand.ExecuteReaderAsync())
                    {
                        while (await trackingReader.ReadAsync())
                        {
                            trackingHistoryList.Add(new MOLTrackingHistoryModel
                            {
                                ID = trackingReader.GetGuid(trackingReader.GetOrdinal("ID")),
                                MOLID = trackingReader.GetGuid(trackingReader.GetOrdinal("MOL_ID")),
                                WRCode = trackingReader.GetString(trackingReader.GetOrdinal("WR_Code")),
                                Status = trackingReader.GetString(trackingReader.GetOrdinal("Status")),
                                AdditionalInfo = trackingReader.GetString(trackingReader.GetOrdinal("Additional_Info"))
                            });
                        }
                    }

                    // Get MOL Status History
                    var statusCommand = new SqlCommand(@"
                        SELECT * FROM Status_History_MOL 
                        WHERE MOL_ID = @MOL_ID", connection);
                    statusCommand.Parameters.AddWithValue("@MOL_ID", mol.ID);

                    using (var statusReader = await statusCommand.ExecuteReaderAsync())
                    {
                        while (await statusReader.ReadAsync())
                        {
                            statusHistoryList.Add(new StatusHistoryModel
                            {
                                ID = statusReader.GetGuid(statusReader.GetOrdinal("ID")),
                                MOLID = statusReader.GetGuid(statusReader.GetOrdinal("MOL_ID")),
                                Status = statusReader.GetString(statusReader.GetOrdinal("Status")),
                                Remark = statusReader.GetString(statusReader.GetOrdinal("Remark")),
                                CreatedAt = statusReader.GetDateTime(statusReader.GetOrdinal("Created_At")),
                                UpdatedAt = statusReader.GetDateTime(statusReader.GetOrdinal("Updated_At"))
                            });
                        }
                    }

                    // Assign the related lists to the MOL model
                    mol.TrackingHistories = trackingHistoryList;
                    mol.StatusHistories = statusHistoryList;
                }
            }

            return mol;
        }


        public async Task Create(MOLModel mol)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand(@"INSERT INTO MOL 
                    (ID, Kode_Number,Tanggal, WO, HM, Kode_Komponen, Part_Number, Description, Quantity, Categories, Remark, Request_By, Status, Version, Created_By, Updated_By, Created_At, Updated_At) 
                    VALUES 
                    (@ID, @KodeNumber,@Tanggal, @WorkOrder, @HourMeter, @KodeKomponen, @PartNumber, @Description, @Quantity, @Categories, @Remark, @RequestBy, @Status, @Version, @CreatedBy, @UpdatedBy, @CreatedAt, @UpdatedAt)", connection);

                command.Parameters.AddWithValue("@ID", mol.ID);
                command.Parameters.AddWithValue("@KodeNumber", mol.KodeNumber);
                command.Parameters.AddWithValue("@Tanggal", mol.Tanggal);
                command.Parameters.AddWithValue("@WorkOrder", mol.WorkOrder);
                command.Parameters.AddWithValue("@HourMeter", mol.HourMeter);
                command.Parameters.AddWithValue("@KodeKomponen", mol.KodeKomponen);
                command.Parameters.AddWithValue("@PartNumber", mol.PartNumber);
                command.Parameters.AddWithValue("@Description", mol.Description);
                command.Parameters.AddWithValue("@Quantity", mol.Quantity);
                command.Parameters.AddWithValue("@Categories", mol.Categories);
                command.Parameters.AddWithValue("@Remark", mol.Remark);
                command.Parameters.AddWithValue("@RequestBy", mol.RequestBy);
                command.Parameters.AddWithValue("@Status", mol.Status);
                command.Parameters.AddWithValue("@Version", mol.Version);
                command.Parameters.AddWithValue("@CreatedBy", mol.CreatedBy);
                command.Parameters.AddWithValue("@UpdatedBy", mol.UpdatedBy);
                command.Parameters.AddWithValue("@CreatedAt", mol.CreatedAt);
                command.Parameters.AddWithValue("@UpdatedAt", mol.UpdatedAt);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task Update(MOLModel mol)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand(@"UPDATE MOL SET 
                    Kode_Number = @KodeNumber, 
                    Tanggal = @Tanggal, 
                    WO = @WorkOrder, 
                    HM = @HourMeter, 
                    Kode_Komponen = @KodeKomponen, 
                    Part_Number = @PartNumber, 
                    Description = @Description, 
                    Quantity = @Quantity, 
                    Categories = @Categories, 
                    Remark = @Remark, 
                    Request_By = @RequestBy, 
                    Version = @Version,
                    Updated_By = @UpdatedBy,
                    Updated_At = @UpdatedAt
                    WHERE ID = @ID", connection);

                command.Parameters.AddWithValue("@ID", mol.ID);
                command.Parameters.AddWithValue("@KodeNumber", mol.KodeNumber);
                command.Parameters.AddWithValue("@Tanggal", mol.Tanggal);
                command.Parameters.AddWithValue("@WorkOrder", mol.WorkOrder);
                command.Parameters.AddWithValue("@HourMeter", mol.HourMeter);
                command.Parameters.AddWithValue("@KodeKomponen", mol.KodeKomponen);
                command.Parameters.AddWithValue("@PartNumber", mol.PartNumber);
                command.Parameters.AddWithValue("@Description", mol.Description);
                command.Parameters.AddWithValue("@Quantity", mol.Quantity);
                command.Parameters.AddWithValue("@Categories", mol.Categories);
                command.Parameters.AddWithValue("@Remark", mol.Remark);
                command.Parameters.AddWithValue("@RequestBy", mol.RequestBy);
                //command.Parameters.AddWithValue("@Status", mol.Status);
                command.Parameters.AddWithValue("@Version", mol.Version + 1);
                command.Parameters.AddWithValue("@UpdatedBy", mol.UpdatedBy);
                command.Parameters.AddWithValue("@UpdatedAt", mol.UpdatedAt);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task Delete(Guid id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {

                await connection.OpenAsync();
                var command = new SqlCommand("DELETE FROM MOL WHERE ID = @ID", connection);
                command.Parameters.AddWithValue("@ID", id);

                await command.ExecuteNonQueryAsync();
            }
        }
    }
}
