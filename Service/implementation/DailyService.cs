using System.Collections.Generic;
using System.Threading.Tasks;
using worklog_api.Model;
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
    }
}
