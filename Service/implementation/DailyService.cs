using System.Collections.Generic;
using System.Threading.Tasks;
using worklog_api.error;
using worklog_api.Model;
using worklog_api.Model.dto;
using worklog_api.Model.form;
using worklog_api.Repository;

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

            if (dailyByEgiAndCodeNumberAndDate._count > 3)
            {
                throw new BadRequestException("Daily already has been filled");
            }
            
            var insertDailyDetail = new Guid();
            if (dailyByEgiAndCodeNumberAndDate == null)
            {
                DailyModel dailyModel = new DailyModel()
                {
                    _id = Guid.NewGuid(),
                    _date = DateTime.Parse(dailyRequest._date),
                    _cnId = Guid.Parse(dailyRequest._cnId),
                    _count = 0,
                    _egiId = Guid.Parse(dailyRequest._egiId),
                    _groupLeader = dailyRequest._groupLeader,
                    _mechanic = dailyRequest._mechanic,
                };
                await _dailyRepository.insertDaily(dailyModel);
                var dailyDetail =
                    await _dailyRepository.insertDailyDetail(dailyRequest._form, Guid.NewGuid());
                return dailyDetail.ToString();
            } else
            {
                dailyRequest._form._dailyId = dailyByEgiAndCodeNumberAndDate._id;
                var dailyDetail = await _dailyRepository.insertDailyDetail(dailyRequest._form, Guid.NewGuid());
                return dailyDetail.ToString();
            }
        }
    }
}
