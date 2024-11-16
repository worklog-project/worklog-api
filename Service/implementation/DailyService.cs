using System.Collections.Generic;
using System.Threading.Tasks;
using worklog_api.error;
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

        public DailyService(IDailyRepository dailyRepository)
        {
            _dailyRepository = dailyRepository;
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
            var dailyByEgiAndCodeNumberAndDate =  
                await _dailyRepository.getDailyByEgiAndCodeNumberAndDate(dailyRequest._egiId, dailyRequest._cnId, dailyRequest._date);


            var generateId = Guid.NewGuid();
            DailyModel dailyModel = new DailyModel()
            {
                _id = generateId,
                _date = DateTime.Parse(dailyRequest._date),
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
                var insertDaily = await _dailyRepository.insertDaily(dailyModel);
                dailyModel._dailyId = insertDaily;
                var dailyDetail =
                    await _dailyRepository.insertDailyDetail(dailyModel, generateId);
                return dailyDetail.ToString();
            } else
            {
                if (dailyByEgiAndCodeNumberAndDate._count > 3)
                {
                    throw new BadRequestException("Daily already has been filled");
                }
                
                dailyRequest._dailyId = dailyByEgiAndCodeNumberAndDate._id;
                var dailyDetail = await _dailyRepository.insertDailyDetail(dailyModel, generateId);
                return dailyDetail.ToString();
            }
        }

        public async Task<DailyWorklogDetailResponse> GetDailyDetailByID(string id)
        {
            var guid = Guid.Parse(id);
            var dailyDetailById = await _dailyRepository.getDailyDetailById(guid);
            return new DailyWorklogDetailResponse()
            {
                _id = dailyDetailById._id.ToString(),
                _sheetDetail = dailyDetailById._sheetDetail,
                _hourmeter = dailyDetailById._hourmeter,
                _startTime = dailyDetailById._startTime,
                _endTime = dailyDetailById._endTime,
                _formType = dailyDetailById._formType,
            };
        }

        public async Task<IEnumerable<AllDailyWorkLogDTO>> GetAllDaily(int page, int pageSize)
        {
            var paginatedDailyWorkLogs = await _dailyRepository.GetPaginatedDailyWorkLogs(page, pageSize);
            return paginatedDailyWorkLogs.Items;
        }
    }
}
