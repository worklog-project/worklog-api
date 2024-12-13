using System;
using System.Threading.Tasks;
using worklog_api.Model;
using worklog_api.Model.dto;
using worklog_api.Repository;
using worklog_api.helper;
using worklog_api.error;

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

        public async Task<(IEnumerable<LWOModel>,int totalCount)> GetAllLWOs(int pageNumber, int pageSize, string sortBy, string sortDirection, DateTime? startDate, DateTime? endDate, string requestBy)
        {
            return await _lwoRepository.GetAll(pageNumber, pageSize, sortBy, sortDirection, startDate, endDate, requestBy);
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
                GroupLeader = lwoDTO.GroupLeader,
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

                    // Validate file with null-safe check
                    bool isValidFile;
                    try
                    {
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                        var maxFileSize = 10 * 1024 * 1024; // 10MB 

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

        public async Task<LWOModel> CreateMetadataByLWOID(Guid lwoID, LWOMetadataCreateDto metadata, IFormFileCollection images, UserModel user)
        {
            try
            {
                var imageMap = new Dictionary<string, IFormFile>();
                // Save images on map
                foreach (var image in images)
                {
                    if (image.Length > 0)
                    {
                        // Add to image map
                        imageMap[image.FileName] = image;
                    }
                }

                var metadataModel = new LWOMetadataModel
                {
                    Komponen = metadata.Komponen,
                    Keterangan = metadata.Keterangan,
                    LWOID = lwoID,
                    CreatedBy = user.username,
                    UpdatedBy = user.username,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Images = new List<LWOImageModel>()
                };

                foreach (var imageName in metadata.ImagesName)
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

                    // Validate file with null-safe check
                    bool isValidFile;
                    try
                    {
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                        var maxFileSize = 10 * 1024 * 1024; // 2MB 

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

                await _lwoRepository.CreateMetadataByLWOID(metadataModel);
                var lwo = await _lwoRepository.GetById(lwoID);


                return lwo;
            }
            catch (Exception e)
            {
                throw new Exception("Error while creating Metadata: " + e.Message);
            }
        }

        public async Task UpdateLWO(Guid id, LWOModel lwo)
        {
            try
            {
                var imageMap = new Dictionary<string, IFormFile>();
                var existingLwo = await _lwoRepository.GetById(id);
                if (existingLwo == null)
                {
                    throw new NotFoundException("LWO Not Found");
                }

                //map new data to existing data
                existingLwo.WONumber = lwo.WONumber;
                existingLwo.WODate = lwo.WODate;
                existingLwo.WOType = lwo.WOType;
                existingLwo.TimeEnd = lwo.TimeEnd;
                existingLwo.TimeStart = lwo.TimeStart;
                existingLwo.Activity = lwo.Activity;
                existingLwo.PIC = lwo.PIC;
                existingLwo.HourMeter = lwo.HourMeter;
                existingLwo.KodeUnit = lwo.KodeUnit;
                existingLwo.UpdatedAt = DateTime.Now;
                existingLwo.UpdatedBy = lwo.UpdatedBy;
                existingLwo.LWOType = lwo.LWOType;
                existingLwo.Version = lwo.Version;

                await _lwoRepository.Update(lwo);
            }
            catch (Exception e)
            {
                throw new Exception("Error while updating LWO: " + e.Message);
            }

        }

        public async Task DeleteMetadataByID(Guid metadataId)
        {
            try
            {
                await _lwoRepository.DeleteMetadataByID(metadataId);
            }
            catch (Exception e)
            {
                throw new Exception("Error while deleting Metadata: " + e.Message);
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
