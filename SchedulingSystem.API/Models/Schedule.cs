using System.ComponentModel.DataAnnotations.Schema;

namespace SchedulingSystem.API.Models
{
    [Table("schedules")]
    public class Schedule
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("shift_type_id")]
        public int ShiftTypeId { get; set; }

        [Column("work_date")]
        public DateTime WorkDate { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("created_by_user_id")]
        public int? CreatedByUserId { get; set; }
    }
}
