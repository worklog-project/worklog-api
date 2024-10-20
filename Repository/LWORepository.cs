using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using worklog_api.Model;

namespace worklog_api.Repository
{
    public class LWORepository : ILWORepository
    {
        private readonly string _connectionString;

        public LWORepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<LWOModel>> GetAll()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var command = new SqlCommand("SELECT * FROM LWO", connection);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    var lwos = new List<LWOModel>();
                    while (reader.Read())
                    {
                        lwos.Add(new LWOModel
                        {
                            ID = reader.GetGuid(reader.GetOrdinal("ID")),
                            WONumber = reader.GetString(reader.GetOrdinal("WO_Number")),
                            WODate = reader.GetDateTime(reader.GetOrdinal("WO_Date")),
                            WOType = reader.GetString(reader.GetOrdinal("WO_Type")),
                            Activity = reader.GetString(reader.GetOrdinal("Activity")),
                            HourMeter = reader.GetInt32(reader.GetOrdinal("HM")),
                            TimeStart = reader.GetString(reader.GetOrdinal("Time_Start")),
                            TimeEnd = reader.GetString(reader.GetOrdinal("Time_End")),
                            PIC = reader.GetString(reader.GetOrdinal("PIC")),
                            LWOType = reader.GetString(reader.GetOrdinal("LWO_Type")),
                            Version = reader.GetInt32(reader.GetOrdinal("Version"))
                        });
                    }
                    return lwos;
                }
            }
        }

        public async Task<LWOModel> GetById(Guid id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var command = new SqlCommand(@"
            SELECT 
                lwo.ID AS LWO_ID, lwo.WO_Number, lwo.WO_Date, lwo.WO_Type, lwo.Activity, lwo.HM, 
                lwo.Time_Start, lwo.Time_End, lwo.PIC, lwo.LWO_Type, lwo.Version,
                meta.ID AS Metadata_ID, meta.Komponen, meta.Keterangan, meta.Kode_Unit, meta.Version AS MetadataVersion,
                img.ID AS Image_ID, img.Path, img.Image_Name
            FROM LWO lwo
            LEFT JOIN LWO_Metadata meta ON lwo.ID = meta.LWO_ID
            LEFT JOIN LWO_Image img ON meta.ID = img.LWO_Metadata_ID
            WHERE lwo.ID = @ID", connection);

                command.Parameters.AddWithValue("@ID", id);

                var lwoDict = new Dictionary<Guid, LWOModel>(); // To manage duplicate LWO records due to JOIN
                var metadataDict = new Dictionary<Guid, LWOMetadataModel>(); // To manage duplicate metadata

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        // Retrieve the LWO record
                        var lwoId = reader.GetGuid(reader.GetOrdinal("LWO_ID"));
                        if (!lwoDict.TryGetValue(lwoId, out var lwo))
                        {
                            lwo = new LWOModel
                            {
                                ID = lwoId,
                                WONumber = reader.GetString(reader.GetOrdinal("WO_Number")),
                                WODate = reader.GetDateTime(reader.GetOrdinal("WO_Date")),
                                WOType = reader.GetString(reader.GetOrdinal("WO_Type")),
                                Activity = reader.GetString(reader.GetOrdinal("Activity")),
                                HourMeter = reader.GetInt32(reader.GetOrdinal("HM")),
                                TimeStart = reader.GetString(reader.GetOrdinal("Time_Start")),
                                TimeEnd = reader.GetString(reader.GetOrdinal("Time_End")),
                                PIC = reader.GetString(reader.GetOrdinal("PIC")),
                                LWOType = reader.GetString(reader.GetOrdinal("LWO_Type")),
                                Version = reader.GetInt32(reader.GetOrdinal("Version")),
                                Metadata = new List<LWOMetadataModel>()
                            };
                            lwoDict[lwoId] = lwo;
                        }

                        // Retrieve the metadata if available
                        if (!reader.IsDBNull(reader.GetOrdinal("Metadata_ID")))
                        {
                            var metadataId = reader.GetGuid(reader.GetOrdinal("Metadata_ID"));
                            if (!metadataDict.TryGetValue(metadataId, out var metadata))
                            {
                                metadata = new LWOMetadataModel
                                {
                                    ID = metadataId,
                                    Komponen = reader.GetString(reader.GetOrdinal("Komponen")),
                                    Keterangan = reader.GetString(reader.GetOrdinal("Keterangan")),
                                    KodeUnit = reader.GetString(reader.GetOrdinal("Kode_Unit")),
                                    Version = reader.GetInt32(reader.GetOrdinal("MetadataVersion")),
                                    Images = new List<LWOImageModel>()
                                };
                                metadataDict[metadataId] = metadata;
                                lwo.Metadata.Add(metadata);
                            }

                            // Retrieve the image if available
                            if (!reader.IsDBNull(reader.GetOrdinal("Image_ID")))
                            {
                                var image = new LWOImageModel
                                {
                                    ID = reader.GetGuid(reader.GetOrdinal("Image_ID")),
                                    Path = reader.GetString(reader.GetOrdinal("Path")),
                                    ImageName = reader.GetString(reader.GetOrdinal("Image_Name"))
                                };
                                metadata.Images.Add(image);
                            }
                        }
                    }
                }

                return lwoDict.Values.FirstOrDefault(); // Return the first (and only) LWOModel from the dictionary
            }
        }


        public async Task Create(LWOModel lwo)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Insert LWO Record
                        var command = new SqlCommand(@"
                    INSERT INTO LWO (ID, WO_Number, WO_Date, WO_Type, Activity, HM, Time_Start, Time_End, PIC, LWO_Type, Version)
                    VALUES (@ID, @WONumber, @WODate, @WOType, @Activity, @HM, @TimeStart, @TimeEnd, @PIC, @LWOType, @Version)", connection, transaction);

                        command.Parameters.AddWithValue("@ID", lwo.ID);
                        command.Parameters.AddWithValue("@WONumber", lwo.WONumber);
                        command.Parameters.AddWithValue("@WODate", lwo.WODate);
                        command.Parameters.AddWithValue("@WOType", lwo.WOType);
                        command.Parameters.AddWithValue("@Activity", lwo.Activity);
                        command.Parameters.AddWithValue("@HM", lwo.HourMeter);
                        command.Parameters.AddWithValue("@TimeStart", lwo.TimeStart);
                        command.Parameters.AddWithValue("@TimeEnd", lwo.TimeEnd);
                        command.Parameters.AddWithValue("@PIC", lwo.PIC);
                        command.Parameters.AddWithValue("@LWOType", lwo.LWOType);
                        command.Parameters.AddWithValue("@Version", lwo.Version);

                        await command.ExecuteNonQueryAsync();

                        // Insert LWO Metadata
                        foreach (var metadata in lwo.Metadata)
                        {
                            var metadataCommand = new SqlCommand(@"
                        INSERT INTO LWO_Metadata (ID, LWO_ID, Komponen, Keterangan, Kode_Unit, Version)
                        VALUES (@ID, @LWOID, @Komponen, @Keterangan, @KodeUnit, @Version)", connection, transaction);

                            var metadataId = Guid.NewGuid(); // Generate new ID for metadata
                            metadataCommand.Parameters.AddWithValue("@ID", metadataId);
                            metadataCommand.Parameters.AddWithValue("@LWOID", lwo.ID);
                            metadataCommand.Parameters.AddWithValue("@Komponen", metadata.Komponen);
                            metadataCommand.Parameters.AddWithValue("@Keterangan", metadata.Keterangan);
                            metadataCommand.Parameters.AddWithValue("@KodeUnit", metadata.KodeUnit);
                            metadataCommand.Parameters.AddWithValue("@Version", metadata.Version);

                            await metadataCommand.ExecuteNonQueryAsync();

                            // Insert LWO Images for the current metadata
                            foreach (var image in metadata.Images)
                            {
                                var imageCommand = new SqlCommand(@"
                            INSERT INTO LWO_Image (ID, LWO_Metadata_ID, Path, Image_Name)
                            VALUES (@ID, @MetadataID, @Path, @ImageName)", connection, transaction);

                                imageCommand.Parameters.AddWithValue("@ID", Guid.NewGuid()); // Generate new ID for image
                                imageCommand.Parameters.AddWithValue("@MetadataID", metadataId);
                                imageCommand.Parameters.AddWithValue("@Path", image.Path);
                                imageCommand.Parameters.AddWithValue("@ImageName", image.ImageName);

                                await imageCommand.ExecuteNonQueryAsync();
                            }
                        }

                        // Commit the transaction if everything is successful
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        // Rollback the transaction in case of an error
                        transaction.Rollback();
                        throw new Exception($"Error creating LWO: {ex.Message}");
                    }
                }
            }
        }

        public async Task Update(LWOModel lwo)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var command = new SqlCommand(@"
                    UPDATE LWO SET 
                    WO_Number = @WONumber,
                    WO_Type = @WOType,
                    Activity = @Activity,
                    HM = @HM,
                    Time_Start = @TimeStart, 
                    Time_End = @TimeEnd,
                    PIC = @PIC,
                    LWO_Type = @LWOType,
                    Version = @Version
                    WHERE ID = @ID", connection);

                command.Parameters.AddWithValue("@ID", lwo.ID);
                command.Parameters.AddWithValue("@WONumber", lwo.WONumber);
                command.Parameters.AddWithValue("@WOType", lwo.WOType);
                command.Parameters.AddWithValue("@Activity", lwo.Activity);
                command.Parameters.AddWithValue("@HM", lwo.HourMeter);
                command.Parameters.AddWithValue("@TimeStart", lwo.TimeStart);
                command.Parameters.AddWithValue("@TimeEnd", lwo.TimeEnd);
                command.Parameters.AddWithValue("@PIC", lwo.PIC);
                command.Parameters.AddWithValue("@LWOType", lwo.LWOType);
                command.Parameters.AddWithValue("@Version", lwo.Version + 1);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task Delete(Guid id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var command = new SqlCommand("DELETE FROM LWO WHERE ID = @ID", connection);
                command.Parameters.AddWithValue("@ID", id);

                await command.ExecuteNonQueryAsync();
            }
        }
    }
}
