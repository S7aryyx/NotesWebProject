const API_URL = "/api";

async function apiRequest(url, options = {}) {
    try {
        const response = await fetch(url, options);

        const text = await response.text();

        let data = null;

        if (text !== "") {
            try {
                data = JSON.parse(text);
            }
            catch {
                data = text;
            }
        }

        return {
            ok: response.ok,
            status: response.status,
            data: data
        };
    }
    catch (error) {
        console.error(error);

        return {
            ok: false,
            status: 0,
            data: "Ошибка соединения с сервером"
        };
    }
}