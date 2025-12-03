document.addEventListener("DOMContentLoaded", () => {
    const showModal = (id) => document.getElementById(id).classList.remove("hidden");
    const hideModal = (id) => document.getElementById(id).classList.add("hidden");

    // Кнопки шапки
    document.getElementById("login").addEventListener("click", () => showModal("loginModal"));

    // Закрытие крестиком
    document.querySelectorAll(".close").forEach(btn => {
        btn.addEventListener("click", () => {
            const modalId = btn.dataset.close;
            if (modalId) hideModal(modalId);
        });
    });

    // Закрытие кликом по фону
    document.querySelectorAll(".modal").forEach(modal => {
        modal.addEventListener("click", e => {
            if (e.target === modal) hideModal(modal.id);
        });
    });

    // Переход между модалками
    document.getElementById("toRegister").addEventListener("click", () => {
        hideModal("loginModal");
        showModal("registerModal");
    });

    document.getElementById("toLogin").addEventListener("click", () => {
        hideModal("registerModal");
        showModal("loginModal");
    });

    // Логин
    document.getElementById("loginSubmit").addEventListener("click", async () => {
        const username = document.getElementById("loginUsername").value;
        const password = document.getElementById("loginPassword").value;

        const res = await fetch("/auth/login", {
            method: "POST",
            headers: {"Content-Type": "application/json"},
            body: JSON.stringify({ username, password })
        });

        if (res.ok) {
            hideModal("loginModal");
            location.reload();
        } else {
            alert("Неверный логин или пароль");
        }
    });

    // Регистрация
    document.getElementById("registerSubmit").addEventListener("click", async () => {
        const username = document.getElementById("regUsername").value;
        const password = document.getElementById("regPassword").value;

        const res = await fetch("/auth/register", {
            method: "POST",
            headers: {"Content-Type": "application/json"},
            body: JSON.stringify({ username, password })
        });

        if (res.ok) {
            hideModal("registerModal");
            alert("Аккаунт создан! Войдите в систему.");
            showModal("loginModal");
        } else {
            alert("Ошибка регистрации или пользователь уже существует.");
        }
    });
});
