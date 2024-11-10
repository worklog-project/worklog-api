using System;
using worklog_api.error;
using worklog_api.Model;
using worklog_api.Model.dto;
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
            // check if schedule with cn id, egi id, and schedule month already exist
            var egiId = schedule.EGIID;
            var cnId = schedule.CNID;

            var existingSchedule = await _scheduleRepository.GetScheduleDetailsByMonth(schedule.ScheduleMonth, egiId, cnId);

            // If an existing schedule is found, throw an error
            if (existingSchedule != null)
            {
                throw new InternalServerError("A schedule with the same EGI, CN, and month already exists.");
            }


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

        public async Task<Schedule> GetScheduleDetailsByMonth(DateTime scheduleMonth, Guid? egiId = null, Guid? cnId = null)
        {
            try
            {
                return await _scheduleRepository.GetScheduleDetailsByMonth(scheduleMonth, egiId, cnId);
               
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw new InternalServerError("Error retrieving schedule details");
            }
        }

        public async Task UpdateScheduleDetails(Guid scheduleId, List<ScheduleDetail> updatedDetails)
        {
            try
            {
                await _scheduleRepository.UpdateScheduleDetails(scheduleId, updatedDetails);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw new InternalServerError("Error updating schedule details");
            }
        }
    }
}
