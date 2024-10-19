using System;
using System.Threading.Tasks;
using worklog_api.Model;

namespace worklog_api.Service
{
    public interface ILWOService
    {
        Task<IEnumerable<LWOModel>> GetAllLWOs(); // Method to get all LWOs
        Task<LWOModel> GetLWOById(Guid id); // Method to get LWO by ID
        Task CreateLWO(LWOModel lwo);    // Method to create a new LWO
        Task UpdateLWO(LWOModel lwo);    // Method to update an existing LWO
        Task DeleteLWO(Guid id);         // Method to delete an LWO by ID
    }
}
