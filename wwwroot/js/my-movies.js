// My Movies (Journal and Collection): filter auto-submit, "•••" menus and delete.
(function () {
    'use strict';

    // Filters: changing month, sort or "first watches only" reloads the list with the new query.
    const filters = document.getElementById('myMoviesFilters');
    if (filters) {
        // Keep URLs clean: drop empty fields (e.g. "searchString=") from the query.
        filters.addEventListener('submit', function () {
            filters.querySelectorAll('input[type=search], select').forEach(function (field) {
                if (!field.value) field.disabled = true;
            });
        });

        // Re-enable them when the page comes back from the browser's back/forward cache.
        window.addEventListener('pageshow', function () {
            filters.querySelectorAll('[disabled]').forEach(function (field) { field.disabled = false; });
        });

        filters.querySelectorAll('[data-autosubmit]').forEach(function (control) {
            control.addEventListener('change', function () {
                filters.requestSubmit ? filters.requestSubmit() : filters.submit();
            });
        });
    }

    // "•••" menus are native <details>: close any open one when clicking elsewhere or pressing Escape.
    function closeMenus(except) {
        document.querySelectorAll('details.fr-more[open]').forEach(function (menu) {
            if (menu !== except) menu.removeAttribute('open');
        });
    }

    document.addEventListener('click', function (e) {
        closeMenus(e.target.closest('details.fr-more'));
    });

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') closeMenus(null);
    });

    // Delete a viewing. Journal removes the row in place; Collection reloads, because the film may
    // still have other viewings.
    document.addEventListener('click', async function (e) {
        const button = e.target.closest('[data-delete-movie]');
        if (!button) return;

        const title = button.dataset.movieTitle || 'this movie';
        const isLatest = button.hasAttribute('data-delete-latest');
        const question = isLatest
            ? 'Delete your latest viewing of "' + title + '"?'
            : 'Delete "' + title + '" from your journal?';
        if (!confirm(question)) return;

        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        button.disabled = true;

        try {
            const response = await fetch('/Movies/Delete', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: 'id=' + encodeURIComponent(button.dataset.movieId) +
                      '&__RequestVerificationToken=' + encodeURIComponent(tokenInput ? tokenInput.value : ''),
                credentials: 'same-origin'
            });

            const data = await response.json().catch(function () { return null; });
            if (!response.ok || !data || !data.success) {
                throw new Error('Delete failed');
            }

            if (isLatest) {
                window.location.reload();
                return;
            }

            const row = button.closest('[data-movie-row]');
            const previous = row.previousElementSibling;
            const next = row.nextElementSibling;

            // The month badge sits on the first row of each month: hand it to the next row of that month.
            const badge = row.querySelector('.fr-diary-mon');
            if (badge && badge.children.length && next && next.matches('[data-movie-row]')) {
                next.querySelector('.fr-diary-mon').innerHTML = badge.innerHTML;
            }
            row.remove();

            // Remove a month heading left without movies.
            if (previous && previous.classList.contains('fr-diary-month') &&
                (!next || next.classList.contains('fr-diary-month'))) {
                previous.remove();
            }

            const count = document.querySelector('[data-movie-count]');
            if (count) {
                const remaining = Math.max(0, parseInt(count.dataset.movieCount, 10) - 1);
                count.dataset.movieCount = remaining;
                count.textContent = remaining + (remaining === 1 ? ' film' : ' films');
            }

            // Page is empty now: reload to show the previous page or the empty state.
            if (!document.querySelector('[data-movie-row]')) {
                window.location.reload();
            }
        } catch (err) {
            button.disabled = false;
            alert('Sorry, we could not delete "' + title + '". Please try again.');
        }
    });
})();
