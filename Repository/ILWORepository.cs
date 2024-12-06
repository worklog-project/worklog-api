using System;
using System.Threading.Tasks;
using worklog_api.Model;

namespace worklog_api.Repository
{
    public interface ILWORepository
    {
        Task<IEnumerable<LWOModel>> GetAll();
        Task<LWOModel> GetById(Guid id);  // Method to get LWO by ID
        Task<Guid> Create(LWOModel lwo);        // Method to create a new LWO
        Task Update(LWOModel lwo);        // Method to update an existing LWO
        Task Delete(Guid id);             // Method to delete an LWO by ID
    } 
}
