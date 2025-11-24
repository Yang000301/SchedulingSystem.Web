namespace SchedulingSystem.API.Dtos.Schedule
{
    public class DeleteScheduleRequest
    {
        public int RequestUserId { get; set; }   // 誰在刪？
        public string RequestUserRole { get; set; } = ""; // "employee" or "boss"

    }
}
