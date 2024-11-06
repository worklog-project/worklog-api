using worklog_api.error;
using worklog_api.Model;
using worklog_api.Repository;
using worklog_api.Repository.implementation;

namespace worklog_api.Service.implementation
{
    public class ScheduleService : IScheduleService
    {
        private readonly IScheduleRepository _scheduleRepository;

        public ScheduleService(IScheduleRepository scheduleRepository)
        {
            _scheduleRepository = scheduleRepository;
        }

        public async Task Create(Schedule schedule)
        {
            try
            {
                await _scheduleRepository.Create(schedule);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw new InternalServerError(e.Message);
            }
        }
    }
}
