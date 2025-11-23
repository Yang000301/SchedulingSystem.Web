namespace SchedulingSystem.API.Exceptions
{
    /// <summary>
    /// 所有「預期內的業務邏輯錯誤」的基底。
    /// 排班衝突、查無資料、權限不足……都屬於 BusinessException。
    /// 後續全域 Exception Middleware 會統一把它轉成適合的 HTTP 回應。
    /// </summary>
    public class BusinessException : Exception
    {
        public BusinessException(string message) : base(message)
        {
        }
    }
}
