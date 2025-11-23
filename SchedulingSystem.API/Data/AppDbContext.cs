using Microsoft.EntityFrameworkCore;
using SchedulingSystem.API.Models;








namespace SchedulingSystem.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<ShiftType> ShiftTypes { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
    }
}
