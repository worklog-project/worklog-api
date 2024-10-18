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
                var command = new SqlCommand("SELECT * FROM mol", connection);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var mol = new MOLModel
                        {
                            ID = reader.GetGuid(reader.GetOrdinal("ID")),
                            KodeNumber = reader.GetString(reader.GetOrdinal("KodeNumber")),
                            Tanggal = reader.GetDateTime(reader.GetOrdinal("Tanggal")),
                            WorkOrder = reader.GetString(reader.GetOrdinal("WorkOrder")),
                            HourMeter = reader.GetInt32(reader.GetOrdinal("HourMeter")),
                            KodeKomponen = reader.GetString(reader.GetOrdinal("KodeKomponen")),
                            PartNumber = reader.GetString(reader.GetOrdinal("PartNumber")),
                            Description = reader.GetString(reader.GetOrdinal("Description")),
                            Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                            Categories = reader.GetString(reader.GetOrdinal("Categories")),
                            Remark = reader.GetString(reader.GetOrdinal("Remark")),
                            RequestBy = reader.GetString(reader.GetOrdinal("RequestBy")),
                            Status = reader.GetString(reader.GetOrdinal("Status"))
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

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand("SELECT * FROM mol WHERE ID = @ID", connection);
                command.Parameters.AddWithValue("@ID", id);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        mol = new MOLModel
                        {
                            ID = reader.GetGuid(reader.GetOrdinal("ID")),
                            KodeNumber = reader.GetString(reader.GetOrdinal("KodeNumber")),
                            Tanggal = reader.GetDateTime(reader.GetOrdinal("Tanggal")),
                            WorkOrder = reader.GetString(reader.GetOrdinal("WorkOrder")),
                            HourMeter = reader.GetInt32(reader.GetOrdinal("HourMeter")),
                            KodeKomponen = reader.GetString(reader.GetOrdinal("KodeKomponen")),
                            PartNumber = reader.GetString(reader.GetOrdinal("PartNumber")),
                            Description = reader.GetString(reader.GetOrdinal("Description")),
                            Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                            Categories = reader.GetString(reader.GetOrdinal("Categories")),
                            Remark = reader.GetString(reader.GetOrdinal("Remark")),
                            RequestBy = reader.GetString(reader.GetOrdinal("RequestBy")),
                            Status = reader.GetString(reader.GetOrdinal("Status"))
                        };
                    }
                }
            }

            return mol;
        }

        public async Task Create(MOLModel mol)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand(@"INSERT INTO mol 
                    (ID, KodeNumber, WorkOrder, HourMeter, KodeKomponen, PartNumber, Description, Quantity, Categories, Remark, RequestBy, Status) 
                    VALUES 
                    (@ID, @KodeNumber, @WorkOrder, @HourMeter, @KodeKomponen, @PartNumber, @Description, @Quantity, @Categories, @Remark, @RequestBy, @Status)", connection);

                command.Parameters.AddWithValue("@ID", mol.ID);
                command.Parameters.AddWithValue("@KodeNumber", mol.KodeNumber);
                //command.Parameters.AddWithValue("@Tanggal", mol.Tanggal);
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

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task Update(MOLModel mol)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand(@"UPDATE mol SET 
                    KodeNumber = @KodeNumber, 
                    Tanggal = @Tanggal, 
                    WorkOrder = @WorkOrder, 
                    HourMeter = @HourMeter, 
                    KodeKomponen = @KodeKomponen, 
                    PartNumber = @PartNumber, 
                    Description = @Description, 
                    Quantity = @Quantity, 
                    Categories = @Categories, 
                    Remark = @Remark, 
                    RequestBy = @RequestBy, 
                    Status = @Status
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

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task Delete(Guid id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand("DELETE FROM mol WHERE ID = @ID", connection);
                command.Parameters.AddWithValue("@ID", id);

                await command.ExecuteNonQueryAsync();
            }
        }
    }
}
