using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using worklog_api.error;
using worklog_api.Model;
using worklog_api.Repository;
using worklog_api.Repository.implementation;

namespace worklog_api.Service
{
    public class MOLService : IMOLService
    {
        private readonly IMOLRepository _molRepository;
        private readonly IStatusHistoryRepository _statusHistoryRepository;

        public MOLService(IMOLRepository molRepository, IStatusHistoryRepository statusHistoryRepository)
        {
            _molRepository = molRepository;
            _statusHistoryRepository = statusHistoryRepository;
        }

        public async Task<(IEnumerable<MOLModel> mols, int totalCount)> GetAllMOLs(int pageNumber, int pageSize, string sortBy, string sortDirection, DateTime? startDate, DateTime? endDate, string requestBy)
        {
            return await _molRepository.GetAll(pageNumber, pageSize, sortBy, sortDirection, startDate, endDate, requestBy);
        }

        public async Task<MOLModel> GetMOLById(Guid id)
        {
            return await _molRepository.GetById(id);
        }

        public async Task CreateMOL(MOLModel mol)
        {
            Console.WriteLine("Create MOL");
            try
            {
                await _molRepository.Create(mol);
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
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

        public async Task ApproveMOL(StatusHistoryModel status, UserModel user)
        {
            var mol = await _molRepository.GetById(status.MOLID);
            if (mol == null)
            {
                throw new NotFoundException("MOL Not Found");
            }

            if (user.role == "Group Leader" && mol.Status == "PENDING")
            {
                status.Status = "APPROVED_GROUP_LEADER";
            } 
            else if (user.role == "Data Planner" && mol.Status == "APPROVED_GROUP_LEADER") 
            {
                status.Status = "APPROVED_DATA_PLANNER";
            }
            else
            {
                throw new AuthorizationException("Invalid Role Or Status Already Updated");
            }

            try
            {
                await _statusHistoryRepository.Create(status);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw new InternalServerError(e.Message);
            }
        }
    }
}
