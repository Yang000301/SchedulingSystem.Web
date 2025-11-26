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
        /// 月排行榜（依排班天數排序）
        /// GET /api/stats/leaderboard?year=2025&month=11
        /// year / month 沒給 → 用當月
        /// </summary>
        [HttpGet("leaderboard")]
        public async Task<ActionResult<List<ScheduleLeaderboard>>> GetMonthlyLeaderboard(
            [FromQuery] int? year,
            [FromQuery] int? month)
        {
            var today = DateTime.Today;
            var y = year ?? today.Year;
            var m = month ?? today.Month;

            var result = await _scheduleService.GetMonthlyLeaderboardAsync(y, m);
            return Ok(result);
        }

        /// <summary>
        /// 年排行榜（依排班天數排序）
        /// GET /api/stats/leaderboard/yearly?year=2025
        /// year 沒給 → 用今年
        /// </summary>
        [HttpGet("leaderboard/yearly")]
        public async Task<ActionResult<List<ScheduleLeaderboard>>> GetYearlyLeaderboard(
            [FromQuery] int? year)
        {
            var y = year ?? DateTime.Today.Year;

            var result = await _scheduleService.GetYearlyLeaderboardAsync(y);
            return Ok(result);
        }
    }
}
