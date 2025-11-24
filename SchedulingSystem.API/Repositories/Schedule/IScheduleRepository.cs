using SchedulingSystem.API.Models;

namespace SchedulingSystem.API.Repositories.ScheduleRepos
{
    public interface IScheduleRepository
    {
        // ➤ 是否同一天同一使用者已排班
        Task<bool> ExistsAsync(int userId, DateTime workDate);

        // ➤ 取得單筆排班
        Task<Schedule?> GetByIdAsync(int id);

        // ➤ 建立排班
        Task<Schedule> CreateAsync(Schedule schedule);

        // 🔥 ➤ 新增：當天已排幾個人（用於最多 2 人上限）
        Task<int> CountByDateAsync(DateTime workDate);

        // 🔥 ➤ 新增：某個員工這個月排了幾天（用於最多 15 天上限）
        Task<int> CountByEmployeeAndMonthAsync(int userId, DateTime anyDateInMonth);

        //刪除 排班
        Task DeleteAsync(Schedule schedule);
        //查某人班表
        Task<List<Schedule>> GetByEmployeeAndMonthAsync(int userId, int year, int month);
        //排行榜 用
        Task<List<Schedule>> GetByMonthAsync(int year, int month);
        }
       
}
