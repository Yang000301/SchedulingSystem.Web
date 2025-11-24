using SchedulingSystem.API.Dtos.Calend;

namespace SchedulingSystem.API.Services.Calend
{
    public interface ICalendarService
    {
        Task<List<CalendarDay>> GetMonthlyCalendarAsync(int year, int month);
    }
}
