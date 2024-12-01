using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using worklog_api.Model;

namespace worklog_api.Repository
{
    public interface IMOLRepository
    {
        Task<(IEnumerable<MOLModel> mols, int totalCount)> GetAll(int pageNumber, int pageSize, string sortBy, string sortDirection, DateTime? startDate, DateTime? endDate, string requestBy, string status);
        Task<MOLModel> GetById(Guid id);
        Task Create(MOLModel mol);
        Task Update(MOLModel mol);
        Task Delete(Guid id);
        Task UpdateApprovedQuantity(Guid id, int quantityApproved);
        Task <IEnumerable<MOLModel>> GetMolByStatus(string status);
    }
}
