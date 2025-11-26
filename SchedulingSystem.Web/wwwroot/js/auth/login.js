// wwwroot/js/auth/login.js

const API_BASE_URL = "https://localhost:7168";  // Swagger 的 host + port

const form = document.getElementById('login-form');
const errorBox = document.getElementById('login-error');

if (form) {
    form.addEventListener('submit', async (e) => {
        e.preventDefault();

        const usernameInput = document.getElementById('username');
        const passwordInput = document.getElementById('password');

        const username = usernameInput.value.trim();
        const password = passwordInput.value;

        if (!username || !password) {
            errorBox.textContent = '請輸入帳號與密碼';
            errorBox.style.display = 'block';
            return;
        }

        errorBox.style.display = 'none';
        errorBox.textContent = '';

        try {
            const url = API_BASE_URL + "/api/auth/login";
            console.log('[LOGIN] sending request to ' + url);

            const res = await fetch(url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'accept': 'application/json'
                },
                body: JSON.stringify({
                    username: username,
                    password: password
                })
            });

            console.log('[LOGIN] status = ' + res.status);

            if (!res.ok) {
                let msg = '登入失敗';
                try {
                    const errData = await res.json();
                    console.log('[LOGIN] error body =', errData);
                    if (errData && errData.message) {
                        msg = errData.message;
                    }
                } catch (err) {
                    console.warn('[LOGIN] cannot parse error json', err);
                }

                errorBox.textContent = msg;
                errorBox.style.display = 'block';
                return;
            }

            const data = await res.json();
            console.log('[LOGIN] success data =', data);

            // 你的 JSON 長這樣：{ token, user: { id, displayName, role } }
            const user = data.user || {};

            localStorage.setItem('jwt', data.token || '');
            localStorage.setItem('userId', user.id || '');
            localStorage.setItem('userAccount', user.username || '');
            localStorage.setItem('userName', user.displayName || user.username || '');
            localStorage.setItem('role', user.role || '');

            // 導到班表頁
            window.location.href = '/ Home / Index';
        } catch (err) {
            console.error('[LOGIN] fetch error =', err);
            errorBox.textContent = '系統錯誤（請稍後再試）';
            errorBox.style.display = 'block';
        }
    });
}
