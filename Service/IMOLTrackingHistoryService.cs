using System.Threading.Tasks;
using worklog_api.Model;

namespace worklog_api.Service
{
    public interface IMOLTrackingHistoryService
    {
         Task  Create(MOLTrackingHistoryModel trackingHistory);
    }
}
