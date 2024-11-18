using System.Collections.Generic;
using System.Threading.Tasks;
using worklog_api.error;
using worklog_api.helper;
using worklog_api.Model;
using worklog_api.Model.dto;
using worklog_api.Model.form;
using worklog_api.Repository;
using worklog_api.Repository.implementation;

namespace worklog_api.Service.implementation
{
    public class DailyService : IDailyService
    {
        private readonly IDailyRepository _dailyRepository;
        private readonly IBacklogRepository _backlogRepository;

        public DailyService(IDailyRepository dailyRepository, IBacklogRepository backlogRepository)
        {
            _dailyRepository = dailyRepository;
            _backlogRepository = backlogRepository;
        }

        public async Task<IEnumerable<EGIModel>> GetEGI(string egi)
        {
            return await _dailyRepository.GetEGI(egi);
        }

        public async Task<IEnumerable<CodeNumberModel>> GetCodeNumber(string codeNumber, Guid egiID)
        {
            return await _dailyRepository.GetCodeNumber(codeNumber, egiID);

        }

        public async Task<string> InsertDaily(DailyRequest dailyRequest)
        {
            var egiNameById = await _dailyRepository.GetEgiNameByID(dailyRequest._egiId);

            var scheduleDetailById = await _dailyRepository.GetScheduleDetailById(dailyRequest._date);
            
            var dailyByEgiAndCodeNumberAndDate =  
                await _dailyRepository.getDailyByEgiAndCodeNumberAndDate(dailyRequest._egiId, dailyRequest._cnId, scheduleDetailById.PlannedDate.ToString("yyyy-MM-dd"));



            if (scheduleDetailById == null)
            {
                throw new NotFoundException("Given Schedule Not Found");
            }else if (scheduleDetailById.IsDone == true)
            {
                throw new BadRequestException("Daily for Given Schedule Has Done");
            }
            
            int count = 0;

            if (egiNameById.Substring(0, 2) == "HD")
            {
                count = 4;
            }
            else
            {
                count = 3;
            }
            

            var generateId = Guid.NewGuid();
            DailyModel dailyModel = new DailyModel()
            {
                _id = generateId,
                _date = scheduleDetailById.PlannedDate,
                _cnId = Guid.Parse(dailyRequest._cnId),
                _count = 0,
                _egiId = Guid.Parse(dailyRequest._egiId),
                _sheetDetail = dailyRequest._sheetDetail,
                _hourmeter = dailyRequest._hourmeter,
                _startTime = dailyRequest._startTime,
                _endTime = dailyRequest._endTime,
                _formType = dailyRequest._formType,
                _dailyId = dailyByEgiAndCodeNumberAndDate != null ? dailyByEgiAndCodeNumberAndDate._id : Guid.Empty, // null
                _groupLeader = dailyRequest._groupLeader,
                _mechanic = dailyRequest._mechanic,
            };
            
            if (dailyByEgiAndCodeNumberAndDate == null)
            {
                var dailyDetailGeneratedId = Guid.NewGuid();
                var insertDaily = await _dailyRepository.insertDaily(dailyModel, scheduleDetailById.ID);
                dailyModel._dailyId = insertDaily;
                var dailyDetail =
                    await _dailyRepository.insertDailyDetail(dailyModel, dailyDetailGeneratedId, scheduleDetailById.ID, count);
                return dailyDetail.ToString();
            } else
            {
                if (dailyByEgiAndCodeNumberAndDate._count >= count)
                {
                    throw new BadRequestException("Daily already has been filled");
                }
                
                dailyRequest._dailyId = dailyByEgiAndCodeNumberAndDate._id;
                var dailyDetailGeneratedId = Guid.NewGuid();
                var dailyDetail = await _dailyRepository.insertDailyDetail(dailyModel, dailyDetailGeneratedId, scheduleDetailById.ID, count);
                return dailyDetail.ToString();
            }
        }

        public async Task<DailyWorklogDetailResponse> GetDailyDetailByID(string id)
        {
            var guid = Guid.Parse(id);
            var dailyDetailById = await _dailyRepository.getDailyDetailById(guid);
            

            if (dailyDetailById == null)
            {
                throw new NotFoundException("Daily Detail with given Id not found");
            }

            var backlogs = await _backlogRepository.GetByDailyDetailIDAsync(guid);

            return new DailyWorklogDetailResponse()
            {
                _id = dailyDetailById._id.ToString(),
                _sheetDetail = dailyDetailById._sheetDetail,
                _hourmeter = dailyDetailById._hourmeter,
                _startTime = dailyDetailById._startTime,
                _endTime = dailyDetailById._endTime,
                _formType = dailyDetailById._formType,
                _groupLeader = dailyDetailById._groupLeader,
                _mechanic = dailyDetailById._mechanic,
                _backlogs = backlogs,
                _date = dailyDetailById._date.Date.ToString("yyyy-MM-dd"),
                _cnid = dailyDetailById._cnId.ToString(),
                _cnName = dailyDetailById._cnName,
                _egiName = dailyDetailById._egiName
            };
        }

        public async Task<(IEnumerable<AllDailyWorkLogDTO>, Pagination pagination)> GetAllDaily(int page, int pageSize, string startDate, string endDate)
        {
            DateTime?  startDateParse = string.IsNullOrEmpty(startDate) ? null : DateTime.Parse(startDate);
            DateTime?  endDateParse = string.IsNullOrEmpty(startDate) ? null : DateTime.Parse(startDate);
            var paginatedDailyWorkLogs = await _dailyRepository.GetPaginatedDailyWorkLogs(page, pageSize,startDateParse, 
                endDateParse);
            return (paginatedDailyWorkLogs.Items, paginatedDailyWorkLogs.Pagination);
        }

        public async Task<bool> DeleteAllDaily(string id)
        {
            var guid = Guid.Parse(id);
            var deleteAllDailyWorkLogs = await _dailyRepository.DeleteAllDailyWorkLogs(guid);
            if (!deleteAllDailyWorkLogs)
            {
                throw new NotFoundException("Given Id not found");
            }
            return deleteAllDailyWorkLogs;
        }

        public async Task<bool> DeleteFormDaily(string id)
        {
            var guid = Guid.Parse(id);
            var deleteFormDaily = await _dailyRepository.DeleteFormDaily(guid);
            if (!deleteFormDaily)
            {
                throw new NotFoundException("Given Id not found");
            }
            return deleteFormDaily;
        }
    }
}
