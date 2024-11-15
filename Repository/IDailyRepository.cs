using worklog_api.Model;
using worklog_api.Model.dto;
using worklog_api.Model.form;

namespace worklog_api.Repository
{
    public interface IDailyRepository
    {
        Task<IEnumerable<EGIModel>> GetEGI(string egi);

        Task<IEnumerable<CodeNumberModel>> GetCodeNumber(string codeNumber, Guid egiID);
        
        Task<DailyModel> getDailyByEgiAndCodeNumberAndDate(string egi, string codeNumber, string date);
        
        Task insertDaily(DailyModel dailyModel);

        Task<Guid> insertDailyDetail(FormDTO formDTO, Guid generateId);
    }
}
