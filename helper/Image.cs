using System.IO;
using worklog_api.payload.error;
namespace worklog_api.helper
{
    public interface IFileUploadHelper
    {
        Task<(string fileName, string filePath)> UploadFileAsync(IFormFile file, string subDirectory = "");
        void DeleteFile(string filePath);
        bool IsValidFile(IFormFile file, string[] allowedExtensions, long maxFileSize);
    }

    public class FileUploadHelper : IFileUploadHelper
    {
        private readonly string _baseUploadPath;
        private readonly ILogger<FileUploadHelper> _logger;

        public FileUploadHelper(IConfiguration configuration, ILogger<FileUploadHelper> logger)
        {
            _baseUploadPath = configuration["FileUpload:BasePath"] ?? "wwwroot/uploads";
            _logger = logger;
        }

        public async Task<(string fileName, string filePath)> UploadFileAsync(IFormFile file, string subDirectory = "")
        {
            try
            {
                if (file == null || file.Length == 0)
                    throw new ArgumentException("File is empty or null", nameof(file));

                // Create unique filename
                var fileExtension = Path.GetExtension(file.FileName);
                var uniqueFileName = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}_{Guid.NewGuid()}{fileExtension}";

                // Construct the upload directory path
                var uploadDirectory = Path.Combine(_baseUploadPath, subDirectory);

                // Ensure directory exists
                if (!Directory.Exists(uploadDirectory))
                    Directory.CreateDirectory(uploadDirectory);

                // Construct the full file path
                var filePath = Path.Combine(uploadDirectory, uniqueFileName);

                // Save the file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                // Return relative path for database storage
                var relativePath = Path.Combine(subDirectory, uniqueFileName).Replace("\\", "/");

                return (uniqueFileName, relativePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file: {FileName}", file.FileName);
                throw new FileUploadException("Failed to upload file", ex);
            }
        }

        public void DeleteFile(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_baseUploadPath, filePath);
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
                throw new FileUploadException("Failed to delete file", ex);
            }
        }

        public bool IsValidFile(IFormFile file, string[] allowedExtensions, long maxFileSize)
        {
            if (file == null || file.Length == 0)
                return false;

            if (file.Length > maxFileSize)
                return false;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return allowedExtensions.Contains(extension);
        }
    }
}
