using Microsoft.AspNetCore.Mvc;
using SchedulingSystem.API.Dtos.Schedule;
using SchedulingSystem.API.Services.ScheduleServices;

namespace SchedulingSystem.API.ApiController.ScheduleApi
{
    [ApiController]
    [Route("api/[controller]")]
    public class SchedulesController : ControllerBase
    {
        private readonly IScheduleService _service;

        public SchedulesController(IScheduleService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateScheduleRequest req)
        {
            var res = await _service.CreateAsync(req);
            return Ok(res);
        }
    }
}
