using Microsoft.AspNetCore.Mvc;
using SchedulingSystem.API.Services.Calend;
using SchedulingSystem.API.Dtos.Calend;

namespace SchedulingSystem.API.ApiController.Calend
{
    [ApiController]
    [Route("api/[controller]")]
    public class CalendarController : ControllerBase
    {
        private readonly ICalendarService _calendarService;

        public CalendarController(ICalendarService calendarService)
        {
            _calendarService = calendarService;
        }

        // GET /api/calendar?year=2025&month=11
        [HttpGet]
        public async Task<ActionResult<List<CalendarDay>>> GetMonthlyCalendar(
            [FromQuery] int? year,
            [FromQuery] int? month)
        {
            var today = DateTime.Today;
            var y = year ?? today.Year;
            var m = month ?? today.Month;

            var result = await _calendarService.GetMonthlyCalendarAsync(y, m);
            return Ok(result);
        }
    }
}