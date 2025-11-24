namespace SchedulingSystem.API.Services.Calend
{
    // <summary>
    /// 給系統內部用的「這一天的假日資訊」統一格式
    /// </summary>
    public class HolidayInfo
    {
        public bool IsHoliday { get; set; }          // 這天從人類角度看是不是假日
        public bool IsWorkingDay { get; set; }       // 這天實際要不要上班（補班日 = true）
        public string HolidayType { get; set; } = "Normal";  // Normal / Weekend / NationalHoliday / MakeUpWorkday / CompanyHoliday...
        public string? HolidayName { get; set; }     // 節日名稱（例：元旦），沒有就 null
    }
}
