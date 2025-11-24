using Microsoft.AspNetCore.Mvc;
using SchedulingSystem.API.Dtos.Schedule;
using SchedulingSystem.API.Services.ScheduleServices;

namespace SchedulingSystem.API.ApiController.ScheduleApi
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly IScheduleService _scheduleService;

        public EmployeesController(IScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        // GET /api/employees/{id}/schedules?year=2025&month=12
        [HttpGet("{id:int}/schedules")]
        public async Task<ActionResult<List<ScheduleResponse>>> GetSchedulesByEmployee(
            int id,
            [FromQuery] int year,
            [FromQuery] int month)
        {
            // 之後如果要做權限（員工只能看自己、老闆可查別人）
            // 可以從 User 取目前登入者 id / role 丟進 Service 做判斷。
            var result = await _scheduleService.GetByEmployeeAndMonthAsync(id, year, month);
            return Ok(result);
        }
    }
}
