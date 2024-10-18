using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using worklog_api.Model;

namespace worklog_api.Service
{
    public interface IMOLService
    {
        Task<IEnumerable<MOLModel>> GetAllMOLs();
        Task<MOLModel> GetMOLById(Guid id);
        Task CreateMOL(MOLModel mol);
        Task UpdateMOL(MOLModel mol);
        Task DeleteMOL(Guid id);
    }
}
