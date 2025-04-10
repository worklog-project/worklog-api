﻿using worklog_api.helper;
using worklog_api.Model;
using worklog_api.Model.dto;
using worklog_api.Model.form;
using worklog_api.Repository.implementation;

namespace worklog_api.Service
{
    public interface IDailyService
    {
        Task<IEnumerable<EGIModel>> GetEGI(string egi);

        Task<IEnumerable<CodeNumberModel>> GetCodeNumber(string codeNumber, Guid egiID);

        Task<string> InsertDaily(DailyRequest dailyRequest);
        Task<DailyWorklogDetailResponse> GetDailyDetailByID(string id);
        Task<(IEnumerable<AllDailyWorkLogDTO>, Pagination pagination)> GetAllDaily(int page, int pageSize, string startDate, string endDate);
        Task<bool> DeleteAllDaily(string id);
        Task<bool> DeleteFormDaily(string id);
    }
}
