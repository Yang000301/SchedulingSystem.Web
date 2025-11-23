namespace SchedulingSystem.API.Dtos.Schedule
{
    public class CreateScheduleRequest
    {
        public int UserId { get; set; }
        public int ShiftTypeId { get; set; }
        public DateTime WorkDate { get; set; }
    }
}
