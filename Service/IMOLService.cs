using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using worklog_api.Model;

namespace worklog_api.Service
{
    public interface IMOLService
    {
        Task<IEnumerable<MOLModel>> GetAllMOLs(int pageNumber, int pageSize, string sortBy, string sortDirection, DateTime? startDate, DateTime? endDate);
        Task<MOLModel> GetMOLById(Guid id);
        Task CreateMOL(MOLModel mol);
        Task UpdateMOL(MOLModel mol);
        Task DeleteMOL(Guid id);
        Task ApproveMOL(StatusHistoryModel status, UserModel user);
    }
}
