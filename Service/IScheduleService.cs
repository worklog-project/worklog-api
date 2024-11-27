using worklog_api.Model;

namespace worklog_api.Service
{
    public interface IScheduleService
    {
        Task Create(Schedule schedule);
        Task<Schedule> GetScheduleDetailsByMonth(DateTime scheduleMonth, Guid? egiId = null, Guid? cnId = null);
        Task UpdateScheduleDetails(Guid scheduleId, List<ScheduleDetail> updatedDetails);
        Task<Byte[]> GetScheduleByMonth(string scheduleMonth);
    }
}
