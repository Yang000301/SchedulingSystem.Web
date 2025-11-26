console.log('[CalendarApp] LIVE MODE loaded');

const API_BASE = 'https://localhost:7168';

export default {
    props: {
        isAuth: { type: Boolean, required: true },
        token: { type: String, required: false },
        userId: { type: Number, required: false },
        userName: { type: String, required: false }
    },

    data() {
        const today = new Date();
        return {
            year: today.getFullYear(),
            month: today.getMonth() + 1,
            weeks: [],
            loading: false,
            error: '',
            selectedDay: null,
            todayStr: today.toISOString().substring(0, 10),

            // key = yyyy-MM-dd → [{ id, name }]
            schedulesByDate: {},
            totalSchedulesCount: 0, // 本月排班筆數（6 天條件用）

            shiftTypes: [
                { id: 1, name: '早班' },
                { id: 2, name: '中班' },
                { id: 3, name: '晚班' }
            ],
            selectedShiftTypeId: 1,
            actionError: ''
        };
    },

    created() {
        console.log('[CalendarApp] init, isAuth:', this.isAuth, 'userId:', this.userId);
        // ✅ 不管有沒有登入，一律先載純月曆
        this.loadMonth();
    },

    methods: {
        authHeaders(extra = {}) {
            const headers = { accept: 'application/json', ...extra };
            if (this.token) {
                headers.Authorization = `Bearer ${this.token}`;
            }
            return headers;
        },

        async loadMonth() {
            this.loading = true;
            this.error = '';
            this.weeks = [];
            this.schedulesByDate = {};
            this.totalSchedulesCount = 0;

            try {
                const qs = new URLSearchParams({
                    year: String(this.year),
                    month: String(this.month)
                });

                // ✅ 月曆 API：沒登入也打
                const url = `${API_BASE}/api/Calendar?${qs.toString()}`;
                console.log('[Calendar] fetch:', url);

                const res = await fetch(url, {
                    headers: this.authHeaders()
                });
                if (!res.ok) throw new Error('Calendar API ' + res.status);

                const days = await res.json();
                this.buildWeeks(days);

                // ✅ 有登入再去載當月個人排班
                if (this.isAuth && this.userId) {
                    await this.loadSchedules();
                } else {
                    console.log('[Calendar] 未登入，只顯示純月曆');
                }
            } catch (err) {
                console.error(err);
                this.error = '載入月曆失敗';
            } finally {
                this.loading = false;
            }
        },

        buildWeeks(days) {
            if (!Array.isArray(days) || !days.length) {
                this.weeks = [];
                return;
            }

            days.sort((a, b) => new Date(a.date) - new Date(b.date));

            const cells = [];
            const firstDow = days[0].dayOfWeek; // 0=日

            for (let i = 0; i < firstDow; i++) {
                cells.push(null);
            }

            for (const d of days) {
                const dt = new Date(d.date);
                const key = dt.toISOString().substring(0, 10);
                cells.push({
                    ...d,
                    key,
                    dayNumber: dt.getDate()
                });
            }

            const weeks = [];
            for (let i = 0; i < cells.length; i += 7) {
                weeks.push(cells.slice(i, i + 7));
            }
            this.weeks = weeks;
        },

        async loadSchedules() {
            if (!this.isAuth || !this.userId) return;

            try {
                const qs = new URLSearchParams({
                    year: String(this.year),
                    month: String(this.month)
                });

                const url = `${API_BASE}/api/Employees/${this.userId}/schedules?${qs.toString()}`;
                console.log('[Schedules] fetch:', url);

                const res = await fetch(url, {
                    headers: this.authHeaders()
                });

                if (!res.ok) {
                    console.warn('[Schedules] status', res.status);
                    return;
                }

                const list = await res.json();
                console.log('[Schedules] raw list:', list);

                // ✅ 這個月總共有幾筆排班（題目那條「不能少於 6 天」用）
                this.totalSchedulesCount = Array.isArray(list) ? list.length : 0;

                const map = {};
                for (const s of list) {
                    const dt = new Date(s.workDate);
                    const key = dt.toISOString().substring(0, 10);
                    if (!map[key]) map[key] = [];
                    map[key].push({
                        id: s.id,
                        name: `班別 ${s.shiftTypeId}`
                    });
                }

                this.schedulesByDate = map;
            } catch (err) {
                console.error('[Schedules] error', err);
            }
        },

        changeMonth(delta) {
            let m = this.month + delta;
            let y = this.year;
            if (m <= 0) { m = 12; y--; }
            else if (m >= 13) { m = 1; y++; }
            this.year = y;
            this.month = m;
            this.loadMonth();
        },

        onCellClick(cell) {
            if (!cell) return;
            this.selectedDay = cell;
            this.actionError = '';
        },

        isHoliday(cell) {
            return cell && cell.holidayType && cell.holidayType !== 'Normal';
        },

        isWeekend(cell) {
            return cell && cell.isWeekend;
        },

        async createSchedule() {
            this.actionError = '';

            if (!this.selectedDay) {
                this.actionError = '請先點選日期';
                return;
            }

            if (!this.isAuth || !this.userId) {
                this.actionError = '請先登入後再排班';
                return;
            }

            // ✅ 未滿 6 天：只提醒，不擋新增
            if (this.totalSchedulesCount < 6) {
                this.actionError = `目前本月僅排 ${this.totalSchedulesCount} 天，請儘快排滿至少 6 天。（仍可新增排班）`;
            }

            try {
                const url = `${API_BASE}/api/Schedules`;
                const payload = {
                    userId: this.userId,
                    shiftTypeId: this.selectedShiftTypeId,
                    workDate: this.selectedDay.date
                };

                console.log('[createSchedule] payload:', payload);

                const res = await fetch(url, {
                    method: 'POST',
                    headers: this.authHeaders({
                        'content-type': 'application/json'
                    }),
                    body: JSON.stringify(payload)
                });

                if (!res.ok) {
                    let msg = '排班失敗';
                    try {
                        const errBody = await res.json();
                        msg = errBody.message || errBody.error || msg;
                    } catch { }
                    this.actionError = msg;
                    return;
                }

                await this.loadSchedules();

                // 重新依照最新天數更新提醒
                if (this.totalSchedulesCount < 6) {
                    this.actionError = `目前本月僅排 ${this.totalSchedulesCount} 天，請儘快排滿至少 6 天。（仍可新增排班）`;
                } else {
                    this.actionError = '';
                }
            } catch (err) {
                console.error('[createSchedule] error', err);
                this.actionError = '系統錯誤';
            }
        },

        async deleteSchedule(id) {
            this.actionError = '';

            if (!this.isAuth || !this.userId) {
                this.actionError = '請先登入後才能刪除排班';
                return;
            }

            // ✅ 題目：不能少於 6 天 → 刪除要擋，避免刪到 6 以下
            if (this.totalSchedulesCount <= 6) {
                this.actionError = `本月排班天數不得少於 6 天，目前為 ${this.totalSchedulesCount} 天，無法再刪除排班`;
                return;
            }

            if (!confirm('確定要刪除這筆排班嗎？')) return;

            try {
                const url = `${API_BASE}/api/Schedules/${id}`;
                const payload = {
                    requestUserId: this.userId,
                    requestUserRole: 'Boss'
                };

                console.log('[deleteSchedule] DELETE', url, payload);

                const res = await fetch(url, {
                    method: 'DELETE',
                    headers: this.authHeaders({
                        'content-type': 'application/json'
                    }),
                    body: JSON.stringify(payload)
                });

                if (!res.ok) {
                    let msg = `刪除失敗 (HTTP ${res.status})`;
                    try {
                        const text = await res.text();
                        console.log('[deleteSchedule] error body:', text);
                        try {
                            const errBody = JSON.parse(text);
                            msg = errBody.message || errBody.error || msg;
                        } catch { }
                    } catch { }

                    this.actionError = msg;
                    return;
                }

                await this.loadSchedules();

                if (this.totalSchedulesCount < 6) {
                    this.actionError = `目前本月僅排 ${this.totalSchedulesCount} 天，系統已鎖定刪除功能。`;
                } else {
                    this.actionError = '';
                }
            } catch (err) {
                console.error('[deleteSchedule] error', err);
                this.actionError = '系統錯誤';
            }
        }
    },

    template: `
<div class="my-calendar">
  <div class="calendar-card card">

    <div class="card-header d-flex justify-content-between align-items-center">
      <button class="btn btn-sm btn-outline-secondary" @click="changeMonth(-1)">◀</button>
      <div class="fw-semibold">{{ year }} 年 {{ month }} 月</div>
      <button class="btn btn-sm btn-outline-secondary" @click="changeMonth(1)">▶</button>
    </div>

    <div class="card-body p-2 p-md-3">

      <!-- 登入資訊 -->
      <div class="mb-2 small" v-if="isAuth && userId">
        👤 登入中：{{ userName || '使用者' }}（ID {{ userId }}）
        <span class="ms-2 text-muted">本月 {{ totalSchedulesCount }} 天班</span>
      </div>

      <div v-if="loading" class="text-muted mb-2">載入中…</div>
      <div v-if="error" class="text-danger mb-2">{{ error }}</div>

      <!-- 月曆本體：永遠顯示 -->
      <div class="calendar-grid" v-if="weeks.length">

        <div class="calendar-header">日</div>
        <div class="calendar-header">一</div>
        <div class="calendar-header">二</div>
        <div class="calendar-header">三</div>
        <div class="calendar-header">四</div>
        <div class="calendar-header">五</div>
        <div class="calendar-header">六</div>

        <template v-for="(week, wi) in weeks" :key="wi">
          <div v-for="(cell, di) in week"
               :key="cell ? cell.key : 'blank-'+wi+'-'+di"
               class="calendar-cell"
               :class="{
                 'cell-blank': !cell,
                 'cell-weekend': isWeekend(cell),
                 'cell-holiday': isHoliday(cell)
               }"
               @click="onCellClick(cell)">

            <div v-if="!cell">&nbsp;</div>

            <div v-else>
              <div class="day-number">{{ cell.dayNumber }}</div>

              <!-- 當天排班列表（有登入時會載資料） -->
              <ul v-if="schedulesByDate[cell.key]" class="cell-shifts">
                <li v-for="sch in schedulesByDate[cell.key]" :key="sch.id">
                  {{ sch.name }}
                  <!-- totalSchedulesCount > 6 才顯示刪除按鈕 -->
                  <button v-if="totalSchedulesCount > 6"
                          class="btn btn-xs btn-link text-danger"
                          style="font-size:0.7rem"
                          @click.stop="deleteSchedule(sch.id)">
                    刪除
                  </button>
                  <span v-else class="text-muted" style="font-size:0.7rem">
                    （已接近最低 6 天，無法刪除）
                  </span>
                </li>
              </ul>

              <div v-if="isHoliday(cell)" class="holiday-name">
                {{ cell.holidayName || '假日' }}
              </div>
            </div>

          </div>
        </template>

      </div>

      <!-- 下方互動區：點到某一天才出現 -->
      <div v-if="selectedDay" class="mt-3 border-top pt-2 small">

        <div class="mb-1">
          <strong>選取日期：</strong>
          {{ selectedDay.date.substring(0,10) }}
        </div>

        <!-- 顯示當天排班列表（上面那一段已經有刪除按鈕） -->
        <div class="mb-2">
          <div v-if="schedulesByDate[selectedDay.key] && schedulesByDate[selectedDay.key].length">
            <div class="mb-1">當天排班：</div>
            <ul class="cell-shifts">
              <li v-for="sch in schedulesByDate[selectedDay.key]" :key="sch.id">
                {{ sch.name }}
              </li>
            </ul>
          </div>
          <div v-else class="text-muted">
            目前這天還沒有排班。
          </div>
        </div>

        <!-- 排班操作區：登入即可新增 -->
        <template v-if="isAuth && userId">
          <div class="mb-2 d-flex align-items-center gap-2">
            <label class="me-2 mb-0">班別：</label>
            <select class="form-select form-select-sm"
                    v-model.number="selectedShiftTypeId"
                    style="max-width: 160px;">
              <option v-for="st in shiftTypes" :key="st.id" :value="st.id">
                {{ st.name }}
              </option>
            </select>

            <button class="btn btn-sm btn-primary" @click="createSchedule">
              新增排班
            </button>
          </div>

          <div v-if="totalSchedulesCount < 6" class="text-warning">
            目前本月僅排 {{ totalSchedulesCount }} 天班，請至少排滿 6 天。（仍可新增）
          </div>
          <div v-else class="text-muted">
            本月已排 {{ totalSchedulesCount }} 天班，刪除時系統會避免低於 6 天。
          </div>
        </template>

        <template v-else>
          <div class="text-warning">
            請先登入後才能使用排班功能。
          </div>
        </template>

        <div v-if="actionError" class="text-danger mt-1">
          {{ actionError }}
        </div>
      </div>

    </div>
  </div>
</div>
`
};
