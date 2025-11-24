using SchedulingSystem.API.Services.Calend;

namespace SchedulingSystem.API.Services.HolidayInfoServices
{
    public interface IHolidayService
    {
        /// <summary>
        /// 取得某年某月所有「有規則」的日期：key = Date, value = HolidayInfo
        /// CalendarService 會用這個 map 來決定每一天怎麼標記。
        /// </summary>
        Task<Dictionary<DateTime, HolidayInfo>> GetMonthHolidayMapAsync(int year, int month);
    }
}
