using worklog_api.Model;

namespace worklog_api.Repository
{
    public class MekanikRepository : IMekanikRepository
    {
        private readonly ApplicationDbContext _context;

        public MekanikRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public MekanikModel GetById(int id)
        {
            return _context.MekanikModels.Find(id);
        }

        public void Add(MekanikModel mekanik)
        {
            _context.MekanikModels.Add(mekanik);
            _context.SaveChanges();
        }
    }
}
