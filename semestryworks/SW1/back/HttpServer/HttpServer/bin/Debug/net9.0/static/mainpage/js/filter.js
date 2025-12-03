const maxCostInput = document.getElementById('max-cost');
const tourList = document.getElementById('cards');
const sortBySelect = document.getElementById('sort-by');
const tagCheckboxes = document.querySelectorAll('input[name="tag"]');

function getCurrentFilters() {
    const maxCost = maxCostInput?.value || '';
    const tags = Array.from(tagCheckboxes)
        .filter(cb => cb.checked)
        .map(cb => cb.value);
    const sortBy = sortBySelect?.value || 'price'; // значение по умолчанию

    return { maxCost, tags, sortBy };
}

function updateCount() {
    const count = document.querySelectorAll('.card').length - 3;
    document.getElementById('count').textContent = count;
}

async function loadTours() {
    const { maxCost, tags, sortBy } = getCurrentFilters();

    // Собираем URL-параметры
    const params = new URLSearchParams();
    if (maxCost) params.set('maxCost', maxCost);
    if (tags.length > 0) params.set('tags', tags.join(',')); // например: tags=Feriados,Lazer
    if (sortBy) params.set('sort', sortBy);

    const url = `/turismo/filter?${params.toString()}`;

    try {
        tourList.innerHTML = "loading...";
        const response = await fetch(url);
        if (!response.ok) throw new Error('Сервер вернул ошибку');
        const html = await response.text();
        tourList.innerHTML = html;
    } catch (err) {
        tourList.innerHTML = '<p>Не удалось загрузить туры.</p>';
        console.error('AJAX ошибка:', err);
    }
    updateCount();
}

// Обновляет URL и загружает данные
function updateUrlAndLoad() {
    const { maxCost, tags, sortBy } = getCurrentFilters();

    const params = new URLSearchParams();
    if (maxCost) params.set('maxCost', maxCost);
    if (tags.length > 0) params.set('tags', tags.join(','));
    if (sortBy) params.set('sort', sortBy);

    const newUrl = `${location.pathname}?${params.toString()}`;
    window.history.replaceState(null, '', newUrl);
    loadTours();
}

// Инициализация: прочитать maxCost из URL и применить
function initFiltersFromUrl() {
    const urlParams = new URLSearchParams(window.location.search);

    // maxCost
    if (maxCostInput) {
        maxCostInput.value = urlParams.get('maxCost') || '';
    }

    // tags
    const tagsParam = urlParams.get('tags');
    const selectedTags = tagsParam ? tagsParam.split(',') : [];
    tagCheckboxes.forEach(cb => {
        cb.checked = selectedTags.includes(cb.value);
    });

    // sort
    if (sortBySelect) {
        const sortFromUrl = urlParams.get('sort');
        if (sortFromUrl === 'price' || sortFromUrl === 'date') {
            sortBySelect.value = sortFromUrl;
        } else {
            sortBySelect.value = 'price'; // значение по умолчанию
        }
    }
}

// Подписка на события
document.addEventListener('DOMContentLoaded', () => {
    initFiltersFromUrl();
    loadTours();

    tagCheckboxes.forEach(cb => {
        cb.addEventListener('change', updateUrlAndLoad);
    });

    if (maxCostInput) {
        maxCostInput.addEventListener('input', updateUrlAndLoad);
    }
    if (sortBySelect) {
        sortBySelect.addEventListener('change', updateUrlAndLoad);
    }
});

document.querySelectorAll(".filter-header").forEach(header => {
    header.addEventListener("click", () => {
        header.parentElement.classList.toggle("active");
    });
});

const start = document.getElementById("start-date");
const end = document.getElementById("end-date");

start.addEventListener("change", () => {
    end.min = start.value;
});


