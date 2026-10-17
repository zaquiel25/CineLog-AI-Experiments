// Suggestions: chips load results via AJAX, Shuffle swaps the cards, cards handle wishlist / not interested.
(function () {
    'use strict';

    const slot = document.getElementById('suggestion-results-slot');
    const reshuffleEndpoints = {
        trending: '/Movies/TrendingReshuffle',
        cast: '/Movies/CastReshuffle',
        director: '/Movies/DirectorReshuffle',
        decade: '/Movies/DecadeReshuffle',
        genre: '/Movies/GenreReshuffle',
        surprise: '/Movies/SurpriseMeReshuffle'
    };

    function setActiveChip(type) {
        document.querySelectorAll('.fr-chip[data-suggestion-type]').forEach(function (chip) {
            const active = chip.dataset.suggestionType === type;
            chip.classList.toggle('is-active', active);
            if (active) chip.setAttribute('aria-current', 'true'); else chip.removeAttribute('aria-current');
        });
    }

    function scrollToResults() {
        const results = document.getElementById('suggestion-results-area');
        if (!results) return;
        const reduce = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
        results.scrollIntoView({ behavior: reduce ? 'auto' : 'smooth', block: 'start' });
    }

    async function getJson(url) {
        const response = await fetch(url, {
            headers: { 'X-Requested-With': 'XMLHttpRequest' },
            credentials: 'same-origin'
        });
        if (!response.ok) throw new Error('HTTP ' + response.status);
        const data = await response.json();
        if (!data.success || typeof data.html !== 'string') throw new Error(data.error || 'Invalid response');
        return data;
    }

    // Chips: load a suggestion type. Falls back to a normal page load if anything fails.
    document.addEventListener('click', async function (e) {
        const chip = e.target.closest('.fr-chip[data-suggestion-type]');
        if (!chip || !slot) return;
        e.preventDefault();

        const type = chip.dataset.suggestionType;
        setActiveChip(type);
        slot.setAttribute('aria-busy', 'true');
        slot.classList.add('is-loading');

        try {
            const data = await getJson(chip.href);
            slot.innerHTML = data.html;
            scrollToResults();
        } catch (err) {
            window.location.href = chip.href;
        } finally {
            slot.removeAttribute('aria-busy');
            slot.classList.remove('is-loading');
        }
    });

    // Shuffle: replace only the cards (and the heading/avatar when the server sends new ones).
    document.addEventListener('click', async function (e) {
        const button = e.target.closest('.reshuffle-btn');
        if (!button) return;
        e.preventDefault();

        const endpoint = reshuffleEndpoints[button.dataset.suggestionType];
        if (!endpoint) return;

        button.disabled = true;
        button.textContent = 'Shuffling…';

        try {
            const data = await getJson(endpoint);
            const grid = document.getElementById('suggestion-grid');
            if (grid) grid.innerHTML = data.html;

            const title = document.getElementById('suggestion-title');
            if (title && data.suggestionTitle) title.textContent = data.suggestionTitle;

            const avatar = document.getElementById('actor-profile-image');
            if (avatar) {
                if (data.actorProfileUrl) {
                    avatar.src = 'https://image.tmdb.org/t/p/w185' + data.actorProfileUrl;
                    avatar.hidden = false;
                } else {
                    avatar.hidden = true;
                }
            }
            scrollToResults();
        } catch (err) {
            alert('Sorry, we could not shuffle right now. Please try again.');
        } finally {
            button.disabled = false;
            button.textContent = 'Shuffle';
        }
    });

    // Save to Wishlist / Not interested (blacklist). Delegated, so it works for AJAX-loaded cards.
    document.addEventListener('submit', async function (e) {
        const form = e.target;
        const isWishlist = form.classList.contains('wishlist-form');
        const isBlacklist = form.classList.contains('blacklist-form');
        if (!isWishlist && !isBlacklist) return;
        e.preventDefault();

        const button = form.querySelector('button[type=submit]');
        const card = form.closest('.fr-suggest-card');
        const original = button.textContent;
        button.disabled = true;

        try {
            const response = await fetch(form.action, {
                method: 'POST',
                body: new FormData(form),
                headers: { 'X-Requested-With': 'XMLHttpRequest' },
                credentials: 'same-origin'
            });
            if (!response.ok) throw new Error('HTTP ' + response.status);

            if (isWishlist) {
                button.textContent = 'In your wishlist';
                // Wishlist and blacklist are mutually exclusive
                const notInterested = card && card.querySelector('.blacklist-form');
                if (notInterested) notInterested.remove();
            } else if (card) {
                card.classList.add('is-leaving');
                setTimeout(function () { card.remove(); }, 250);
            }
        } catch (err) {
            button.disabled = false;
            button.textContent = original;
            alert(isWishlist
                ? 'Sorry, we could not save it to your wishlist. Please try again.'
                : 'Sorry, we could not hide this movie. Please try again.');
        }
    });

    // Close any open "•••" menu when clicking elsewhere or pressing Escape.
    document.addEventListener('click', function (e) {
        const current = e.target.closest('details.fr-more');
        document.querySelectorAll('details.fr-more[open]').forEach(function (menu) {
            if (menu !== current) menu.removeAttribute('open');
        });
    });
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            document.querySelectorAll('details.fr-more[open]').forEach(function (menu) { menu.removeAttribute('open'); });
        }
    });

    // Results rendered on page load (no-JS fallback / direct link): bring them into view.
    if (document.getElementById('suggestion-results-area')) scrollToResults();
})();
