namespace SchedulingSystem.API.Exceptions
{
    /// <summary>
    /// 使用者沒有權限執行動作時丟出，例如員工想修改別人的班表。
    /// 最後會對應成 HTTP 403 Forbidden。
    /// </summary>
    public class ForbiddenException : BusinessException
    {
        public ForbiddenException(string message) : base(message)
        {
        }
    }
}
