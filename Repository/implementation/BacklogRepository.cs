using System.Data.SqlClient;
using worklog_api.Model;

namespace worklog_api.Repository.implementation
{
    public class BacklogRepository : IBacklogRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<BacklogRepository> _logger;

        public BacklogRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<Guid> InsertBacklogAsync(BacklogModel backlogModel, BacklogImageModel backlogImageModel)
        {
            var backlogQuery = @"INSERT INTO daily_work_log_backlog
        (ID, date_inspection, cn_id, problem_description, component, part_number_required, description, 
         no_index, no_figure, qty, plan_repair, estimate_repair_hour, status, daily_detail_id, created_at, created_by)
        OUTPUT INSERTED.id
        VALUES (@Id, @date_inspection, @cn_id, @problem_description, @component, @part_number_required, 
                @description, @no_index, @no_figure, @quantity, @plan_repair, @estimate_repair_hour, 
                @status, @daily_detail_id, @created_at, @created_by)";

            var backlogImageQuery = @"INSERT INTO daily_work_log_backlog_image
        (ID, backlog_id, image_url, created_at, created_by)
        VALUES (@Id, @backlog_id, @image_url, @created_at, @created_by)";

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        backlogModel.ID = Guid.NewGuid();
                        backlogImageModel.ID = Guid.NewGuid();

                        // Insert Backlog
                        var backlogCommand = new SqlCommand(backlogQuery, connection, transaction);
                        backlogCommand.Parameters.AddWithValue("@Id", backlogModel.ID);
                        backlogCommand.Parameters.AddWithValue("@date_inspection", backlogModel.DateInspection);
                        backlogCommand.Parameters.AddWithValue("@cn_id", backlogModel.CNID);
                        backlogCommand.Parameters.AddWithValue("@problem_description", backlogModel.ProblemDescription ?? (object)DBNull.Value);
                        backlogCommand.Parameters.AddWithValue("@component", backlogModel.Component ?? (object)DBNull.Value);
                        backlogCommand.Parameters.AddWithValue("@part_number_required", backlogModel.PartNumberRequired ?? (object)DBNull.Value);
                        backlogCommand.Parameters.AddWithValue("@description", backlogModel.Description ?? (object)DBNull.Value);
                        backlogCommand.Parameters.AddWithValue("@no_index", backlogModel.NoIndex ?? (object)DBNull.Value);
                        backlogCommand.Parameters.AddWithValue("@no_figure", backlogModel.NoFigure ?? (object)DBNull.Value);
                        backlogCommand.Parameters.AddWithValue("@quantity", backlogModel.Quantity ?? (object)DBNull.Value);
                        backlogCommand.Parameters.AddWithValue("@plan_repair", backlogModel.PlanRepair ?? (object)DBNull.Value);
                        backlogCommand.Parameters.AddWithValue("@estimate_repair_hour", backlogModel.EstimateRepairHour ?? (object)DBNull.Value);
                        backlogCommand.Parameters.AddWithValue("@status", backlogModel.Status ?? (object)DBNull.Value);
                        backlogCommand.Parameters.AddWithValue("@daily_detail_id", backlogModel.DailyDetailId);
                        backlogCommand.Parameters.AddWithValue("@created_at", DateTime.Now);
                        backlogCommand.Parameters.AddWithValue("@created_by", backlogModel.CreatedBy ?? (object)DBNull.Value);

                        Guid backlogId = (Guid)await backlogCommand.ExecuteScalarAsync();

                        // Insert Backlog Image
                        var backlogImageCommand = new SqlCommand(backlogImageQuery, connection, transaction);
                        backlogImageCommand.Parameters.AddWithValue("@Id", backlogImageModel.ID);
                        backlogImageCommand.Parameters.AddWithValue("@backlog_id", backlogId);
                        backlogImageCommand.Parameters.AddWithValue("@image_url", backlogImageModel.FilePath);
                        backlogImageCommand.Parameters.AddWithValue("@created_at", DateTime.Now);
                        backlogImageCommand.Parameters.AddWithValue("@created_by", backlogImageModel.CreatedBy ?? (object)DBNull.Value);

                        await backlogImageCommand.ExecuteNonQueryAsync();

                        // Commit transaction
                        await transaction.CommitAsync();
                        return backlogId;
                    }
                    catch (Exception e)
                    {
                        _logger.LogWarning(e.Message);
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }

        public async Task<BacklogModel> GetByIDAsync(Guid backlogID)
        {
            var query = @"
        SELECT 
            b.ID, b.date_inspection, b.cn_id, b.problem_description, b.component, 
            b.part_number_required, b.description, b.no_index, b.no_figure, b.qty, 
            b.plan_repair, b.estimate_repair_hour, b.status, b.daily_detail_id, 
            b.created_at, b.created_by,
            i.ID AS ImageID, i.image_url, i.created_at AS ImageCreatedAt, i.created_by AS ImageCreatedBy
        FROM 
            daily_work_log_backlog b
        LEFT JOIN 
            daily_work_log_backlog_image i ON b.ID = i.backlog_id
        WHERE 
            b.ID = @backlogID";

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@backlogID", backlogID);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    BacklogModel backlog = null;
                    var images = new List<BacklogImageModel>();

                    while (await reader.ReadAsync())
                    {
                        // Initialize the backlog model only once
                        if (backlog == null)
                        {
                            backlog = new BacklogModel
                            {
                                ID = reader.GetGuid(reader.GetOrdinal("ID")),
                                DateInspection = reader.GetDateTime(reader.GetOrdinal("date_inspection")),
                                CNID = reader.GetGuid(reader.GetOrdinal("cn_id")),
                                ProblemDescription = reader.IsDBNull(reader.GetOrdinal("problem_description"))
                                    ? null : reader.GetString(reader.GetOrdinal("problem_description")),
                                Component = reader.IsDBNull(reader.GetOrdinal("component"))
                                    ? null : reader.GetString(reader.GetOrdinal("component")),
                                PartNumberRequired = reader.IsDBNull(reader.GetOrdinal("part_number_required"))
                                    ? null : reader.GetString(reader.GetOrdinal("part_number_required")),
                                Description = reader.IsDBNull(reader.GetOrdinal("description"))
                                    ? null : reader.GetString(reader.GetOrdinal("description")),
                                NoIndex = reader.IsDBNull(reader.GetOrdinal("no_index"))
                                    ? null : reader.GetString(reader.GetOrdinal("no_index")),
                                NoFigure = reader.IsDBNull(reader.GetOrdinal("no_figure"))
                                    ? null : reader.GetString(reader.GetOrdinal("no_figure")),
                                // Handle qty as a string that needs parsing into int
                                Quantity = reader.IsDBNull(reader.GetOrdinal("qty"))
                                    ? null : reader.GetInt32(reader.GetOrdinal("qty")),
                                PlanRepair = reader.IsDBNull(reader.GetOrdinal("plan_repair"))
                                    ? null : reader.GetString(reader.GetOrdinal("plan_repair")),
                                EstimateRepairHour = reader.IsDBNull(reader.GetOrdinal("estimate_repair_hour"))
                                    ? null : reader.GetDouble(reader.GetOrdinal("estimate_repair_hour")),
                                Status = reader.IsDBNull(reader.GetOrdinal("status"))
                                    ? null : reader.GetString(reader.GetOrdinal("status")),
                                DailyDetailId = reader.GetGuid(reader.GetOrdinal("daily_detail_id")),
                                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                                CreatedBy = reader.IsDBNull(reader.GetOrdinal("created_by"))
                                    ? null : reader.GetString(reader.GetOrdinal("created_by")),
                                BacklogImages = new List<BacklogImageModel>() // Initialize images list
                            };
                        }

                        // Add associated images if available
                        if (!reader.IsDBNull(reader.GetOrdinal("ImageID"))) // Check if ImageID is not null
                        {
                            images.Add(new BacklogImageModel
                            {
                                ID = reader.GetGuid(reader.GetOrdinal("ImageID")),
                                FilePath = reader.GetString(reader.GetOrdinal("image_url")),
                                CreatedAt = reader.GetDateTime(reader.GetOrdinal("ImageCreatedAt")),
                                CreatedBy = reader.IsDBNull(reader.GetOrdinal("ImageCreatedBy"))
                                    ? null : reader.GetString(reader.GetOrdinal("ImageCreatedBy"))
                            });
                        }
                    }

                    // Assign images to the backlog model
                    if (backlog != null)
                    {
                        backlog.BacklogImages = images;
                    }

                    return backlog;
                }
            }
        }


        public async Task<List<BacklogModel>> GetByDailyIDAsync(Guid dailyDetailId)
        {
            var query = @"
            SELECT 
                b.ID, b.date_inspection, b.cn_id, b.problem_description, b.component, 
                b.part_number_required, b.description, b.no_index, b.no_figure, b.qty, 
                b.plan_repair, b.estimate_repair_hour, b.status, b.daily_detail_id, 
                b.created_at, b.created_by,
                i.ID AS ImageID, i.image_url, i.created_at AS ImageCreatedAt, i.created_by AS ImageCreatedBy
            FROM 
                daily_work_log_backlog b
            LEFT JOIN 
                daily_work_log_backlog_image i ON b.ID = i.backlog_id
            WHERE
                b.daily_detail_id = @dailyDetailId";

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@dailyDetailId", dailyDetailId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    var backlogs = new Dictionary<Guid, BacklogModel>();

                    while (await reader.ReadAsync())
                    {
                        var backlogId = reader.GetGuid(reader.GetOrdinal("ID"));

                        // Check if the backlog already exists in the dictionary
                        if (!backlogs.ContainsKey(backlogId))
                        {
                            var backlog = new BacklogModel
                            {
                                ID = backlogId,
                                DateInspection = reader.GetDateTime(reader.GetOrdinal("date_inspection")),
                                CNID = reader.GetGuid(reader.GetOrdinal("cn_id")),
                                ProblemDescription = reader.IsDBNull(reader.GetOrdinal("problem_description"))
                                    ? null : reader.GetString(reader.GetOrdinal("problem_description")),
                                Component = reader.IsDBNull(reader.GetOrdinal("component"))
                                    ? null : reader.GetString(reader.GetOrdinal("component")),
                                PartNumberRequired = reader.IsDBNull(reader.GetOrdinal("part_number_required"))
                                    ? null : reader.GetString(reader.GetOrdinal("part_number_required")),
                                Description = reader.IsDBNull(reader.GetOrdinal("description"))
                                    ? null : reader.GetString(reader.GetOrdinal("description")),
                                NoIndex = reader.IsDBNull(reader.GetOrdinal("no_index"))
                                    ? null : reader.GetString(reader.GetOrdinal("no_index")),
                                NoFigure = reader.IsDBNull(reader.GetOrdinal("no_figure"))
                                    ? null : reader.GetString(reader.GetOrdinal("no_figure")),
                                Quantity = reader.IsDBNull(reader.GetOrdinal("qty"))
                                    ? null : reader.GetInt32(reader.GetOrdinal("qty")),
                                PlanRepair = reader.IsDBNull(reader.GetOrdinal("plan_repair"))
                                    ? null : reader.GetString(reader.GetOrdinal("plan_repair")),
                                EstimateRepairHour = reader.IsDBNull(reader.GetOrdinal("estimate_repair_hour"))
                                    ? null : reader.GetDouble(reader.GetOrdinal("estimate_repair_hour")),
                                Status = reader.IsDBNull(reader.GetOrdinal("status"))
                                    ? null : reader.GetString(reader.GetOrdinal("status")),
                                DailyDetailId = reader.GetGuid(reader.GetOrdinal("daily_detail_id")),
                                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                                CreatedBy = reader.IsDBNull(reader.GetOrdinal("created_by"))
                                    ? null : reader.GetString(reader.GetOrdinal("created_by")),
                                BacklogImages = new List<BacklogImageModel>() // Initialize images list
                            };

                            backlogs[backlogId] = backlog;
                        }

                        // Add associated images if available
                        if (!reader.IsDBNull(reader.GetOrdinal("ImageID")))
                        {
                            var image = new BacklogImageModel
                            {
                                ID = reader.GetGuid(reader.GetOrdinal("ImageID")),
                                FilePath = reader.GetString(reader.GetOrdinal("image_url")),
                                CreatedAt = reader.GetDateTime(reader.GetOrdinal("ImageCreatedAt")),
                                CreatedBy = reader.IsDBNull(reader.GetOrdinal("ImageCreatedBy"))
                                    ? null : reader.GetString(reader.GetOrdinal("ImageCreatedBy"))
                            };

                            backlogs[backlogId].BacklogImages.Add(image);
                        }
                    }

                    return backlogs.Values.ToList();
                }
            }
        }

        public async Task<bool> DeleteBacklogAsync(Guid backlogId)
        {
            //delete on daily_work_log_image first
            var deleteImageQuery = @"DELETE FROM daily_work_log_backlog_image WHERE backlog_id = @backlogId";
            //delete on daily_work_log_backlog
            var deleteQuery = @"DELETE FROM daily_work_log_backlog WHERE ID = @backlogId";

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var deleteImageCommand = new SqlCommand(deleteImageQuery, connection, transaction);
                        deleteImageCommand.Parameters.AddWithValue("@backlogId", backlogId);
                        await deleteImageCommand.ExecuteNonQueryAsync();

                        var deleteCommand = new SqlCommand(deleteQuery, connection, transaction);
                        deleteCommand.Parameters.AddWithValue("@backlogId", backlogId);
                        var rowsAffected = await deleteCommand.ExecuteNonQueryAsync();

                        await transaction.CommitAsync();
                        return rowsAffected > 0;
                    }
                    catch (Exception e)
                    {
                        _logger.LogWarning(e.Message);
                        await transaction.RollbackAsync();
                        throw;
                    }
                }

            }

        }
    }
}
