namespace SchedulingSystem.API.Exceptions
{
    /// <summary>
    /// 排班衝突，例如同一位員工在同一天已經有班。
    /// 這屬於業務邏輯衝突，最後會轉成 HTTP 409 Conflict。
    /// </summary>
    public class ScheduleConflictException : BusinessException
    {
        public ScheduleConflictException(string message) : base(message)
        {
        }
    }
}
