using SchedulingSystem.API.Models;

namespace SchedulingSystem.API.Repositories.ScheduleRepos
{   
    public interface IScheduleRepository
    {
        Task<Schedule> CreateAsync(Schedule schedule);
        Task<bool> ExistsAsync(int userId, DateTime workDate);
        Task<Schedule?> GetByIdAsync(int id);
    }
}
