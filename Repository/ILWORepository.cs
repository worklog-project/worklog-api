﻿using System;
using System.Threading.Tasks;
using worklog_api.Model;

namespace worklog_api.Repository
{
    public interface ILWORepository
    {
        Task<(IEnumerable<LWOModel>, int totalCount)> GetAll(int pageNumber, int pageSize, string sortBy, string sortDirection, DateTime? startDate, DateTime? endDate, string requestBy);
        Task<LWOModel> GetById(Guid id);  // Method to get LWO by ID
        Task<Guid> Create(LWOModel lwo);        // Method to create a new LWO
        Task CreateMetadataByLWOID(LWOMetadataModel metadata);
        Task Update(LWOModel lwo);        // Method to update an existing LWO
        Task DeleteMetadataByID(Guid metadataID);
        Task Delete(Guid id);             // Method to delete an LWO by ID
    } 
}
