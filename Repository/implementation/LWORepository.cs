using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.CodeAnalysis;
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

        public async Task<(IEnumerable<LWOModel>, int totalCount)> GetAll(int pageNumber, int pageSize, string sortBy, string sortDirection, DateTime? startDate, DateTime? endDate, string requestBy)
        {
            var lwoList = new List<LWOModel>();
            int totalCount = 0;

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Build the SQL query
                var query = $@"
                -- Fetch paginated LWO records
                SELECT * FROM LWO 
                WHERE (@startDate IS NULL OR CAST(WO_Date AS DATE) >= CAST(@startDate AS DATE)) 
                AND (@endDate IS NULL OR CAST(WO_Date AS DATE) <= CAST(@endDate AS DATE))
                AND (@requestBy IS NULL OR PIC LIKE '%' + @requestBy + '%')
                ORDER BY {sortBy} {sortDirection}
                OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;

                -- Fetch total record count
                SELECT COUNT(*) FROM LWO
                WHERE (@startDate IS NULL OR CAST(WO_Date AS DATE) >= CAST(@startDate AS DATE))
                AND (@endDate IS NULL OR CAST(WO_Date AS DATE) <= CAST(@endDate AS DATE))
                AND (@requestBy IS NULL OR PIC LIKE '%' + @requestBy + '%');
        ";

                using (var command = new SqlCommand(query, connection))
                {
                    // Add parameters
                    command.Parameters.AddWithValue("@startDate", (object)startDate ?? DBNull.Value);
                    command.Parameters.AddWithValue("@endDate", (object)endDate ?? DBNull.Value);
                    command.Parameters.AddWithValue("@requestBy", (object)requestBy ?? DBNull.Value);
                    command.Parameters.AddWithValue("@offset", (pageNumber - 1) * pageSize);
                    command.Parameters.AddWithValue("@pageSize", pageSize);

                    // Execute and read results
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        // First result set: LWO records
                        while (await reader.ReadAsync())
                        {
                            lwoList.Add(new LWOModel
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

                        // Move to the second result set: Total count
                        if (await reader.NextResultAsync() && await reader.ReadAsync())
                        {
                            totalCount = reader.GetInt32(0);
                        }
                    }
                }
            }

            return (lwoList, totalCount);
        }
        public async Task<LWOModel> GetById(Guid id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var command = new SqlCommand(@"
                SELECT 
                lwo.ID AS LWO_ID, lwo.WO_Number, lwo.WO_Date, lwo.WO_Type, lwo.Activity, lwo.HM, 
                lwo.Time_Start, lwo.Time_End, lwo.PIC, lwo.LWO_Type, lwo.Version AS LWO_Version, lwo.Kode_Unit AS Kode_Unit,
                lwo.Group_Leader AS Group_Leader,
                lwo.Created_By AS LWO_Created_By, lwo.Updated_By AS LWO_Updated_By,
                lwo.Created_At AS LWO_Created_At, lwo.Updated_At AS LWO_Updated_At,
                meta.ID AS Metadata_ID, meta.Komponen, meta.Keterangan, meta.Version AS MetadataVersion,
                meta.Created_By AS Metadata_Created_By, meta.Updated_By AS Metadata_Updated_By,
                meta.Created_At AS Metadata_Created_At, meta.Updated_At AS Metadata_Updated_At,
                img.ID AS Image_ID, img.Path, img.Image_Name,
                img.Created_By AS Image_Created_By, img.Updated_By AS Image_Updated_By,
                img.Created_At AS Image_Created_At, img.Updated_At AS Image_Updated_At
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
                                KodeUnit = reader.GetString(reader.GetOrdinal("Kode_Unit")),
                                GroupLeader = reader.GetString(reader.GetOrdinal("Group_Leader")),
                                LWOType = reader.GetString(reader.GetOrdinal("LWO_Type")),
                                Version = reader.GetInt32(reader.GetOrdinal("LWO_Version")),
                                CreatedBy = reader.GetString(reader.GetOrdinal("LWO_Created_By")),
                                UpdatedBy = reader.GetString(reader.GetOrdinal("LWO_Updated_By")),
                                CreatedAt = reader.GetDateTime(reader.GetOrdinal("LWO_Created_At")),
                                UpdatedAt = reader.GetDateTime(reader.GetOrdinal("LWO_Updated_At")),
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
                                    LWOID = reader.GetGuid(reader.GetOrdinal("LWO_ID")),
                                    Keterangan = reader.GetString(reader.GetOrdinal("Keterangan")),
                                    Version = reader.GetInt32(reader.GetOrdinal("MetadataVersion")),
                                    CreatedBy = reader.GetString(reader.GetOrdinal("Metadata_Created_By")),
                                    UpdatedBy = reader.GetString(reader.GetOrdinal("Metadata_Updated_By")),
                                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("Metadata_Created_At")),
                                    UpdatedAt = reader.GetDateTime(reader.GetOrdinal("Metadata_Updated_At")),
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
                                    LWOMetadataID = reader.GetGuid(reader.GetOrdinal("Metadata_ID")),
                                    Path = reader.GetString(reader.GetOrdinal("Path")),
                                    ImageName = reader.GetString(reader.GetOrdinal("Image_Name")),
                                    CreatedBy = reader.GetString(reader.GetOrdinal("Image_Created_By")),
                                    UpdatedBy = reader.GetString(reader.GetOrdinal("Image_Updated_By")),
                                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("Image_Created_At")),
                                    UpdatedAt = reader.GetDateTime(reader.GetOrdinal("Image_Updated_At"))
                                };
                                metadata.Images.Add(image);
                            }
                        }
                    }
                }

                return lwoDict.Values.FirstOrDefault(); // Return the first (and only) LWOModel from the dictionary
            }
        }
        public async Task<Guid> Create(LWOModel lwo)
        {
            Guid id = Guid.Empty;
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Insert LWO Record
                        // Insert LWO Record
                        var command = new SqlCommand(@"
                        INSERT INTO LWO (ID, WO_Number, WO_Date, WO_Type, Activity, HM, Time_Start, Time_End, PIC, LWO_Type, Version, Kode_Unit, Group_Leader, Created_By, Updated_By, Created_At, Updated_At)
                        VALUES (@ID, @WONumber, @WODate, @WOType, @Activity, @HM, @TimeStart, @TimeEnd, @PIC, @LWOType, @Version, @KodeUnit, @GroupLeader, @CreatedBy, @UpdatedBy, @CreatedAt, @UpdatedAt)", connection, transaction);

                        // Add null checks and default values
                        id = Guid.NewGuid(); // Generate new ID for LWO
                        command.Parameters.AddWithValue("@ID", id);
                        command.Parameters.AddWithValue("@WONumber", lwo.WONumber ?? string.Empty);
                        command.Parameters.AddWithValue("@WODate", lwo.WODate != null);
                        command.Parameters.AddWithValue("@WOType", lwo.WOType ?? string.Empty);
                        command.Parameters.AddWithValue("@Activity", lwo.Activity ?? string.Empty);
                        command.Parameters.AddWithValue("@HM", lwo.HourMeter);
                        command.Parameters.AddWithValue("@TimeStart", lwo.TimeStart);
                        command.Parameters.AddWithValue("@TimeEnd", lwo.TimeEnd);
                        command.Parameters.AddWithValue("@PIC", lwo.PIC ?? string.Empty);
                        command.Parameters.AddWithValue("@LWOType", lwo.LWOType ?? string.Empty);
                        command.Parameters.AddWithValue("@Version", lwo.Version);
                        command.Parameters.AddWithValue("@KodeUnit", lwo.KodeUnit ?? string.Empty);
                        command.Parameters.AddWithValue("@GroupLeader", lwo.GroupLeader);

                        // Ensure CreatedBy and UpdatedBy are not null
                        command.Parameters.AddWithValue("@CreatedBy", lwo.CreatedBy ?? "System");
                        command.Parameters.AddWithValue("@UpdatedBy", lwo.UpdatedBy ?? "System");

                        // Use current time if CreatedAt or UpdatedAt are null
                        command.Parameters.AddWithValue("@CreatedAt", lwo.CreatedAt);
                        command.Parameters.AddWithValue("@UpdatedAt", lwo.UpdatedAt);

                        await command.ExecuteScalarAsync();

                        // Insert LWO Metadata
                        foreach (var metadata in lwo.Metadata)
                        {
                            var metadataCommand = new SqlCommand(@"
                            INSERT INTO LWO_Metadata (ID, LWO_ID, Komponen, Keterangan, Version,Created_By, Updated_By, Created_At, Updated_At)
                            VALUES (@ID, @LWOID, @Komponen, @Keterangan, @Version, @CreatedBy, @UpdatedBy, @CreatedAt, @UpdatedAt)", connection, transaction);

                            var metadataId = Guid.NewGuid(); // Generate new ID for metadata
                            metadataCommand.Parameters.AddWithValue("@ID", metadataId);
                            metadataCommand.Parameters.AddWithValue("@LWOID", id);
                            metadataCommand.Parameters.AddWithValue("@Komponen", metadata.Komponen);
                            metadataCommand.Parameters.AddWithValue("@Keterangan", metadata.Keterangan);
                            metadataCommand.Parameters.AddWithValue("@Version", metadata.Version);
                            metadataCommand.Parameters.AddWithValue("@CreatedBy", lwo.CreatedBy);
                            metadataCommand.Parameters.AddWithValue("@UpdatedBy", lwo.UpdatedBy);
                            metadataCommand.Parameters.AddWithValue("@CreatedAt", lwo.CreatedAt);
                            metadataCommand.Parameters.AddWithValue("@UpdatedAt", lwo.UpdatedAt);

                            await metadataCommand.ExecuteNonQueryAsync();

                            // Insert LWO Images for the current metadata
                            foreach (var image in metadata.Images)
                            {
                                var imageCommand = new SqlCommand(@"
                                INSERT INTO LWO_Image (ID, LWO_Metadata_ID, Path, Image_Name,Created_By, Updated_By, Created_At, Updated_At)
                                VALUES (@ID, @MetadataID, @Path, @ImageName,@CreatedBy, @UpdatedBy, @CreatedAt, @UpdatedAt)", connection, transaction);

                                imageCommand.Parameters.AddWithValue("@ID", Guid.NewGuid()); // Generate new ID for image
                                imageCommand.Parameters.AddWithValue("@MetadataID", metadataId);
                                imageCommand.Parameters.AddWithValue("@Path", image.Path);
                                imageCommand.Parameters.AddWithValue("@ImageName", image.ImageName);
                                imageCommand.Parameters.AddWithValue("@CreatedBy", lwo.CreatedBy);
                                imageCommand.Parameters.AddWithValue("@UpdatedBy", lwo.UpdatedBy);
                                imageCommand.Parameters.AddWithValue("@CreatedAt", lwo.CreatedAt);
                                imageCommand.Parameters.AddWithValue("@UpdatedAt", lwo.UpdatedAt);

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
            return id;
        }
        public async Task CreateMetadataByLWOID(LWOMetadataModel metadata)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {

                            var metadataId = Guid.NewGuid(); // Generate new ID for metadata
                            var command = new SqlCommand(@"
                            INSERT INTO LWO_Metadata (ID, LWO_ID, Komponen, Keterangan, Version, Created_By, Created_At, Updated_By, Updated_At)
                            VALUES (@ID, @LWOID, @Komponen, @Keterangan, @Version, @CreatedBy, @CreatedAt, @UpdatedBy, @UpdatedAt)", connection, transaction);

                            command.Parameters.AddWithValue("@ID", metadataId);
                            command.Parameters.AddWithValue("@LWOID", metadata.LWOID);
                            command.Parameters.AddWithValue("@Komponen", metadata.Komponen);
                            command.Parameters.AddWithValue("@Keterangan", metadata.Keterangan);
                            command.Parameters.AddWithValue("@Version", metadata.Version);
                            command.Parameters.AddWithValue("@CreatedBy", metadata.CreatedBy);
                            command.Parameters.AddWithValue("@CreatedAt", metadata.CreatedAt);
                            command.Parameters.AddWithValue("@UpdatedBy", metadata.UpdatedBy);
                            command.Parameters.AddWithValue("@UpdatedAt", metadata.UpdatedAt);

                            await command.ExecuteNonQueryAsync();

                            // Insert LWO Images for the current metadata
                            foreach (var image in metadata.Images)
                            {
                                var imageCommand = new SqlCommand(@"
                                INSERT INTO LWO_Image (ID, LWO_Metadata_ID, Path, Image_Name,Created_By, Updated_By, Created_At, Updated_At)
                                VALUES (@ID, @MetadataID, @Path, @ImageName,@CreatedBy, @UpdatedBy, @CreatedAt, @UpdatedAt)", connection, transaction);

                                imageCommand.Parameters.AddWithValue("@ID", metadataId); // Generate new ID for image
                                imageCommand.Parameters.AddWithValue("@MetadataID", metadataId);
                                imageCommand.Parameters.AddWithValue("@Path", image.Path);
                                imageCommand.Parameters.AddWithValue("@ImageName", image.ImageName);
                                imageCommand.Parameters.AddWithValue("@CreatedBy", metadata.CreatedBy);
                                imageCommand.Parameters.AddWithValue("@UpdatedBy", metadata.UpdatedBy);
                                imageCommand.Parameters.AddWithValue("@CreatedAt", metadata.CreatedAt);
                                imageCommand.Parameters.AddWithValue("@UpdatedAt", metadata.UpdatedAt);

                                await imageCommand.ExecuteNonQueryAsync();
                            }
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw new Exception($"Error creating metadata: {ex.Message}");
                        }
                    }
                }
   
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating metadata: {ex.Message}");
            }
        }

        public async Task Update(LWOModel lwo)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Update the main LWOModel record
                        var updateLwoCommand = new SqlCommand(@"
                        UPDATE LWO SET 
                        WO_Number = @WONumber,
                        WO_Type = @WOType,
                        Activity = @Activity,
                        HM = @HM,
                        Time_Start = @TimeStart, 
                        Time_End = @TimeEnd,
                        PIC = @PIC,
                        LWO_Type = @LWOType,
                        Version = @Version,
                        Kode_Unit = @KodeUnit,
                        Updated_At = @UpdatedAt,
                        Updated_By = @UpdatedBy
                        WHERE ID = @ID", connection, transaction);

                        updateLwoCommand.Parameters.AddWithValue("@ID", lwo.ID);
                        updateLwoCommand.Parameters.AddWithValue("@WONumber", lwo.WONumber);
                        updateLwoCommand.Parameters.AddWithValue("@WOType", lwo.WOType);
                        updateLwoCommand.Parameters.AddWithValue("@Activity", lwo.Activity);
                        updateLwoCommand.Parameters.AddWithValue("@HM", lwo.HourMeter);
                        updateLwoCommand.Parameters.AddWithValue("@TimeStart", lwo.TimeStart);
                        updateLwoCommand.Parameters.AddWithValue("@TimeEnd", lwo.TimeEnd);
                        updateLwoCommand.Parameters.AddWithValue("@PIC", lwo.PIC);
                        updateLwoCommand.Parameters.AddWithValue("@LWOType", lwo.LWOType);
                        updateLwoCommand.Parameters.AddWithValue("@Version", lwo.Version + 1);
                        updateLwoCommand.Parameters.AddWithValue("@KodeUnit", lwo.KodeUnit);
                        updateLwoCommand.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);
                        updateLwoCommand.Parameters.AddWithValue("@UpdatedBy", lwo.UpdatedBy);

                        await updateLwoCommand.ExecuteNonQueryAsync();

                        // Handle metadata
                        var existingMetadataCommand = new SqlCommand(@"
                        SELECT ID FROM LWO_Metadata WHERE LWO_ID = @LWOID", connection, transaction);
                        existingMetadataCommand.Parameters.AddWithValue("@LWOID", lwo.ID);

                        var existingMetadataIds = new List<Guid>();
                        using (var reader = await existingMetadataCommand.ExecuteReaderAsync())
                        {
                            while (reader.Read())
                            {
                                existingMetadataIds.Add(reader.GetGuid(0));
                            }
                        }

                        var newMetadataIds = lwo.Metadata?.Select(m => m.ID).ToHashSet() ?? new HashSet<Guid>();

                        // Delete images and metadata not in the request
                        foreach (var id in existingMetadataIds.Except(newMetadataIds))
                        {
                            // Delete images associated with metadata
                            var deleteImagesCommand = new SqlCommand(@"
                    DELETE FROM LWO_Image WHERE LWO_Metadata_ID = @LWOMetadataID", connection, transaction);
                            deleteImagesCommand.Parameters.AddWithValue("@LWOMetadataID", id);
                            await deleteImagesCommand.ExecuteNonQueryAsync();

                            // Delete metadata
                            var deleteMetadataCommand = new SqlCommand(@"
                    DELETE FROM LWO_Metadata WHERE ID = @ID", connection, transaction);
                            deleteMetadataCommand.Parameters.AddWithValue("@ID", id);
                            await deleteMetadataCommand.ExecuteNonQueryAsync();
                        }

                        // Insert or update metadata and images as before
                        foreach (var metadata in lwo.Metadata ?? Enumerable.Empty<LWOMetadataModel>())
                        {
                            var newMetadataID = Guid.Empty;
                            if (!existingMetadataIds.Contains(metadata.ID))
                            {

                                // Insert new metadata
                                newMetadataID = Guid.NewGuid();
                                var insertMetadataCommand = new SqlCommand(@"
                                INSERT INTO LWO_Metadata 
                                (ID, LWO_ID, Komponen, Keterangan, Version, Created_By, Created_At, Updated_By, Updated_At)
                                VALUES (@ID, @LWOID, @Komponen, @Keterangan, @Version, @CreatedBy, @CreatedAt, @UpdatedBy, @UpdatedAt)", connection, transaction);

                                insertMetadataCommand.Parameters.AddWithValue("@ID", newMetadataID);
                                insertMetadataCommand.Parameters.AddWithValue("@LWOID", lwo.ID);
                                insertMetadataCommand.Parameters.AddWithValue("@Komponen", metadata.Komponen);
                                insertMetadataCommand.Parameters.AddWithValue("@Keterangan", metadata.Keterangan);
                                insertMetadataCommand.Parameters.AddWithValue("@Version", metadata.Version);
                                insertMetadataCommand.Parameters.AddWithValue("@CreatedBy", metadata.CreatedBy);
                                insertMetadataCommand.Parameters.AddWithValue("@CreatedAt", metadata.CreatedAt);
                                insertMetadataCommand.Parameters.AddWithValue("@UpdatedBy", metadata.UpdatedBy);
                                insertMetadataCommand.Parameters.AddWithValue("@UpdatedAt", metadata.UpdatedAt);

                                await insertMetadataCommand.ExecuteNonQueryAsync();
                            }
                            else
                            {
                                // Update existing metadata
                                var updateMetadataCommand = new SqlCommand(@"
                                UPDATE LWO_Metadata SET 
                                Komponen = @Komponen,
                                Keterangan = @Keterangan,
                                Version = @Version,
                                Updated_By = @UpdatedBy,
                                Updated_At = @UpdatedAt
                                WHERE ID = @ID", connection, transaction);

                                updateMetadataCommand.Parameters.AddWithValue("@ID", metadata.ID);
                                updateMetadataCommand.Parameters.AddWithValue("@Komponen", metadata.Komponen);
                                updateMetadataCommand.Parameters.AddWithValue("@Keterangan", metadata.Keterangan);
                                updateMetadataCommand.Parameters.AddWithValue("@Version", metadata.Version);
                                updateMetadataCommand.Parameters.AddWithValue("@UpdatedBy", lwo.UpdatedBy);
                                updateMetadataCommand.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);

                                await updateMetadataCommand.ExecuteNonQueryAsync();
                            }

                            // Handle images for each metadata
                            var existingImageCommand = new SqlCommand(@"
                            SELECT ID FROM LWO_Image WHERE LWO_Metadata_ID = @LWOMetadataID", connection, transaction);
                            existingImageCommand.Parameters.AddWithValue("@LWOMetadataID", metadata.ID);

                            var existingImageIds = new List<Guid>();
                            using (var reader = await existingImageCommand.ExecuteReaderAsync())
                            {
                                while (reader.Read())
                                {
                                    existingImageIds.Add(reader.GetGuid(0));
                                }
                            }

                            var newImageIds = metadata.Images?.Select(i => i.ID).ToHashSet() ?? new HashSet<Guid>();

                            // Delete images not in the request
                            foreach (var id in existingImageIds.Except(newImageIds))
                            {
                                var deleteImageCommand = new SqlCommand(@"
                                DELETE FROM LWO_Image WHERE ID = @ID", connection, transaction);
                                deleteImageCommand.Parameters.AddWithValue("@ID", id);
                                await deleteImageCommand.ExecuteNonQueryAsync();
                            }

                            // Insert new images
                            foreach (var image in metadata.Images?.Where(i => !existingImageIds.Contains(i.ID)) ?? Enumerable.Empty<LWOImageModel>())
                            {
                                var insertImageCommand = new SqlCommand(@"
                                INSERT INTO LWO_Image 
                                (ID, LWO_Metadata_ID, Path, Image_Name, Created_By, Created_At, Updated_By, Updated_At)
                                VALUES (@ID, @LWOMetadataID, @Path, @ImageName, @CreatedBy, @CreatedAt, @UpdatedBy, @UpdatedAt)", connection, transaction);

                                insertImageCommand.Parameters.AddWithValue("@ID", Guid.NewGuid());
                                insertImageCommand.Parameters.AddWithValue("@LWOMetadataID", newMetadataID);
                                insertImageCommand.Parameters.AddWithValue("@Path", image.Path);
                                insertImageCommand.Parameters.AddWithValue("@ImageName", image.ImageName);
                                insertImageCommand.Parameters.AddWithValue("@CreatedBy", image.CreatedBy);
                                insertImageCommand.Parameters.AddWithValue("@CreatedAt", image.CreatedAt);
                                insertImageCommand.Parameters.AddWithValue("@UpdatedBy", lwo.UpdatedBy);
                                insertImageCommand.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);

                                await insertImageCommand.ExecuteNonQueryAsync();
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
        public async Task DeleteMetadataByID(Guid metadataID)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Delete images associated with metadata
                            var deleteImagesCommand = new SqlCommand(@"
                            DELETE FROM LWO_Image WHERE LWO_Metadata_ID = @LWOMetadataID", connection, transaction);
                            deleteImagesCommand.Parameters.AddWithValue("@LWOMetadataID", metadataID);
                            await deleteImagesCommand.ExecuteNonQueryAsync();

                            // Delete metadata
                            var deleteMetadataCommand = new SqlCommand(@"
                            DELETE FROM LWO_Metadata WHERE ID = @ID", connection, transaction);
                            deleteMetadataCommand.Parameters.AddWithValue("@ID", metadataID);
                            await deleteMetadataCommand.ExecuteNonQueryAsync();

                            transaction.Commit();
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting metadata: {ex.Message}");
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
