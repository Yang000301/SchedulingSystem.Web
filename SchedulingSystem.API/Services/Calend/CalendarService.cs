using SchedulingSystem.API.Dtos.Calend;
using SchedulingSystem.API.Services.HolidayInfoServices;

namespace SchedulingSystem.API.Services.Calend
{
    public class CalendarService : ICalendarService
    {
        private readonly IHolidayService _holidayService;

        public CalendarService(IHolidayService holidayService)
        {
            _holidayService = holidayService;
        }

        public async Task<List<CalendarDay>> GetMonthlyCalendarAsync(int year, int month)
        {
            var daysInMonth = DateTime.DaysInMonth(year, month);
            var list = new List<CalendarDay>(daysInMonth);

            // 1) 先拿到「這個月的國定假日 / 補班」資訊
            var holidayMap = await _holidayService.GetMonthHolidayMapAsync(year, month);

            for (int day = 1; day <= daysInMonth; day++)
            {
                var date = new DateTime(year, month, day);
                var dow = (int)date.DayOfWeek;
                var isWeekend = dow == 0 || dow == 6;

                // 2) 看這一天有沒有外部假日設定
                HolidayInfo? info = null;
                holidayMap.TryGetValue(date.Date, out info);

                var dto = new CalendarDay
                {
                    Date = date,
                    DayOfWeek = dow,
                    IsWeekend = isWeekend,
                    // 如果外部有資料 → 用外部邏輯  
                    // 外部沒有 → 平日=上班／週末=不上班
                    IsWorkingDay = info?.IsWorkingDay ?? !isWeekend,
                    HolidayType = info?.HolidayType ?? (isWeekend ? "Weekend" : "Normal"),
                    HolidayName = info?.HolidayName
                };

                list.Add(dto);
            }

            return list;
        }
    }
}
