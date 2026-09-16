async function createUser() {
    user =
    {
        login: "testUser111",
        email: "test@email.com111",
        password: "12341234"
    };

    try {
        const response = await fetch("/api/user", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(user)
        });

        if (!response.ok) {
            console.log("Ошибка : ", response.status);
            return;
        }
        const result = await response.json();
        console.log("Пользователь создан");
        console.log(result);
        return result;
    }
    catch (error) {
        console.log("Ошибка : ", error);
    }
}

createUser();