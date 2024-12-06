using System;
using System.Threading.Tasks;
using worklog_api.Model;
using worklog_api.Model.dto;
using worklog_api.Repository;
using worklog_api.helper;

namespace worklog_api.Service
{
    public class LWOService : ILWOService
    {
        private readonly ILWORepository _lwoRepository;
        private readonly IFileUploadHelper _fileUploadHelper;

        public LWOService(ILWORepository lwoRepository,IFileUploadHelper fileUploadHelper)
        {
            _lwoRepository = lwoRepository;
            _fileUploadHelper = fileUploadHelper;
        }

        public async Task<IEnumerable<LWOModel>> GetAllLWOs()
        {
            return await _lwoRepository.GetAll();
        }

        public async Task<LWOModel> GetLWOById(Guid id)
        {
            return await _lwoRepository.GetById(id);
        }

        public async Task<LWOModel> CreateLWO(LWOCreateDto lwoDTO, IFormFileCollection images)
        {

            // Create a dictionary to store image names
            var imageMap = new Dictionary<string, IFormFile>();
            var lwo = new LWOModel {
                WONumber = lwoDTO.WONumber,
                WODate = lwoDTO.WODate,
                WOType = lwoDTO.WOType,
                TimeEnd = lwoDTO.TimeEnd,
                TimeStart = lwoDTO.TimeStart,
                Activity = lwoDTO.Activity,
                PIC = lwoDTO.PIC,
                HourMeter = lwoDTO.HourMeter,
                KodeUnit = lwoDTO.KodeUnit,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                CreatedBy = lwoDTO.CreatedBy,
                UpdatedBy = lwoDTO.UpdatedBy,
                LWOType = lwoDTO.LWOType,
                Version = lwoDTO.Version,
                Metadata = new List<LWOMetadataModel>()
            };


            // Save images on map
            foreach (var image in images)
            {
                if (image.Length > 0)
                {
                    // Add to image map
                    imageMap[image.FileName] = image;
                }
            }

            Console.WriteLine("Image map: " + imageMap);

            foreach (var metada in lwoDTO.Metadatas)
            {
                var metadataModel = new LWOMetadataModel
                {
                    Komponen = metada.Komponen,
                    Keterangan = metada.Keterangan,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Images = new List<LWOImageModel>()
                };

                foreach (var imageName in metada.ImagesName)
                {
                    Console.WriteLine($"Processing Image Name: {imageName}");

                    // Skip empty image names
                    if (string.IsNullOrWhiteSpace(imageName))
                    {
                        Console.WriteLine("Skipping empty image name");
                        continue;
                    }

                    // Detailed logging and validation
                    if (!imageMap.TryGetValue(imageName, out var imageFile))
                    {
                        Console.WriteLine($"Image not found in map: {imageName}");
                        throw new ArgumentException($"Image file not found: {imageName}");
                    }

                    // Defensive null checks before file validation
                    ArgumentNullException.ThrowIfNull(imageFile, nameof(imageFile));
                    ArgumentNullException.ThrowIfNull(_fileUploadHelper, nameof(_fileUploadHelper));

                    // Logging additional details about the file
                    Console.WriteLine($"Image File Details:");
                    Console.WriteLine($"  Name: {imageFile.FileName}");
                    Console.WriteLine($"  Length: {imageFile.Length}");
                    Console.WriteLine($"  Content Type: {imageFile.ContentType}");

                    // Validate file with null-safe check
                    bool isValidFile;
                    try
                    {
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                        var maxFileSize = 2 * 1024 * 1024; // 2MB 

                        // Defensive null checking in method call
                        isValidFile = _fileUploadHelper.IsValidFile(
                            imageFile,
                            allowedExtensions ?? Array.Empty<string>(),
                            maxFileSize > 0 ? maxFileSize : int.MaxValue
                        );
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error in file validation: {ex}");
                        throw new InvalidOperationException($"File validation failed for {imageName}", ex);
                    }

                    // Check validation result
                    if (!isValidFile)
                    {
                        throw new ArgumentException($"Invalid file: {imageName}. Please check file type and size.");
                    }

                    // Upload file
                    var (fileName, filePath) = await _fileUploadHelper.UploadFileAsync(imageFile, "lwo");
                    var lwoImage = new LWOImageModel
                    {
                        Path = filePath,
                        ImageName = fileName
                    };
                    metadataModel.Images.Add(lwoImage);
                }
                lwo.Metadata.Add(metadataModel);
            }

            try
            {
                var id = await _lwoRepository.Create(lwo);
                var result = await _lwoRepository.GetById(id);

                return result;
            }
            catch (Exception e)
            {
                throw new Exception("Error while creating LWO: " + e.Message);
            }

             
        }

        public async Task UpdateLWO(LWOModel lwo)
        {
            try
            {
                await _lwoRepository.Update(lwo);
            }
            catch (Exception e)
            {
                throw new Exception("Error while updating LWO: " + e.Message);
            }

        }

        public async Task DeleteLWO(Guid id)
        {
            try
            {
                await _lwoRepository.Delete(id);
            }
            catch (Exception e)
            {
                throw new Exception("Error while deleting LWO: " + e.Message);
            }
        }
    }
}
