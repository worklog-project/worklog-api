﻿using worklog_api.Model;

namespace worklog_api.Repository
{
    public interface IDailyRepository
    {
        Task<IEnumerable<EGIModel>> GetEGI(string egi);

        Task<IEnumerable<CodeNumberModel>> GetCodeNumber(string codeNumber, Guid egiID);
    }
}
