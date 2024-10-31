using worklog_api.Model;

namespace worklog_api.Service
{
    public interface IDailyService
    {
        Task<IEnumerable<EGIModel>> GetEGI(string egi);

        Task<IEnumerable<CodeNumberModel>> GetCodeNumber(string codeNumber, Guid egiID);
    }
}
