using worklog_api.Model;

namespace worklog_api.Repository
{
    public interface IScheduleRepository
    {
        Task Create(Schedule schedule);
    }
}
