using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using worklog_api.error;
using worklog_api.Model;
using worklog_api.Repository;

namespace worklog_api.Service
{
    public class MOLService : IMOLService
    {
        private readonly IMOLRepository _molRepository;

        public MOLService(IMOLRepository molRepository)
        {
            _molRepository = molRepository;
        }

        public async Task<IEnumerable<MOLModel>> GetAllMOLs()
        {
            return await _molRepository.GetAll();
        }

        public async Task<MOLModel> GetMOLById(Guid id)
        {
            return await _molRepository.GetById(id);
        }

        public async Task CreateMOL(MOLModel mol)
        {
            try
            {
                await _molRepository.Create(mol);
                
            }
            catch (Exception e)
            {
                throw new InternalServerError(e.Message);
            }
            

        }

        public async Task UpdateMOL(MOLModel mol)
        {
            await _molRepository.Update(mol);
        }

        public async Task DeleteMOL(Guid id)
        {
            await _molRepository.Delete(id);
        }
    }
}
