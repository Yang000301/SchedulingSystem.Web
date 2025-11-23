using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using SchedulingSystem.API.Data;   // 放 AppDbContext 的 namespace
using SchedulingSystem.API.Models; // 放 User entity 的 namespace


namespace SchedulingSystem.API.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class DebugController : ControllerBase
    {
        private readonly AppDbContext _db;

        public DebugController(AppDbContext db)
        {
            _db = db;
        }

        // GET: /api/debug/users
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            // 1) 試著從 DB 抓前 10 筆 user
            var users = await _db.Users
                .OrderBy(u => u.Id)
                .Take(10)
                .ToListAsync();

            // 2) 如果沒有資料，也至少知道不會爆錯（代表有連線到 DB）
            return Ok(new
            {
                Count = users.Count,
                Items = users
            });
        }
    }
}
