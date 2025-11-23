namespace SchedulingSystem.API.Exceptions
{
    /// <summary>
    /// 查不到想要的資料，例如查不到員工、班別、排班紀錄。
    /// Service 層遇到這種情況就丟，最後會轉成 HTTP 404。
    /// </summary>
    public class NotFoundException : BusinessException
    {
        public NotFoundException(string message) : base(message)
        {
        }
    }
}
