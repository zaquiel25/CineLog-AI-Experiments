// Wishlist page (Want to see / Not interested tabs): filter auto-submit, "•••" menus and remove.
(function () {
    'use strict';

    // Filters: changing the sort reloads the wishlist with the new query.
    const filters = document.getElementById('listFilters');
    if (filters) {
        // Keep URLs clean: drop an empty search ("searchString=") from the query.
        filters.addEventListener('submit', function () {
            filters.querySelectorAll('input[type=search]').forEach(function (field) {
                if (!field.value) field.disabled = true;
            });
        });

        // Re-enable it when the page comes back from the browser's back/forward cache.
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

    // Remove from the wishlist, or show a hidden movie in suggestions again. The item leaves in place;
    // no confirm, because both are easy to undo from Suggestions.
    const endpoints = {
        wishlist: { url: '/Movies/RemoveFromWishlist', count: 'want' },
        blacklist: { url: '/Movies/RemoveFromBlacklist', count: 'hidden' }
    };

    document.addEventListener('click', async function (e) {
        const button = e.target.closest('[data-list-remove]');
        if (!button) return;

        const endpoint = endpoints[button.dataset.listRemove];
        if (!endpoint) return;

        const title = button.dataset.title || 'this movie';
        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        button.disabled = true;

        try {
            const response = await fetch(endpoint.url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: 'tmdbId=' + encodeURIComponent(button.dataset.tmdbId) +
                      '&__RequestVerificationToken=' + encodeURIComponent(tokenInput ? tokenInput.value : ''),
                credentials: 'same-origin'
            });

            const data = await response.json().catch(function () { return null; });
            if (!response.ok || !data || !data.success) {
                throw new Error('Remove failed');
            }

            button.closest('[data-list-item]').remove();

            const count = document.querySelector('[data-list-count="' + endpoint.count + '"]');
            if (count) {
                count.textContent = Math.max(0, parseInt(count.textContent, 10) - 1);
            }

            // Page is empty now: reload to show the previous page or the empty state.
            if (!document.querySelector('[data-list-item]')) {
                window.location.reload();
            }
        } catch (err) {
            button.disabled = false;
            alert('Sorry, we could not update "' + title + '". Please try again.');
        }
    });
})();
