using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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

        public async Task<IEnumerable<MOLModel>> GetAll()
        {
            var molList = new List<MOLModel>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand("SELECT * FROM MOL", connection);

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

                            // You may need to fetch StatusHistories and TrackingHistories in separate queries
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
                                CreateDate = statusReader.GetDateTime(statusReader.GetOrdinal("Create_Date")),
                                UpdateDate = statusReader.GetDateTime(statusReader.GetOrdinal("Update_Date"))
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
                    (ID, Kode_Number,Tanggal, WO, HM, Kode_Komponen, Part_Number, Description, Quantity, Categories, Remark, Request_By, Status, Version) 
                    VALUES 
                    (@ID, @KodeNumber,@Tanggal, @WorkOrder, @HourMeter, @KodeKomponen, @PartNumber, @Description, @Quantity, @Categories, @Remark, @RequestBy, @Status, @Version)", connection);

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
                    Status = @Status,
                    Version = @Version
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
                command.Parameters.AddWithValue("@Status", mol.Status);
                command.Parameters.AddWithValue("@Version", mol.Version + 1);

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
