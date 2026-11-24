document.addEventListener("DOMContentLoaded", () => {
    document.getElementById("login-form")?.addEventListener("submit", loginUser);
    document.getElementById("register-form")?.addEventListener("submit", registerUser);
});

function showAuthMessage(element, message, type) {
    element.className = `auth-result alert alert-${type}`;
    element.textContent = message;
}

async function loginUser(event) {
    event.preventDefault();

    const login = document.getElementById("login_login").value.trim();
    const password = document.getElementById("login_password").value;
    const result = document.getElementById("login_result");

    const response = await apiRequest("/api/auth/login", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ login, password })
    });

    if (!response.ok) {
        showAuthMessage(result, "Не удалось выполнить вход. Проверьте логин и пароль.", "danger");
        return;
    }

    // Password and password hash are never stored in browser storage.
    sessionStorage.setItem("currentUser", JSON.stringify(response.data));
    showAuthMessage(result, "Вход выполнен.", "success");
    setTimeout(() => window.location.href = "/index.html", 400);
}

async function registerUser(event) {
    event.preventDefault();

    const email = document.getElementById("register_email").value.trim();
    const login = document.getElementById("register_login").value.trim();
    const password = document.getElementById("register_password").value;
    const result = document.getElementById("register_result");

    const response = await apiRequest("/api/auth/register", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ email, login, password })
    });

    if (!response.ok) {
        const message = typeof response.data === "string"
            ? response.data
            : "Не удалось создать аккаунт. Проверьте введённые данные.";
        showAuthMessage(result, message, "danger");
        return;
    }

    showAuthMessage(result, "Регистрация выполнена. Перенаправляем на страницу входа...", "success");
    setTimeout(() => window.location.href = "/login.html", 900);
}
