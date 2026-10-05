document.addEventListener("DOMContentLoaded", function () {
    const loginForm = document.getElementById("login-form");
    const registerForm =
        document.getElementById("register-form");
    if (loginForm) {
        loginForm.addEventListener(
            "submit",
            loginUser
        );
    }
    if (registerForm) {
        registerForm.addEventListener(
            "submit",
            registerUser
        );
    }
});
async function loginUser(event) {
    event.preventDefault();
    const login =
        document.getElementById("login_login").value;
    const password =
        document.getElementById("login_password").value;
    const result =
        document.getElementById("login_result");
    const request =
    {
        login: login,
        password: password
    };
    const response = await apiRequest(
        "/api/auth/login",
        {
            method: "POST",
            headers:
            {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(request)
        }
    );
    if (!response.ok) {
        result.innerHTML = `
            <div class="alert alert-danger">
                Не удалось выполнить вход.
                <br>
                ${response.data}
            </div>
        `;

        return;
    }

    sessionStorage.setItem(
        "currentUser",
        JSON.stringify(response.data)
    );


    result.innerHTML = `
        <div class="alert alert-success">
            Вход выполнен.
        </div>
    `;


    setTimeout(function () {
        window.location.href = "/index.html";
    }, 500);
}
async function registerUser(event) {
    event.preventDefault();


    const email =
        document.getElementById("register_email").value;

    const login =
        document.getElementById("register_login").value;

    const password =
        document.getElementById("register_password").value;


    const result =
        document.getElementById("register_result");


    const request =
    {
        email: email,
        login: login,
        password: password
    };


    const response = await apiRequest(
        "/api/auth/register",
        {
            method: "POST",

            headers:
            {
                "Content-Type": "application/json"
            },

            body: JSON.stringify(request)
        }
    );


    if (!response.ok) {
        result.innerHTML = `
            <div class="alert alert-danger">
                Ошибка регистрации.
                <br>
                ${response.data}
            </div>
        `;

        return;
    }


    result.innerHTML = `
        <div class="alert alert-success">
            Регистрация выполнена успешно.
            Перенаправление на страницу входа...
        </div>
    `;


    setTimeout(function () {
        window.location.href = "/login.html";
    }, 1000);
}