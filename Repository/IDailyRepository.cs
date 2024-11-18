using worklog_api.Model;
using worklog_api.Model.dto;
using worklog_api.Repository.implementation;

namespace worklog_api.Repository
{
    public interface IDailyRepository
    {
        Task<IEnumerable<EGIModel>> GetEGI(string egi);

        Task<IEnumerable<CodeNumberModel>> GetCodeNumber(string codeNumber, Guid egiID);
        
        Task<DailyModel> getDailyByEgiAndCodeNumberAndDate(string egi, string codeNumber, string date);
        
        Task<Guid> insertDaily(DailyModel dailyModel, Guid scheduleId);

        Task<Guid> insertDailyDetail(DailyModel dailyModel, Guid generateId, Guid scheduleId, int counted);

        Task<(IEnumerable<AllDailyWorkLogDTO> Items, int TotalCount)> GetPaginatedDailyWorkLogs(int pageNumber, int pageSize);
         
        Task<DailyModel> getDailyDetailById(Guid id);
        
        Task<string> GetEgiNameByID(string id);
        Task<ScheduleDetail> GetScheduleDetailById(string id);
        
        Task <bool> DeleteAllDailyWorkLogs(Guid scheduleId);
        Task <bool> DeleteFormDaily(Guid scheduleId);
        
    }
}
