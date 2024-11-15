using worklog_api.Model;
using worklog_api.Model.dto;
using worklog_api.Model.form;

namespace worklog_api.Service
{
    public interface IDailyService
    {
        Task<IEnumerable<EGIModel>> GetEGI(string egi);

        Task<IEnumerable<CodeNumberModel>> GetCodeNumber(string codeNumber, Guid egiID);

        Task<string> InsertDaily(DailyRequest dailyRequest);
    }
}
