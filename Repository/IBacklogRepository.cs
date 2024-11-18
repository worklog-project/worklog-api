using worklog_api.Model;

namespace worklog_api.Repository
{
    public interface IBacklogRepository
    {
        Task<Guid> InsertBacklogAsync(BacklogModel backlogModel, BacklogImageModel backlogImageModel);
        Task<BacklogModel> GetByIDAsync(Guid backlogID);
        Task<bool> DeleteBacklogAsync(Guid backlogId);
    }
}
