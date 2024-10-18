using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            Console.Write("ini insert ke db");
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            _context.MOLs.Add(mol);
            stopwatch.Stop();
            Console.WriteLine("Time for adding entity: " + stopwatch.ElapsedMilliseconds + "ms");
     
            stopwatch.Restart();
            await _context.SaveChangesAsync();
            stopwatch.Stop();
            Console.WriteLine("Time for saving changes: " + stopwatch.ElapsedMilliseconds + "ms");
        }

        public async Task Update(MOLModel mol)
        {
            _context.MOLs.Update(mol);
            _context.SaveChanges();
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
