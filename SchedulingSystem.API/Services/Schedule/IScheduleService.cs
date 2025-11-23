using SchedulingSystem.API.Dtos.Schedule;

namespace SchedulingSystem.API.Services.ScheduleServices
{
    public interface IScheduleService
    {
        Task<ScheduleResponse> CreateAsync(CreateScheduleRequest req);
    }
}
