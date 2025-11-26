(() => {

    const API_BASE = "https://localhost:7168";

    const btn = document.getElementById('btn-load-leaderboard');
    const monthInput = document.getElementById('leaderboard-month');
    const tableBody = document.querySelector('#leaderboard-table tbody');
    const emptyBox = document.getElementById('leaderboard-empty');

    if (!btn || !monthInput || !tableBody) return;

    btn.addEventListener('click', loadLeaderboard);

    function getSelectedYearMonth() {
        if (!monthInput.value) return null;
        const [year, month] = monthInput.value.split('-');
        return { year, month };
    }

    async function loadLeaderboard() {
        const ym = getSelectedYearMonth();
        if (!ym) {
            alert('請先選擇年月');
            return;
        }

        const jwt = localStorage.getItem('jwt');
        if (!jwt) {
            alert('尚未登入');
            return;
        }

        const url =
            `${API_BASE}/api/Stats/leaderboard?year=${ym.year}&month=${ym.month}`;

        tableBody.innerHTML = '';
        emptyBox.classList.add('d-none');

        try {
            const res = await fetch(url, {
                headers: {
                    'Authorization': `Bearer ${jwt}`,
                    'accept': 'application/json'
                }
            });

            if (!res.ok) {
                alert('排行榜讀取失敗');
                return;
            }

            const data = await res.json();

            if (!data || data.length === 0) {
                emptyBox.classList.remove('d-none');
                return;
            }

            render(data);

        } catch (err) {
            console.error('[LEADERBOARD] error', err);
            alert('系統錯誤');
        }
    }

    function render(list) {
        tableBody.innerHTML = '';

        list.forEach((item, index) => {
            const tr = document.createElement('tr');

            let rankClass = '';
            if (index === 0) rankClass = 'rank-1';
            if (index === 1) rankClass = 'rank-2';
            if (index === 2) rankClass = 'rank-3';

            tr.className = rankClass;

            tr.innerHTML = `
                <td class="rank-num">${index + 1}</td>
                <td>${item.displayName}</td>
                <td class="shift-count">${item.totalShifts}</td>
            `;

            tableBody.appendChild(tr);
        });
    }

})();
