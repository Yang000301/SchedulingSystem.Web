using System.Text.Json.Serialization;

namespace SchedulingSystem.API.Models.External
{
    //「對外 API 用」，跟你自己的 DTO 分開。
    // 專門對應 ruyut/TaiwanCalendar 的 JSON 格式
    public class TaiwanCalendarDay
    {
        [JsonPropertyName("date")]
        public string Date { get; set; } = "";          // "20250101"

        [JsonPropertyName("week")]
        public string Week { get; set; } = "";          // "一" ~ "日"

        [JsonPropertyName("isHoliday")]
        public bool IsHoliday { get; set; }             // true / false

        [JsonPropertyName("description")]
        public string Description { get; set; } = "";   // 假日名稱（一般週末就空字串）
    }
}
