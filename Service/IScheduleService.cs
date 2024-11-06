using worklog_api.Model;

namespace worklog_api.Service
{
    public interface IScheduleService
    {
        Task Create(Schedule schedule);
    }
}
