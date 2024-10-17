using worklog_api.Model;

namespace worklog_api.Repository
{
    public interface IMekanikRepository
    {
        MekanikModel GetById(int id);
        void Add(MekanikModel mekanik);
    }
}
