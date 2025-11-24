using SchedulingSystem.API.Models.External;
using System.Text.Json;

namespace SchedulingSystem.API.Services.External
{
    /// <summary>
    /// 專門跟 https://cdn.jsdelivr.net/gh/ruyut/TaiwanCalendar/data/{year}.json 講話
    /// 只負責「抓資料 + 反序列化」，不管業務規則。
    /// </summary>
    public class TaiwanCalendarClient : ITaiwanCalendarClient
    {
        private readonly HttpClient _http;

        public TaiwanCalendarClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<TaiwanCalendarDay>> GetYearAsync(int year)
        {
            // 這個就是你剛貼的網址，只是把 2025 改成 year 變數
            var url = $"https://cdn.jsdelivr.net/gh/ruyut/TaiwanCalendar/data/{year}.json";

            var json = await _http.GetStringAsync(url);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var data = JsonSerializer.Deserialize<List<TaiwanCalendarDay>>(json, options)
                       ?? new List<TaiwanCalendarDay>();

            return data;
        }
    }
}
