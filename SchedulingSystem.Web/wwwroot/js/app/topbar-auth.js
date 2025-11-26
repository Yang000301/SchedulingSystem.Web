// wwwroot/js/app/topbar-auth.js

// topbar-auth.js
const userSpan = document.getElementById('topbar-user');
const logoutBtn = document.getElementById('btn-logout');
const loginLink = document.getElementById('btn-login');

if (userSpan && logoutBtn && loginLink) {

    const name =
        localStorage.getItem('userName') ||
        localStorage.getItem('displayName') ||
        localStorage.getItem('userAccount') ||
        '';

    const hasJwt = !!localStorage.getItem('jwt');

    if (hasJwt && name) {
        // 已登入
        userSpan.textContent = `登入中：${name}`;

        loginLink.style.display = 'none';
        logoutBtn.style.display = 'inline-block';
    } else {
        // 未登入
        userSpan.textContent = '尚未登入';

        loginLink.style.display = 'inline-block';
        logoutBtn.style.display = 'none';
    }

    logoutBtn.addEventListener('click', () => {
        localStorage.clear();
        window.location.href = '/Auth/Login';
    });
}
