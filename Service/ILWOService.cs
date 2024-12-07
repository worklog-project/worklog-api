using System;
using System.Threading.Tasks;
using worklog_api.Model;
using worklog_api.Model.dto;

namespace worklog_api.Service
{
    public interface ILWOService
    {
        Task<(IEnumerable<LWOModel>, int totalCount)> GetAllLWOs(int pageNumber, int pageSize, string sortBy, string sortDirection, DateTime? startDate, DateTime? endDate, string requestBy); // Method to get all LWOs
        Task<LWOModel> GetLWOById(Guid id); // Method to get LWO by ID
        Task<LWOModel> CreateLWO(LWOCreateDto lwoDTO, IFormFileCollection images);    // Method to create a new LWO
        Task UpdateLWO(LWOModel lwo);    // Method to update an existing LWO
        Task DeleteLWO(Guid id);         // Method to delete an LWO by ID
    }
}
