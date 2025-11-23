using SchedulingSystem.API.Exceptions;

namespace SchedulingSystem.API.Middlewares
{
    // 全域例外處理的 Middleware
    // 放在 /Middlewares/GlobalExceptionMiddleware.cs 之類的地方
    public class GlobalExceptionMiddleware
    {
        // RequestDelegate = 下一個 middleware 要執行的東西（類似「把請求往後傳的 function 指標」）
        private readonly RequestDelegate _next;

        // 建構子：ASP.NET Core 會幫你注入「下一個 middleware」進來
        public GlobalExceptionMiddleware(RequestDelegate next)
        {
            _next = next;  // 存起來，等下 Invoke 要呼叫
        }

        // 每一個 HTTP 請求都會進來這裡跑一次
        // context = 這次 HTTP 請求 / 回應的所有資訊（路徑、header、body、response...）
        public async Task Invoke(HttpContext context)
        {
            try
            {
                // 把請求往 pipeline 的下一站丟（Controller、其他中介層等等）
                await _next(context);
            }
            // 第一層：只抓你自己定義的業務例外（BusinessException 以及它的子類）
            catch (BusinessException ex)
            {
                // 用 C# 的 pattern matching 判斷這是哪一種 BusinessException
                var statusCode = ex switch
                {
                    // 如果是 NotFoundException → 回傳 404
                    NotFoundException => StatusCodes.Status404NotFound,

                    // 如果是 ForbiddenException → 回傳 403
                    ForbiddenException => StatusCodes.Status403Forbidden,

                    // 如果是 ScheduleConflictException → 回傳 409
                    ScheduleConflictException => StatusCodes.Status409Conflict,

                    // 其它沒特別分類的 BusinessException → 一律 400 Bad Request
                    _ => StatusCodes.Status400BadRequest
                };

                await HandleException(context, ex, statusCode);
            }
            // 第二層：抓「所有沒預期到的一般例外」
            catch (Exception ex)
            {
                // 這代表你沒自己處理、也不是 BusinessException
                // 統一當成 500 內部錯誤
                await HandleException(context, ex, StatusCodes.Status500InternalServerError);
            }
        }

        // 共用的「寫錯誤回應」方法
        private static Task HandleException(HttpContext ctx, Exception ex, int statusCode)
        {
            // 設定 HTTP 狀態碼（404/403/409/400/500）
            ctx.Response.StatusCode = statusCode;

            // 告訴前端：我回的是 JSON
            ctx.Response.ContentType = "application/json";

            // 把錯誤資訊以 JSON 格式寫回去
            // ex.Message 會是你在 service 丟例外時填的 message
            return ctx.Response.WriteAsJsonAsync(new
            {
                error = ex.Message, // 錯誤訊息
                status = statusCode // 狀態碼（方便前端解析）
            });
        }
    }

    }
