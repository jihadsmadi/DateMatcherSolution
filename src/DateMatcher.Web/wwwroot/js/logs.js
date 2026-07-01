(function () {
    const modal = document.getElementById('logResponseModal');
    if (!modal) {
        return;
    }

    const modalLabel = document.getElementById('logResponseModalLabel');
    const modalMeta = document.getElementById('logResponseModalMeta');
    const errorWrap = document.getElementById('logResponseErrorWrap');
    const errorEl = document.getElementById('logResponseError');
    const responseBody = document.getElementById('logResponseBody');

    modal.addEventListener('show.bs.modal', async function (event) {
        const button = event.relatedTarget;
        if (!button) {
            return;
        }

        const logId = button.getAttribute('data-log-id');
        const status = button.getAttribute('data-log-status') ?? '';
        const criteria = button.getAttribute('data-log-criteria') ?? '';
        const error = button.getAttribute('data-log-error') ?? '';

        modalLabel.textContent = 'Log #' + logId + ' · ' + status;
        modalMeta.textContent = criteria;
        responseBody.textContent = 'Loading…';

        if (error) {
            errorEl.textContent = error;
            errorWrap.classList.remove('d-none');
        } else {
            errorEl.textContent = '';
            errorWrap.classList.add('d-none');
        }

        try {
            const response = await fetch('/api/searchlogs/' + logId);
            if (!response.ok) {
                responseBody.textContent = 'Unable to load response details.';
                return;
            }

            const log = await response.json();
            responseBody.textContent = log.responseJson || '[]';
        } catch {
            responseBody.textContent = 'Unable to load response details.';
        }
    });
})();
