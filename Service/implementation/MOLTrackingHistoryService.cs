using System.Threading.Tasks;
using worklog_api.Model;
using worklog_api.Repository;

namespace worklog_api.Service
{
    public class MOLTrackingHistoryService : IMOLTrackingHistoryService
    {
        private readonly IMOLTrackingHistoryRepository _repository;
        private readonly IMOLRepository _molRepository;
        private readonly IStatusHistoryRepository _statusHistoryRepository;

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

            if (mol.Status == "COMPLETED")
            {
                throw new System.Exception("MOL Request Already Completed");
            }

            // check MOL Status
            if (mol.Status == trackingHistory.Status)
            {
                throw new System.Exception("MOL already updated");
            }
            

            await _repository.Create(trackingHistory);
        }
    }
}
