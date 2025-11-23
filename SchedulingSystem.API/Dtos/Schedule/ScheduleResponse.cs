namespace SchedulingSystem.API.Dtos.Schedule
{
    public class ScheduleResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ShiftTypeId { get; set; }
        public DateTime WorkDate { get; set; }
    }
}
