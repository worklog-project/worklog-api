using worklog_api.Model;
using worklog_api.Repository;

namespace worklog_api.Service
{
    public class MekanikService : IMekanikService
    {
        private readonly IMekanikRepository _mekanikRepository;

        public MekanikService(IMekanikRepository mekanikRepository)
        {
            _mekanikRepository = mekanikRepository;
        }

        public MekanikModel GetMekanikById(int id)
        {
            return _mekanikRepository.GetById(id);
        }

        public void CreateMekanik(MekanikModel mekanik)
        {
            _mekanikRepository.Add(mekanik);
        }
    }
}
