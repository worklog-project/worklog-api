using System;
using System.Threading.Tasks;
using worklog_api.Model;
using worklog_api.Repository;

namespace worklog_api.Service
{
    public class LWOService : ILWOService
    {
        private readonly ILWORepository _lwoRepository;

        public LWOService(ILWORepository lwoRepository)
        {
            _lwoRepository = lwoRepository;
        }

        public async Task<IEnumerable<LWOModel>> GetAllLWOs()
        {
            return await _lwoRepository.GetAll();
        }

        public async Task<LWOModel> GetLWOById(Guid id)
        {
            return await _lwoRepository.GetById(id);
        }

        public async Task CreateLWO(LWOModel lwo)
        {
            try
            {
                await _lwoRepository.Create(lwo);
            }
            catch (Exception e)
            {
                throw new Exception("Error while creating LWO: " + e.Message);
            }
        }

        public async Task UpdateLWO(LWOModel lwo)
        {
            try
            {
                await _lwoRepository.Update(lwo);
            }
            catch (Exception e)
            {
                throw new Exception("Error while updating LWO: " + e.Message);
            }

        }

        public async Task DeleteLWO(Guid id)
        {
            try
            {
                await _lwoRepository.Delete(id);
            }
            catch (Exception e)
            {
                throw new Exception("Error while deleting LWO: " + e.Message);
            }
        }
    }
}
