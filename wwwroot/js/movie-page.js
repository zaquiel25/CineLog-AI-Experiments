// Movie page (Preview and Details): "•••" menu, remove from Wishlist / Not interested, delete confirmation.
(function () {
    'use strict';

    // "•••" menus are native <details>: close when clicking elsewhere or pressing Escape.
    function closeMenus(except) {
        document.querySelectorAll('details.fr-more[open]').forEach(function (menu) {
            if (menu !== except) menu.removeAttribute('open');
        });
    }

    document.addEventListener('click', function (e) { closeMenus(e.target.closest('details.fr-more')); });
    document.addEventListener('keydown', function (e) { if (e.key === 'Escape') closeMenus(null); });

    // Remove from Wishlist / show in suggestions again, then reload so the page shows the new state.
    const endpoints = { wishlist: '/Movies/RemoveFromWishlist', blacklist: '/Movies/RemoveFromBlacklist' };

    document.addEventListener('click', async function (e) {
        const button = e.target.closest('[data-movie-remove]');
        if (!button) return;
        const url = endpoints[button.dataset.movieRemove];
        if (!url) return;

        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        button.disabled = true;
        try {
            const response = await fetch(url, {
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
            if (!response.ok || !data || !data.success) throw new Error('Remove failed');
            window.location.reload();
        } catch (err) {
            button.disabled = false;
            alert('Sorry, that did not work. Please try again.');
        }
    });

    // Deleting a viewing cannot be undone: ask first.
    document.querySelectorAll('form[data-confirm-delete]').forEach(function (form) {
        form.addEventListener('submit', function (e) {
            if (!confirm('Delete this viewing of "' + form.dataset.confirmDelete + '"?')) e.preventDefault();
        });
    });
})();
