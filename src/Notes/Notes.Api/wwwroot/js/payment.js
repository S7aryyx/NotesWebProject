(() => {
    const statusElement = document.getElementById("access-status");
    const expiryElement = document.getElementById("access-expiry");
    const messageElement = document.getElementById("payment-message");

    async function loadStatus() {
        try {
            const response = await fetch("/api/billing/status");
            if (response.status === 401) {
                window.location.href = "/login.html";
                return;
            }
            if (!response.ok) throw new Error("Не удалось получить статус доступа.");
            const data = await response.json();
            statusElement.textContent = data.hasLifetimeAccess
                ? "Постоянный доступ активен"
                : data.hasAiAccess ? "Подписка активна" : "Доступ не оплачен";
            expiryElement.textContent = data.subscriptionUntil && !data.hasLifetimeAccess
                ? `Действует до: ${new Date(data.subscriptionUntil).toLocaleString("ru-RU")}`
                : "";
        } catch (error) {
            statusElement.textContent = error.message;
        }
    }

    async function createPayment(plan) {
        messageElement.textContent = "Создаём платёж...";
        document.querySelectorAll("[data-plan]").forEach(button => button.disabled = true);
        const paymentWindow = window.open("about:blank", "_blank");
        try {
            const response = await fetch("/api/billing/create", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ plan })
            });
            const data = await response.json();
            if (response.status === 401) {
                if (paymentWindow) paymentWindow.close();
                window.location.href = "/login.html";
                return;
            }
            if (!response.ok) throw new Error(data.message || data.title || "Не удалось создать платёж.");
            if (!data.confirmationUrl) throw new Error("Платёжный сервис не вернул ссылку на оплату.");
            if (paymentWindow) paymentWindow.location.href = data.confirmationUrl;
            else window.location.href = data.confirmationUrl;
            messageElement.textContent = "После оплаты вернитесь на эту страницу и обновите статус.";
        } catch (error) {
            if (paymentWindow) paymentWindow.close();
            messageElement.textContent = error.message;
        } finally {
            document.querySelectorAll("[data-plan]").forEach(button => button.disabled = false);
        }
    }

    document.querySelectorAll("[data-plan]").forEach(button =>
        button.addEventListener("click", () => createPayment(button.dataset.plan)));
    document.getElementById("refresh-access").addEventListener("click", loadStatus);
    loadStatus();
    const params = new URLSearchParams(location.search);
    if (params.get("payment") === "return") {
        messageElement.textContent = "Платёж обрабатывается. Статус обновится после подтверждения ЮKassa.";
    }
})();