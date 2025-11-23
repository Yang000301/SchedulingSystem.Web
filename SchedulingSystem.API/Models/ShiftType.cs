using System.ComponentModel.DataAnnotations.Schema;

namespace SchedulingSystem.API.Models
{
    [Table("shift_types")]
    public class ShiftType
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; } = "";

        [Column("start_time")]
        public TimeSpan StartTime { get; set; }

        [Column("end_time")]
        public TimeSpan EndTime { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        [Column("sort_order")]
        public int SortOrder { get; set; }
    }
}
