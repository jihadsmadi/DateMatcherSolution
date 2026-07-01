(function () {
    const form = document.getElementById('searchForm');
    const resultsPanel = document.getElementById('resultsPanel');
    const resultsEmpty = document.getElementById('resultsEmpty');
    const resultsCountBadge = document.getElementById('resultsCountBadge');
    const searchErrors = document.getElementById('searchErrors');
    const submitBtn = document.getElementById('searchSubmitBtn');

    if (!form || !resultsPanel) {
        return;
    }

    form.addEventListener('submit', async function (event) {
        event.preventDefault();
        clearErrors();
        setLoading(true);

        const payload = {
            startYear: parseInt(form.elements['SearchRequest.StartYear'].value, 10),
            endYear: parseInt(form.elements['SearchRequest.EndYear'].value, 10),
            dayOfMonth: parseInt(form.elements['SearchRequest.DayOfMonth'].value, 10),
            dayOfWeek: form.elements['SearchRequest.DayOfWeek'].value
        };

        try {
            const response = await fetch('/api/datematcher', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });

            const body = await response.json();

            if (!response.ok) {
                showErrors(extractErrors(body));
                renderEmptyResults();
                return;
            }

            renderResults(payload, body.matches ?? []);
        } catch {
            showErrors(['Unable to reach the search API. Please try again.']);
            renderEmptyResults();
        } finally {
            setLoading(false);
        }
    });

    function setLoading(isLoading) {
        submitBtn.disabled = isLoading;
        submitBtn.textContent = isLoading ? 'Searching…' : 'Find matches';
    }

    function clearErrors() {
        searchErrors.classList.add('d-none');
        searchErrors.textContent = '';
    }

    function showErrors(messages) {
        if (!messages.length) {
            return;
        }

        searchErrors.textContent = messages.join(' ');
        searchErrors.classList.remove('d-none');
    }

    function extractErrors(body) {
        if (!body) {
            return ['Validation failed.'];
        }

        if (Array.isArray(body.errors)) {
            return body.errors.filter(Boolean);
        }

        if (body.errors && typeof body.errors === 'object') {
            return Object.values(body.errors)
                .flat()
                .filter(Boolean);
        }

        if (body.detail) {
            return [body.detail];
        }

        if (body.title) {
            return [body.title];
        }

        return ['Validation failed.'];
    }

    function renderEmptyResults() {
        resultsCountBadge.classList.add('d-none');
        resultsPanel.innerHTML = '';
        resultsPanel.appendChild(resultsEmpty);
        resultsEmpty.classList.remove('d-none');
    }

    function renderResults(criteria, matches) {
        resultsEmpty.classList.add('d-none');
        resultsCountBadge.classList.remove('d-none');
        resultsCountBadge.textContent = matches.length + (matches.length === 1 ? ' match' : ' matches');

        if (matches.length === 0) {
            resultsPanel.innerHTML =
                '<div class="empty-state empty-state-compact">' +
                    '<div class="empty-state-icon">📭</div>' +
                    '<h3 class="h6 mb-2">No matches found</h3>' +
                    '<p class="text-muted mb-3">Try widening the year range or choosing a different day.</p>' +
                    '<a class="btn btn-sm btn-outline-secondary" href="/">Reset search</a>' +
                '</div>';
            return;
        }

        const summary =
            '<div class="results-summary mb-3">' +
                '<span class="text-muted small">' +
                    'Showing matches for day ' + criteria.dayOfMonth + ' on ' + criteria.dayOfWeek + ', ' +
                    criteria.startYear + '–' + criteria.endYear +
                '</span>' +
            '</div>';

        const chips = matches
            .map(function (match) { return '<span class="result-chip">' + escapeHtml(match) + '</span>'; })
            .join('');

        resultsPanel.innerHTML = summary + '<div class="results-grid">' + chips + '</div>';
    }

    function escapeHtml(value) {
        return String(value)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;');
    }
})();
