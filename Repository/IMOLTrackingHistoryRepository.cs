using worklog_api.Model;

namespace worklog_api.Repository
{
    public interface IMOLTrackingHistoryRepository
    {
        Task Create(MOLTrackingHistoryModel molTracking);
    }
}
