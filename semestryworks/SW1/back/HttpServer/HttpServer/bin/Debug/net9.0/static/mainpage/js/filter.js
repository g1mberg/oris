document.addEventListener('DOMContentLoaded', () => {
    const searchInput = document.getElementById('search-input');
    const maxCostInput = document.getElementById('max-cost');
    const originCityInput = document.getElementById('origin-city');
    const destinationInput = document.getElementById('destination');
    const startDateInput = document.getElementById('start-date');
    const endDateInput = document.getElementById('end-date');
    const sortBySelect = document.getElementById('sort-by');
    const tagCheckboxes = document.querySelectorAll('input[name="tag"]');
    const tourList = document.getElementById('cards');
    const resetButton = document.getElementById('reset-filters');

    const today = new Date().toISOString().split('T')[0];

    function getFieldElementByName(name) {
        switch (name) {
            case 'search': return searchInput;
            case 'maxCost': return maxCostInput;
            case 'originCity': return originCityInput;
            case 'destination': return destinationInput;
            case 'startDate': return startDateInput;
            case 'endDate': return endDateInput;
            default: return null;
        }
    }

    function validateField(fieldName, value) {
        switch (fieldName) {
            case 'maxCost':
                return !value || (!isNaN(value) && parseFloat(value) > 0);
            case 'startDate':
            case 'endDate':
                if (!value) return true;
                if (!/^\d{4}-\d{2}-\d{2}$/.test(value)) return false;
                if (value < today) return false;
                return true;
            default:
                return true;
        }
    }


    function getFieldErrorMessage(fieldName) {
        switch (fieldName) {
            case 'maxCost': return 'Цена должна быть числом больше 0';
            case 'startDate':
            case 'endDate': return 'Дата должна быть в формате YYYY-MM-DD и не раньше сегодня';
            case 'search':
            case 'originCity':
            case 'destination': return 'Поле должно содержать минимум 2 символа';
            default: return 'Неверный ввод';
        }
    }

    function showFieldError(field) {
        field.style.borderColor = '#f44336';
        field.style.boxShadow = '0 0 0 2px rgba(244,67,54,0.5)';
        showValidationError(getFieldErrorMessage(field.id || field.name));
    }

    function clearFieldError(field) {
        field.style.borderColor = '';
        field.style.boxShadow = '';
    }

    function showValidationError(message) {
        const errorDiv = document.createElement('div');
        errorDiv.style.cssText = 'position:fixed;top:10px;right:10px;background:#f44336;color:white;padding:10px;border-radius:4px;z-index:9999;max-width:300px;box-shadow:0 4px 12px rgba(0,0,0,0.3);';
        errorDiv.textContent = message;
        document.body.appendChild(errorDiv);
        setTimeout(() => errorDiv.remove(), 4000);
    }

    function getCurrentFilters() {
        const rawFilters = {
            search: searchInput?.value.trim() || '',
            maxCost: maxCostInput?.value || '',
            originCity: originCityInput?.value.trim() || '',
            destination: destinationInput?.value.trim() || '',
            startDate: startDateInput?.value || '',
            endDate: endDateInput?.value || '',
            tags: Array.from(tagCheckboxes).filter(cb => cb.checked).map(cb => cb.value),
            sortBy: sortBySelect?.value || 'price',
        };

        const validFilters = {};
        let hasInvalidField = false;

        Object.entries(rawFilters).forEach(([fieldName, value]) => {
            if (value && !validateField(fieldName, value)) {
                const field = getFieldElementByName(fieldName);
                if (field) {
                    showFieldError(field);
                    hasInvalidField = true;
                }
            } else if (value) {
                validFilters[fieldName] = value;
                const field = getFieldElementByName(fieldName);
                if (field) clearFieldError(field);
            }
        });

        return validFilters;
    }

    function buildParams(filters) {
        const params = new URLSearchParams();
        Object.entries(filters).forEach(([key, value]) => {
            if (value && (!Array.isArray(value) || value.length > 0)) {
                params.set(key, Array.isArray(value) ? value.join(',') : value);
            }
        });
        return params;
    }

    async function loadTours() {
        const filters = getCurrentFilters();
        const params = buildParams(filters);
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

    function updateUrlAndLoad() {
        const filters = getCurrentFilters();
        const params = buildParams(filters);
        window.history.replaceState(null, '', `${location.pathname}?${params.toString()}`);
        loadTours();
    }

    function resetAllFilters() {
        if (searchInput) searchInput.value = '';
        if (maxCostInput) maxCostInput.value = '';
        if (originCityInput) originCityInput.value = '';
        if (destinationInput) destinationInput.value = '';
        if (startDateInput) startDateInput.value = '';
        if (endDateInput) endDateInput.value = '';
        if (sortBySelect) sortBySelect.value = 'price';
        tagCheckboxes.forEach(cb => cb.checked = false);

        // Очистить все ошибки
        [searchInput, maxCostInput, originCityInput, destinationInput, startDateInput, endDateInput].forEach(field => {
            if (field) clearFieldError(field);
        });

        window.history.replaceState(null, '', location.pathname);
        loadTours();
    }

    function updateCount() {
        const count = document.querySelectorAll('.card').length - 3;
        document.getElementById('count').textContent = count || '0';
    }

    function initFiltersFromUrl() {
        const urlParams = new URLSearchParams(window.location.search);

        if (searchInput) searchInput.value = urlParams.get('search') || '';
        if (maxCostInput) maxCostInput.value = urlParams.get('maxCost') || '';
        if (originCityInput) originCityInput.value = urlParams.get('originCity') || '';
        if (destinationInput) destinationInput.value = urlParams.get('destination') || '';

        if (startDateInput) {
            startDateInput.value = urlParams.get('startDate') || '';
            startDateInput.min = today;
        }
        if (endDateInput) {
            endDateInput.value = urlParams.get('endDate') || '';
            endDateInput.min = today;
        }

        const tagsParam = urlParams.get('tags');
        const selectedTags = tagsParam ? tagsParam.split(',') : [];
        tagCheckboxes.forEach(cb => cb.checked = selectedTags.includes(cb.value));

        if (sortBySelect) {
            const sortFromUrl = urlParams.get('sort');
            sortBySelect.value = ['price', 'date'].includes(sortFromUrl) ? sortFromUrl : 'price';
        }
    }

    initFiltersFromUrl();
    loadTours();

    if (resetButton) resetButton.addEventListener('click', resetAllFilters);

    [searchInput, originCityInput, destinationInput].forEach(el => {
        if (el) el.addEventListener('input', updateUrlAndLoad);
    });

    if (maxCostInput) {
        maxCostInput.addEventListener('input', () => {
            maxCostInput.value = maxCostInput.value.replace(/[^0-9]/g, '');
            updateUrlAndLoad();
        });
    }

    [startDateInput, endDateInput].forEach(el => {
        if (el) el.addEventListener('change', updateUrlAndLoad);
    });

    if (sortBySelect) sortBySelect.addEventListener('change', updateUrlAndLoad);

    tagCheckboxes.forEach(cb => cb.addEventListener('change', updateUrlAndLoad));

    if (startDateInput && endDateInput) {
        startDateInput.addEventListener('change', () => {
            endDateInput.min = startDateInput.value || today;
            updateUrlAndLoad();
        });
    }

    document.querySelectorAll(".filter-header").forEach(header => {
        header.addEventListener("click", () => header.parentElement.classList.toggle("active"));
    });
});