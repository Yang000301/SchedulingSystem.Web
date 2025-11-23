using SchedulingSystem.API.Dtos.Schedule;
using SchedulingSystem.API.Exceptions;
using SchedulingSystem.API.Repositories.ScheduleRepos;
using SchedulingSystem.API.Models;

namespace SchedulingSystem.API.Services.ScheduleServices
{
    public class ScheduleService : IScheduleService
    {
        private readonly IScheduleRepository _repo;

        public ScheduleService(IScheduleRepository repo)
        {
            _repo = repo;
        }

        public async Task<ScheduleResponse> CreateAsync(CreateScheduleRequest req)
        {
            // 1. 檢查是否已排班（同一天同一使用者）
            var exists = await _repo.ExistsAsync(req.UserId, req.WorkDate);
            if (exists)
                throw new ScheduleConflictException("這個日期已經有排班了");

            // 2. 建 Schedule
            var entity = new Schedule
            {
                UserId = req.UserId,
                ShiftTypeId = req.ShiftTypeId,
                WorkDate = req.WorkDate,
                CreatedAt = DateTime.UtcNow
            };

            entity = await _repo.CreateAsync(entity);

            // 3. 回 Response
            return new ScheduleResponse
            {
                Id = entity.Id,
                UserId = entity.UserId,
                ShiftTypeId = entity.ShiftTypeId,
                WorkDate = entity.WorkDate
            };
        }
    }
}
