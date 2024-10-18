using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using worklog_api.Infrastructure;
using worklog_api.Model;

namespace worklog_api.Repository
{
    public class MOLRepository : IMOLRepository
    {
        private readonly ApplicationDbContext _context;

        public MOLRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MOLModel>> GetAll()
        {
            return await _context.MOLs.Include(m => m.StatusHistories)
                                      .Include(m => m.TrackingHistories)
                                      .ToListAsync();
        }

        public async Task<MOLModel> GetById(Guid id)
        {
            return await _context.MOLs.Include(m => m.StatusHistories)
                                      .Include(m => m.TrackingHistories)
                                      .FirstOrDefaultAsync(m => m.ID == id);
        }

        public async Task Create(MOLModel mol)
        {
            await _context.MOLs.AddAsync(mol);
            await _context.SaveChangesAsync();
        }

        public async Task Update(MOLModel mol)
        {
            _context.MOLs.Update(mol);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(Guid id)
        {
            var mol = await _context.MOLs.FindAsync(id);
            if (mol != null)
            {
                _context.MOLs.Remove(mol);
                await _context.SaveChangesAsync();
            }
        }
    }
}
