using SchedulingSystem.API.Models.External;
using SchedulingSystem.API.Services.Calend;
using SchedulingSystem.API.Services.External;
using SchedulingSystem.API.Services.HolidayInfoServices;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace SchedulingSystem.API.Services.Calendar
{
    /// <summary>
    /// 把 TaiwanCalendar 原始資料 + 公司規則
    /// 轉成統一的 HolidayInfo 給系統其他地方用。
    /// </summary>
    public class HolidayService : IHolidayService
    {
       
            private readonly ITaiwanCalendarClient _client;

            public HolidayService(ITaiwanCalendarClient client)
            {
                _client = client;
            }

            public async Task<Dictionary<DateTime, HolidayInfo>> GetMonthHolidayMapAsync(int year, int month)
            {
                // 1) 抓整年 JSON
                var allDays = await _client.GetYearAsync(year);

                // 2) 挑出這個月，例如 2025 + 01 → "202501"
                var monthStr = month.ToString("00");
                var prefix = $"{year}{monthStr}";

                var targetDays = allDays
                    .Where(d => d.Date.StartsWith(prefix))
                    .ToList();

                var dict = new Dictionary<DateTime, HolidayInfo>();

                foreach (var d in targetDays)
                {
                    // "20250101" → DateTime
                    if (!DateTime.TryParseExact(
                            d.Date,
                            "yyyyMMdd",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out var date))
                    {
                        continue;
                    }

                    var isWeekend = date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
                    var info = new HolidayInfo();

                    // 外部直接給 bool
                    var isHoliday = d.IsHoliday;

                    if (isHoliday)
                    {
                        // 放假
                        if (!string.IsNullOrWhiteSpace(d.Description))
                        {
                            // 有名稱 → 國定假日
                            info.IsHoliday = true;
                            info.IsWorkingDay = false;
                            info.HolidayType = "NationalHoliday";
                            info.HolidayName = d.Description;
                        }
                        else
                        {
                            // 沒名稱 → 通常是一般週末
                            info.IsHoliday = true;
                            info.IsWorkingDay = false;
                            info.HolidayType = "Weekend";
                            info.HolidayName = null;
                        }
                    }
                    else
                    {
                        // 不放假
                        if (isWeekend)
                        {
                            // 週末但不放假 → 補班日
                            info.IsHoliday = false;
                            info.IsWorkingDay = true;
                            info.HolidayType = "MakeUpWorkday";
                            info.HolidayName = "補班日";
                        }
                        else
                        {
                            // 平日
                            info.IsHoliday = false;
                            info.IsWorkingDay = true;
                            info.HolidayType = "Normal";
                            info.HolidayName = null;
                        }

                        // 未來公司特休日要硬加就寫在這裡 if (date == 某天) ...
                    }

                    dict[date.Date] = info;
                }

                return dict;
            }
        

        // 小工具：支援 "yyyyMMdd" 與 "yyyy-MM-dd"
        private static bool TryParseDate(string raw, out DateTime date)
        {
            var formats = new[] { "yyyyMMdd", "yyyy-MM-dd" };
            return DateTime.TryParseExact(
                raw,
                formats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out date
            );
        }
    }
}
