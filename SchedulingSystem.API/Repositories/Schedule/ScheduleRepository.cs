using SchedulingSystem.API.Data;
using Microsoft.EntityFrameworkCore;
using SchedulingSystem.API.Models;

namespace SchedulingSystem.API.Repositories.ScheduleRepos
{
    public class ScheduleRepository : IScheduleRepository
    {
        private readonly AppDbContext _db;

        public ScheduleRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<bool> ExistsAsync(int userId, DateTime workDate)
        {
            return await _db.Schedules
                .AnyAsync(s => s.UserId == userId && s.WorkDate == workDate);
        }

        public async Task<Schedule?> GetByIdAsync(int id)
        {
            return await _db.Schedules.FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Schedule> CreateAsync(Schedule schedule)
        {
            _db.Schedules.Add(schedule);
            await _db.SaveChangesAsync();
            return schedule;
        }
    }
}
