using worklog_api.Model;
using worklog_api.Model.dto;

namespace worklog_api.Repository
{
    public interface IScheduleRepository
    {
        Task Create(Schedule schedule);
        Task<List<ScheduleDTO>> GetScheduleDetailsByMonth(DateTime scheduleMonth);
    }
}
