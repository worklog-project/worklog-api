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

        public async Task<(IEnumerable<MOLModel> mols, int totalCount)> GetAll(int pageNumber, int pageSize, string sortBy, string sortDirection, DateTime? startDate, DateTime? endDate, string requestBy, string status)
        {
            var molList = new List<MOLModel>();
            int totalCount = 0;

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Build the query to fetch MOL records and count in one go
                var query = @"
                    -- Fetch MOL records
                    SELECT * FROM MOL 
                    WHERE (@startDate IS NULL OR CAST(Tanggal AS DATE) >= CAST(@startDate AS DATE)) 
                    AND (@endDate IS NULL OR CAST(Tanggal AS DATE) <= CAST(@endDate AS DATE))
                    AND (@requestBy IS NULL OR Request_By LIKE '%' + @requestBy + '%')
                    AND (@status IS NULL OR Status = @status)
                    ORDER BY " + sortBy + " " + sortDirection + @" 
                    OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;


                    -- Get total count of matching records
                    SELECT COUNT(*) FROM MOL
                    WHERE (@startDate IS NULL OR CAST(Tanggal AS DATE) >= CAST(@startDate AS DATE))
                    AND (@endDate IS NULL OR CAST(Tanggal AS DATE) <= CAST(@endDate AS DATE))
                    AND (@requestBy IS NULL OR Request_By LIKE '%' + @requestBy + '%')
                    AND (@status IS NULL OR Status = @status);
                    ";

                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@startDate", (object)startDate ?? DBNull.Value);
                command.Parameters.AddWithValue("@endDate", (object)endDate ?? DBNull.Value);
                command.Parameters.AddWithValue("@requestBy", (object)requestBy ?? DBNull.Value);
                command.Parameters.AddWithValue("@status", (object)status ?? DBNull.Value);
                command.Parameters.AddWithValue("@offset", (pageNumber - 1) * pageSize);
                command.Parameters.AddWithValue("@pageSize", pageSize);

                // Execute the command and process the results
                using (var reader = await command.ExecuteReaderAsync())
                {
                    // First result set: MOL records
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
                            Version = reader.GetInt32(reader.GetOrdinal("Version")),
                            CreatedBy = reader.GetString(reader.GetOrdinal("Created_By")),
                            UpdatedBy = reader.GetString(reader.GetOrdinal("Updated_By")),
                            CreatedAt = reader.GetDateTime(reader.GetOrdinal("Created_At")),
                            UpdatedAt = reader.GetDateTime(reader.GetOrdinal("Updated_At"))
                        };
                        molList.Add(mol);
                    }

                    // Move to the second result set: Total count of records
                    if (await reader.NextResultAsync() && await reader.ReadAsync())
                    {
                        totalCount = reader.GetInt32(0);
                    }
                }
            }

            return (molList, totalCount);
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
                            Version = reader.GetInt32(reader.GetOrdinal("Version")),
                            QuantityApproved = reader.GetInt32(reader.GetOrdinal("Approved_Quantity")),
                            CreatedBy = reader.GetString(reader.GetOrdinal("Created_By")),
                            UpdatedBy = reader.GetString(reader.GetOrdinal("Updated_By")),
                            CreatedAt = reader.GetDateTime(reader.GetOrdinal("Created_At")),
                            UpdatedAt = reader.GetDateTime(reader.GetOrdinal("Updated_At"))
                        };
                    }
                }

                if (mol != null)
                {
                    // Get MOL Tracking History
                    var trackingCommand = new SqlCommand(@"
                        SELECT * FROM MOL_Tracking_History 
                        WHERE MOL_ID = @MOL_ID
                        ORDER BY Created_At DESC", connection);
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
                                AdditionalInfo = trackingReader.GetString(trackingReader.GetOrdinal("Additional_Info")),
                                CreatedAt = trackingReader.GetDateTime(trackingReader.GetOrdinal("Created_At")),
                                UpdatedAt = trackingReader.GetDateTime(trackingReader.GetOrdinal("Updated_At")),
                                CreatedBy = trackingReader.GetString(trackingReader.GetOrdinal("Created_By")),
                                UpdatedBy = trackingReader.GetString(trackingReader.GetOrdinal("Updated_By")),
                            });
                        }
                    }

                    // Get MOL Status History
                    var statusCommand = new SqlCommand(@"
                        SELECT * FROM Status_History_MOL 
                        WHERE MOL_ID = @MOL_ID
                        ORDER BY Created_At DESC", connection);
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
                                UpdatedAt = statusReader.GetDateTime(statusReader.GetOrdinal("Updated_At")),
                                CreatedBy = statusReader.GetString(statusReader.GetOrdinal("Created_By")),
                                UpdatedBy = statusReader.GetString(statusReader.GetOrdinal("Updated_By")),
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

            Console.WriteLine("Create MOL");
            Console.WriteLine(mol);

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand(@"INSERT INTO MOL 
                    (ID, Kode_Number,Tanggal, WO, HM, Kode_Komponen, Part_Number, Description, Quantity, Categories, Remark, Request_By, Status, Version, Created_By, Updated_By, Created_At, Updated_At, Approved_Quantity) 
                    VALUES 
                    (@ID, @KodeNumber,@Tanggal, @WorkOrder, @HourMeter, @KodeKomponen, @PartNumber, @Description, @Quantity, @Categories, @Remark, @RequestBy, @Status, @Version, @CreatedBy, @UpdatedBy, @CreatedAt, @UpdatedAt, @ApprovedQuantity)", connection);

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
                command.Parameters.AddWithValue("@ApprovedQuantity", mol.QuantityApproved);

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

        public async Task UpdateApprovedQuantity(Guid id, int quantityApproved)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            var command = new SqlCommand("UPDATE MOL SET Approved_Quantity = @QuantityApproved WHERE ID = @ID", connection);
            command.Parameters.AddWithValue("@QuantityApproved", quantityApproved);
            command.Parameters.AddWithValue("@ID", id);

            await command.ExecuteNonQueryAsync();
        }
    }
}
