using SchedulingSystem.API.Dtos.Schedule;

namespace SchedulingSystem.API.Services.ScheduleServices
{
    public interface IScheduleService
    {
        Task<ScheduleResponse> CreateAsync(CreateScheduleRequest req);

        Task DeleteAsync(int scheduleId, DeleteScheduleRequest req);

        Task<List<ScheduleResponse>> GetByEmployeeAndMonthAsync(int employeeId, int year, int month);

        //老闆看排行榜

        Task<List<ScheduleLeaderboard>> GetMonthlyLeaderboardAsync(int year, int month);
    }


}
