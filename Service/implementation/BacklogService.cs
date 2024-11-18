using worklog_api.Repository;
using worklog_api.Model;
using worklog_api.Model.dto;
using worklog_api.helper;
using System.Net.Quic;
using Microsoft.AspNetCore.Mvc;
using worklog_api.error;

namespace worklog_api.Service.implementation
{
    public class BacklogService : IBacklogService
    {
        private readonly IBacklogRepository _backlogRepository;
        private readonly IFileUploadHelper _fileUploadHelper;
        private readonly ILogger<BacklogService> _logger;

        public BacklogService(
        IBacklogRepository backlogRepository,
        IFileUploadHelper fileUploadHelper,
        ILogger<BacklogService> logger)
        {
            _backlogRepository = backlogRepository;
            _fileUploadHelper = fileUploadHelper;
            _logger = logger;
        }

        public async Task<Guid> InsertBacklogAsync(BacklogDTO backlogDTO, BacklogImageDTO imageDTO)
        {
            string uploadedFilePath = null;

            try
            {
                // Validate file
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var maxFileSize = 2 * 1024 * 1024; // 2MB

                if (!_fileUploadHelper.IsValidFile(imageDTO.ImageFile, allowedExtensions, maxFileSize))
                {
                    throw new ArgumentException("Invalid file. Please check file type and size.");
                }

                // Upload file
                var (fileName, filePath) = await _fileUploadHelper.UploadFileAsync(
                    imageDTO.ImageFile,
                    "backlogs"  // subdirectory for backlog images
                );

                uploadedFilePath = filePath;

                // Map DTOs to Models
                var backlogModel = new BacklogModel
                {
                    DateInspection = backlogDTO.DateOfInspection,
                    ProblemDescription = backlogDTO.ProblemDescription,
                    Description = backlogDTO.Description,
                    CNID = backlogDTO.CodeNumber,
                    Component = backlogDTO.Component,
                    PartNumberRequired = backlogDTO.PartNumberRequired,
                    NoIndex = backlogDTO.NoIndex,
                    NoFigure = backlogDTO.NoFigure,
                    Quantity = backlogDTO.Qty,
                    PlanRepair = backlogDTO.PlanRepair,
                    EstimateRepairHour = backlogDTO.EstimateRepairHour,
                    Status = backlogDTO.Status,
                    DailyDetailId = backlogDTO.DailyDetailId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = backlogDTO.CreatedBy
                };

                var backlogImageModel = new BacklogImageModel
                {
                    FormatType = imageDTO.ImageFile.ContentType,
                    FileName = fileName,
                    FilePath = filePath,
                    CreatedBy = backlogDTO.CreatedBy,
                    CreatedAt = DateTime.UtcNow
                };

                return await _backlogRepository.InsertBacklogAsync(backlogModel, backlogImageModel);
            }
            catch (Exception ex)
            {
                // If we have uploaded a file and an error occurs during repository insert,
                // clean up the uploaded file
                if (!string.IsNullOrEmpty(uploadedFilePath))
                {
                    try
                    {
                        _fileUploadHelper.DeleteFile(uploadedFilePath);
                        _logger.LogInformation("Successfully cleaned up file after failed insert: {FilePath}", uploadedFilePath);
                    }
                    catch (Exception deleteEx)
                    {
                        // Log but don't throw - we want to throw the original exception
                        _logger.LogWarning(deleteEx, "Failed to clean up file after failed insert: {FilePath}", uploadedFilePath);
                    }
                }

                _logger.LogError(ex, "Error inserting backlog");
                throw new InternalServerError("Error inserting backlog"); // Re-throw the original exception
            }
        }

        public async Task<BacklogModel> GetByIDAsync(Guid backlogID)
        {
            return await _backlogRepository.GetByIDAsync(backlogID);
        }

        public async Task<bool> DeleteBacklogAsync(Guid backlogID)
        {
            try
            {
                var backlog = await _backlogRepository.GetByIDAsync(backlogID);
                if (backlog == null)
                {
                    throw new NotFoundException("Backlog not found");
                }
                await _backlogRepository.DeleteBacklogAsync(backlogID);
                for (int i = 0; i < backlog.BacklogImages.Count; i++)
                {
                    _fileUploadHelper.DeleteFile(backlog.BacklogImages[i].FilePath);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting backlog");
                throw new InternalServerError("Backlog not found");
                throw;
            }
             
        }
    }


}
