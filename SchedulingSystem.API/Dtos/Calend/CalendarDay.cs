namespace SchedulingSystem.API.Dtos.Calend
{
    public class CalendarDay
    {
        public DateTime Date { get; set; }
        public int DayOfWeek { get; set; }      // 0=Sunday...6=Saturday
        public bool IsWeekend { get; set; }

        // 今天是不是要上班（補班日 = true，國定假日 = false）
        public bool IsWorkingDay { get; set; }

        // Normal / Weekend / NationalHoliday / MakeUpWorkday
        public string HolidayType { get; set; } = "Normal";

        // 國定假日名稱（例如「春節」，沒東西就 null）
        public string? HolidayName { get; set; }
    }
}
