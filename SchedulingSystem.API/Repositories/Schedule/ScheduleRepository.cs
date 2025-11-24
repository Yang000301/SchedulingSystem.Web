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

        // -------------------------------
        // ✔ 是否同一天同一使用者已排班
        // -------------------------------
        public async Task<bool> ExistsAsync(int userId, DateTime workDate)
        {
            return await _db.Schedules
                .AnyAsync(s =>
                    s.UserId == userId &&
                    s.WorkDate == workDate.Date);
        }

        // -------------------------------
        // ✔ 取得單筆排班
        // -------------------------------
        public async Task<Schedule?> GetByIdAsync(int id)
        {
            return await _db.Schedules
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        // -------------------------------
        // ✔ 建立排班
        // -------------------------------
        public async Task<Schedule> CreateAsync(Schedule schedule)
        {
            _db.Schedules.Add(schedule);
            await _db.SaveChangesAsync();
            return schedule;
        }

        // -------------------------------
        // 🔥 當天已排幾個人（最多 2 人用）
        // -------------------------------
        public async Task<int> CountByDateAsync(DateTime workDate)
        {
            return await _db.Schedules
                .CountAsync(s => s.WorkDate == workDate.Date);
        }

        // -------------------------------
        // 🔥 某人這個月排了幾天（最多 15 天用）
        // -------------------------------
        public async Task<int> CountByEmployeeAndMonthAsync(int userId, DateTime anyDateInMonth)
        {
            var year = anyDateInMonth.Year;
            var month = anyDateInMonth.Month;

            return await _db.Schedules
                .CountAsync(s =>
                    s.UserId == userId &&
                    s.WorkDate.Year == year &&
                    s.WorkDate.Month == month);
        }

        //刪除用
        public async Task DeleteAsync(Schedule schedule)
        {
            _db.Schedules.Remove(schedule);
            await _db.SaveChangesAsync();
        }

        //查自己的班表
        public async Task<List<Schedule>> GetByEmployeeAndMonthAsync(int userId, int year, int month)
        {
            return await _db.Schedules
                .Where(s =>
                    s.UserId == userId &&
                    s.WorkDate.Year == year &&
                    s.WorkDate.Month == month)
                .OrderBy(s => s.WorkDate)
                .ToListAsync();
        }
        public async Task<List<Schedule>> GetByMonthAsync(int year, int month)
        {
            return await _db.Schedules
                .Include(s => s.User)       

                .Where(s =>
                    s.WorkDate.Year == year &&
                    s.WorkDate.Month == month)
                .ToListAsync();
        }
    }
}
    