using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using worklog_api.Model;

namespace worklog_api.Infrastructure{ 
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<MOLModel> MOLs { get; set; }
    public DbSet<StatusHistoryModel> StatusHistories { get; set; }
    public DbSet<MOLTrackingHistoryModel> MOLTrackingHistories { get; set; }
}

}