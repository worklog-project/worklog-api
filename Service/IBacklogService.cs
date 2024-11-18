using System;
using System.Threading.Tasks;
using worklog_api.Model;
using worklog_api.Model.dto;

namespace worklog_api.Service
{
    public interface IBacklogService
    {
        Task<Guid> InsertBacklogAsync(BacklogDTO backlogDTO, BacklogImageDTO imageDTO);
        Task<BacklogModel> GetByIDAsync(Guid backlogID);
        Task<bool> DeleteBacklogAsync(Guid backlogID);
    }
}
