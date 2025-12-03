const editableSelectors = [
    ".tour-header .title",
    ".tour-body p",
    ".more-info",
    ".offer",
    ".tour-price-type",
    ".tour-price-installment",
    ".tour-price-single",
    ".payment-section .text"
];

document.getElementById("editTourBtn").addEventListener("click", () => {
    enableEditMode();
});

function enableEditMode() {
    // Делаем редактируемыми
    editableSelectors.forEach(sel => {
        document.querySelectorAll(sel).forEach(el => {
            el.setAttribute("contenteditable", "true");
            el.style.outline = "2px solid #ff9800";
            el.style.padding = "3px";
        });
    });

    // Появляется панель «Сохранить / Отмена»
    showSavePanel();
}

function showSavePanel() {
    const panel = document.createElement("div");
    panel.className = "save-panel";
    panel.innerHTML = `
        <button class="save-btn">Сохранить</button>
        <button class="cancel-btn">Отмена</button>
    `;
    document.body.appendChild(panel);

    panel.querySelector(".save-btn").onclick = saveChanges;
    panel.querySelector(".cancel-btn").onclick = cancelEdit;
}

function collectData() {
    let data = {};

    editableSelectors.forEach(sel => {
        let elements = document.querySelectorAll(sel);
        data[sel] = [];

        elements.forEach(el => data[sel].push(el.innerHTML));
    });

    return data;
}

function saveChanges() {
    const data = collectData();
    const tourId = document.getElementById("editTourBtn").dataset.tourId;

    fetch(`/admin/save-tour?id=${tourId}`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(data)
    })
    .then(r => r.text())
    .then(_ => location.reload());
}

function cancelEdit() {
    location.reload(); // просто перезагружаем
}