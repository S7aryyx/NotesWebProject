document.addEventListener("DOMContentLoaded", function () {

    const registerForm = document.getElementById("registerForm");

    if (registerForm) {
        registerForm.addEventListener("submit", register);
    }

    const loginForm = document.getElementById("loginForm");

    if (loginForm) {
        loginForm.addEventListener("submit", login);
    }
});


async function register(event) {

    event.preventDefault();

    const email = document.getElementById("email").value;
    const login = document.getElementById("login").value;
    const password = document.getElementById("password").value;

    const message = document.getElementById("registerMessage");

    const user = {
        email: email,
        login: login,
        password: password
    };

    try {

        const response = await fetch("/api/auth/register", {
            method: "POST",

            headers: {
                "Content-Type": "application/json"
            },

            body: JSON.stringify(user)
        });

        if (!response.ok) {

            const error = await response.text();

            message.textContent = error;

            return;
        }

        const result = await response.json();

        message.textContent = "Аккаунт успешно создан.";

        console.log("Created user:", result);

        setTimeout(function () {
            window.location.href = "/pages/login.html";
        }, 800);

    }
    catch (error) {

        console.error(error);

        message.textContent =
            "Не удалось подключиться к серверу.";
    }
}


async function login(event) {

    event.preventDefault();

    const login = document.getElementById("login").value;
    const password = document.getElementById("password").value;

    const message = document.getElementById("loginMessage");

    const request = {
        login: login,
        password: password
    };

    try {

        const response = await fetch("/api/auth/login", {
            method: "POST",

            headers: {
                "Content-Type": "application/json"
            },

            body: JSON.stringify(request)
        });

        if (!response.ok) {

            const error = await response.text();

            message.textContent = error;

            return;
        }

        const user = await response.json();

        /*
         * Временное решение.
         *
         * Позже, после добавления Cookie Authentication,
         * это будет удалено.
         */

        localStorage.setItem(
            "currentUser",
            JSON.stringify(user)
        );

        console.log("Logged in:", user);

        window.location.href = "/pages/index.html";

    }
    catch (error) {

        console.error(error);

        message.textContent =
            "Не удалось подключиться к серверу.";
    }
}


function getCurrentUser() {

    const userJson =
        localStorage.getItem("currentUser");

    if (!userJson) {
        return null;
    }

    return JSON.parse(userJson);
}


function logout() {

    localStorage.removeItem("currentUser");

    window.location.href =
        "/pages/login.html";
}
