using worklog_api.Model;
using worklog_api.Model.dto;

namespace worklog_api.Repository
{
    public interface IScheduleRepository
    {
        Task Create(Schedule schedule);
        Task<Schedule> GetScheduleDetailsByMonth(DateTime scheduleMonth, Guid? egiId = null, Guid? cnId = null);
        Task UpdateScheduleDetails(Guid scheduleId, List<ScheduleDetail> updatedDetails);
        
        Task<IEnumerable<Schedule>> GetScheduleByMonth(DateTime scheduleMonth);
        
        Task<IEnumerable<ScheduleDetail>> GetScheduleDetailsById(Guid scheduleId);
        
    }
}
