using SchedulingSystem.API.Models.External;

namespace SchedulingSystem.API.Services.External
{
    public interface ITaiwanCalendarClient
    {
        /// <summary>
        /// 取得某年的 TaiwanCalendar 原始資料（1/1 ~ 12/31 全部）
        /// </summary>
        Task<List<TaiwanCalendarDay>> GetYearAsync(int year);
    }
}


//之後要寫 fake 實作測試 / demo 很方便。

//你真正的 HttpClient 實作只是其中一種版本。