using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using worklog_api.model;
using worklog_api.Model;
using worklog_api.Repository;

namespace worklog_api.Infrastructure
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Define DbSets for your entities (tables)
        public DbSet<MekanikModel> MekanikRepository { get; set; }
    }
}
