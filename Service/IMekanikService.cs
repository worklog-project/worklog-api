using worklog_api.Model;

namespace worklog_api.Service
{
    public interface IMekanikService
    {
        MekanikModel GetMekanikById(int id);
        void CreateMekanik(MekanikModel mekanik);
    }
}