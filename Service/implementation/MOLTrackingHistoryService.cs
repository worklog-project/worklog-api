using System.Threading.Tasks;
using worklog_api.Model;
using worklog_api.Repository;

namespace worklog_api.Service
{
    public class MOLTrackingHistoryService : IMOLTrackingHistoryService
    {
        private readonly IMOLTrackingHistoryRepository _repository;
        private readonly IMOLRepository _molRepository;

        public MOLTrackingHistoryService(IMOLTrackingHistoryRepository repository, IMOLRepository molRepository)
        {
            _repository = repository;
            _molRepository = molRepository;
        }

        // Service only receives the model and passes it to the repository
        public async Task Create(MOLTrackingHistoryModel trackingHistory)
        {
            // Check if the MOL exists
            var mol = await _molRepository.GetById(trackingHistory.MOLID);
            if (mol == null)
            {
                throw new System.Exception("MOL not found");
            }

            // check MOL Status
            if (mol.Status != "APPROVED_DATA_PLANNER")
            {
                throw new System.Exception("MOL is not in the correct status to be tracked");
            }

            await _repository.Create(trackingHistory);
        }
    }
}
