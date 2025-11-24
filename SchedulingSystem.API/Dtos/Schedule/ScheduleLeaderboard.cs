namespace SchedulingSystem.API.Dtos.Schedule
{
    public class ScheduleLeaderboard
    {
        public int UserId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public int TotalShifts { get; set; }      // 出勤次數
        //public double TotalHours { get; set; }     // 出勤時數
    }
}
