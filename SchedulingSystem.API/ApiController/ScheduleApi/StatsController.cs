using Microsoft.AspNetCore.Mvc;
using SchedulingSystem.API.Dtos.Schedule;
using SchedulingSystem.API.Services.ScheduleServices;

namespace SchedulingSystem.API.ApiController.ScheduleApi
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatsController : ControllerBase
    {
        private readonly IScheduleService _scheduleService;

        public StatsController(IScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        /// <summary>
        /// 排班排行榜（依排班天數排序）
        /// GET /api/stats/leaderboard?year=2025&month=11
        /// </summary>
        [HttpGet("leaderboard")]
        public async Task<ActionResult<List<ScheduleLeaderboard>>> GetLeaderboard(
            [FromQuery] int? year,
            [FromQuery] int? month)
        {
            // year/month 沒給 → 自動帶入當月
            var today = DateTime.Today;
            var y = year ?? today.Year;
            var m = month ?? today.Month;

            var result = await _scheduleService.GetMonthlyLeaderboardAsync(y, m);

            return Ok(result);
        }
    }
}
