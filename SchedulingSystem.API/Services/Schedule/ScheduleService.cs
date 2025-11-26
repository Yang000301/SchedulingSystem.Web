using SchedulingSystem.API.Dtos.Schedule;
using SchedulingSystem.API.Exceptions;
using SchedulingSystem.API.Repositories.ScheduleRepos;
using SchedulingSystem.API.Models;

namespace SchedulingSystem.API.Services.ScheduleServices
{
    public class ScheduleService : IScheduleService
    {
        private readonly IScheduleRepository _repo;

        public ScheduleService(IScheduleRepository repo)
        {
            _repo = repo;
        }

        /// <summary>
        /// 建立排班（POST /api/schedules）
        /// 這裡是排班所有規則的「總大門」，該擋的一律在這裡擋掉。
        /// 規則：
        /// 1. 只能排「下個月」
        /// 2. 假日 / 週末不能排
        /// 3. 同一個人、同一天不可重複排
        /// 4. 同一天最多 2 個人
        /// 5. 每個人每月最多 15 天
        /// （至少 6 天是「考核/統計」規則，不在這裡擋，之後查報表時再用）
        /// </summary>
        public async Task<ScheduleResponse> CreateAsync(CreateScheduleRequest req)
        {
            // 0) 先把日期只看「年月」，避免時區或時間影響（月判斷用）
            var workDate = req.WorkDate.Date;
            var today = DateTime.Today;
            var firstDayThisMonth = new DateTime(today.Year, today.Month, 1);
            var firstDayNextMonth = firstDayThisMonth.AddMonths(1);
            var workMonth = new DateTime(workDate.Year, workDate.Month, 1);

            // 1) 只能排「下個月」
            //    例如今天是 2025-11-23 → 只能排 2025-12 整個月
            if (workMonth != firstDayNextMonth)
            {
                throw new BusinessException("只允許排『下個月』的班別。");
            }

            // 2) 假日 / 週末不能排班
            //    這裡先用「週六日 = 例假日」示範，
            //    未來要接「國定假日 / 補假」可以改這個方法。
            if (IsHolidayOrWeekend(workDate))
            {
                throw new BusinessException("假日不能排班。");
            }

            // 3) 同一個人、同一天不可重複排班
            var exists = await _repo.ExistsAsync(req.UserId, workDate);
            if (exists)
            {
                // 這種是「業務上的衝突」，用你自訂的 ScheduleConflictException
                throw new ScheduleConflictException("你這一天已經有排班了。");
            }

            // 4) 同一天最多 2 位員工
            //    這邊需要 Repository 提供 CountByDateAsync()
            var countToday = await _repo.CountByDateAsync(workDate);
            if (countToday >= 2)
            {
                throw new ScheduleConflictException("這一天的排班人數已滿（最多 2 人）。");
            }

            // 5) 每個員工每月最多 15 天
            //    對應規則：「一個人每月不能排超過 15 天」
            var monthCount = await _repo.CountByEmployeeAndMonthAsync(req.UserId, workDate);
            if (monthCount >= 15)
            {
                throw new BusinessException("這個月你的排班天數已達上限 15 天。");
            }

            // ⚠「每月至少 6 天」不在這裡擋：
            //    這種是「你整月太少，要提醒或考核」，
            //    比較適合放在統計 / 報表 API（例如 /api/stats/leaderboard）
            //    而不是在單筆建立時就阻止。

            // 6) 通過所有檢查 → 建立排班實體
            var entity = new Schedule
            {
                UserId = req.UserId,
                ShiftTypeId = req.ShiftTypeId,
                WorkDate = workDate,
                CreatedAt = DateTime.UtcNow
            };

            // 7) 丟給 Repository 寫進資料庫
            entity = await _repo.CreateAsync(entity);

            // 8) 回傳給前端的 DTO（避免把 Entity 全噴出去）
            return new ScheduleResponse
            {
                Id = entity.Id,
                UserId = entity.UserId,
                ShiftTypeId = entity.ShiftTypeId,
                WorkDate = entity.WorkDate
            };
        }

        /// <summary>
        /// 判斷是不是「假日 / 週末」
        /// 現階段先用「週六日 = 假日」。
        /// 未來如果要接「國定假日 / 補班表」，可以：
        /// - 查一張 Holidays 資料表
        /// - 或呼叫外部假日 API
        /// </summary>
        private static bool IsHolidayOrWeekend(DateTime workDate)
        {
            // 週六 / 週日 → 假日
            if (workDate.DayOfWeek == DayOfWeek.Saturday ||
                workDate.DayOfWeek == DayOfWeek.Sunday)
            {
                return true;
            }

            // TODO: 未來可以在這裡加「國定假日 / 補假」判斷
            return false;
        }


        public async Task DeleteAsync(int scheduleId, DeleteScheduleRequest req)
        {
            // 1. 找資料
            var entity = await _repo.GetByIdAsync(scheduleId);
            if (entity == null)
                throw new NotFoundException("這筆排班不存在");

            // 2. 是否可刪？過去的班禁止刪除
            if (entity.WorkDate.Date < DateTime.UtcNow.Date)
                throw new BusinessException("無法刪除已過去的班別");

            // 3. 權限：員工只能刪自己的；老闆可以刪任何人的
            if (req.RequestUserRole == "employee" && entity.UserId != req.RequestUserId)
                throw new ForbiddenException("你不能刪除別人的班表");

            // 4. 正式刪除
            await _repo.DeleteAsync(entity);

        }

        //看某人班表
        public async Task<List<ScheduleResponse>> GetByEmployeeAndMonthAsync(int employeeId, int year, int month)
        {           
            if (year <= 0 || month < 1 || month > 12)
                throw new BusinessException("year / month 格式不正確");

            var entities = await _repo.GetByEmployeeAndMonthAsync(employeeId, year, month);

            // 把 Entity 轉成你對外用的 DTO
            return entities
                .Select(s => new ScheduleResponse
                {
                    Id = s.Id,
                    UserId = s.UserId,
                    ShiftTypeId = s.ShiftTypeId,
                    WorkDate = s.WorkDate
                })
                .ToList();
        }
        //老闆看排行榜

        public async Task<List<ScheduleLeaderboard>> GetMonthlyLeaderboardAsync(int year, int month)
        {
            // 1) 先從 repo 抓「這個月所有人的班表」
            var schedules = await _repo.GetByMonthAsync(year, month);

            // 2) 在 Service 這邊 GroupBy + 統計 + 排序
            var result = schedules
                .GroupBy(s => new { s.UserId, s.User.DisplayName })
                .Select(g => new ScheduleLeaderboard
                {
                    UserId = g.Key.UserId,
                    DisplayName = g.Key.DisplayName,
                    TotalShifts = g.Count()
                })
                .OrderByDescending(x => x.TotalShifts)
                .ToList();

            return result;
        }

        public async Task<List<ScheduleLeaderboard>> GetYearlyLeaderboardAsync(int year)
        {
            // 1) 先從 repo 抓「這一年所有人的班表」
            var schedules = await _repo.GetByYearAsync(year);

            // 2) 一樣 GroupBy + 統計 + 排序
            var result = schedules
                .GroupBy(s => new { s.UserId, s.User.DisplayName })
                .Select(g => new ScheduleLeaderboard
                {
                    UserId = g.Key.UserId,
                    DisplayName = g.Key.DisplayName,
                    TotalShifts = g.Count()
                })
                .OrderByDescending(x => x.TotalShifts)
                .ToList();

            return result;
        }


    }
}
