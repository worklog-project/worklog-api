using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using worklog_api.Model;

namespace worklog_api.Repository
{
    public interface IStatusHistoryRepository
    {
        Task Create(StatusHistoryModel statusHistory);
    }
}
