// Log a movie (Add page): TMDB search, then the log form. Edit uses the same log form without the search step.
// The sentence (desktop) and the chips (mobile) are two views of the same values, kept in the hidden inputs.
(function () {
    'use strict';

    const form = document.getElementById('logForm');
    if (!form) return;

    const searchStep = form.querySelector('[data-step="search"]');
    const logStep = form.querySelector('[data-step="log"]');
    const searchInput = document.getElementById('logSearchInput');
    const resultsList = document.getElementById('logSearchResults');
    const hidden = {};
    form.querySelectorAll('[data-hidden]').forEach(function (input) { hidden[input.dataset.hidden] = input; });

    const IMG = 'https://image.tmdb.org/t/p/';

    // ---------------------------------------------------------------- dates
    function isoDate(date) {
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const day = String(date.getDate()).padStart(2, '0');
        return date.getFullYear() + '-' + month + '-' + day;
    }

    function todayIso() { return isoDate(new Date()); }

    function yesterdayIso() {
        const date = new Date();
        date.setDate(date.getDate() - 1);
        return isoDate(date);
    }

    form.querySelectorAll('[data-field="date"]').forEach(function (input) { input.max = todayIso(); });

    // ---------------------------------------------------------------- state
    const state = {
        when: 'today',
        date: '',
        where: hidden.where.value || 'Home',
        rewatch: hidden.rewatch.value === 'true',
        rating: parseFloat(hidden.rating.value) || 0
    };

    // Work out "when" from the posted date (today on a fresh form, anything after a validation error).
    (function initWhen() {
        const value = hidden.date.value;
        if (!value) state.when = 'unknown';
        else if (value === todayIso()) state.when = 'today';
        else if (value === yesterdayIso()) state.when = 'yesterday';
        else { state.when = 'date'; state.date = value; }
    })();

    function dateValue() {
        switch (state.when) {
            case 'today': return todayIso();
            case 'yesterday': return yesterdayIso();
            case 'date': return state.date;
            default: return '';
        }
    }

    function ratingText(value) {
        return value > 0 ? value.toString().replace(/\.0$/, '') + ' of 5' : 'No rating';
    }

    // Paint every control from the state, and copy it into the hidden inputs.
    function render() {
        hidden.date.value = dateValue();
        hidden.where.value = state.where;
        hidden.rewatch.value = state.rewatch ? 'true' : 'false';
        hidden.rating.value = state.rating > 0 ? state.rating.toFixed(1) : '';

        form.querySelectorAll('[data-pick]').forEach(function (menu) {
            const field = menu.dataset.pick;
            const current = field === 'rewatch' ? String(state.rewatch) : state[field];
            menu.querySelectorAll('[data-value]').forEach(function (option) {
                const on = option.dataset.value === current;
                option.classList.toggle('is-current', on);
                option.setAttribute('aria-checked', on ? 'true' : 'false');
            });
            const chosen = menu.querySelector('[data-value="' + current + '"]');
            // Once a date is picked the sentence reads "on 10/10/2026"; the menu still offers "on a date…"
            menu.querySelector('[data-pick-label]').textContent = field === 'when' && current === 'date' ? 'on' : chosen.textContent;
        });
        form.querySelectorAll('[data-field="rewatch-check"]').forEach(function (c) { c.checked = state.rewatch; });

        form.querySelectorAll('[data-field="date"]').forEach(function (input) {
            input.value = state.date;
            input.closest('label').hidden = state.when !== 'date';
        });

        form.querySelectorAll('[data-chip]').forEach(function (chip) {
            const active = chip.dataset.value === state[chip.dataset.chip];
            chip.classList.toggle('is-active', active);
            chip.setAttribute('aria-checked', active ? 'true' : 'false');
        });

        form.querySelectorAll('[data-rating]').forEach(function (dots) {
            Array.prototype.forEach.call(dots.children, function (dot, index) {
                dot.className = state.rating >= index + 1 ? 'is-full' : state.rating >= index + 0.5 ? 'is-half' : '';
            });
            dots.setAttribute('aria-valuenow', state.rating);
            dots.setAttribute('aria-valuetext', ratingText(state.rating));
        });
        form.querySelectorAll('[data-rating-text]').forEach(function (t) { t.textContent = ratingText(state.rating); });
    }

    // ---------------------------------------------------------------- controls
    function choose(field, value) {
        if (field === 'rewatch') state.rewatch = value === 'true';
        else state[field] = value;
        if (field === 'when' && value === 'date' && !state.date) state.date = todayIso();
    }

    form.addEventListener('change', function (e) {
        const field = e.target.dataset.field;
        if (field === 'rewatch-check') {
            state.rewatch = e.target.checked;
        } else if (field === 'date') {
            state.date = e.target.value;
        } else {
            return;
        }
        render();
    });

    form.addEventListener('click', function (e) {
        const chip = e.target.closest('[data-chip]');
        if (!chip) return;
        choose(chip.dataset.chip, chip.dataset.value);
        render();
    });

    // Sentence menus (desktop): native <details> with our own list instead of a <select>,
    // because native select popups rendered unreadable and in the wrong place.
    form.addEventListener('click', function (e) {
        const option = e.target.closest('[data-pick] [data-value]');
        if (!option) return;
        const menu = option.closest('[data-pick]');
        choose(menu.dataset.pick, option.dataset.value);
        menu.removeAttribute('open');
        render();
        const dateInput = form.querySelector('.fr-log-inline--date input');
        if (menu.dataset.pick === 'when' && option.dataset.value === 'date' && dateInput) dateInput.focus();
        else menu.querySelector('summary').focus();
    });

    function closeMenus(except) {
        form.querySelectorAll('details[data-pick][open]').forEach(function (menu) {
            if (menu !== except) menu.removeAttribute('open');
        });
    }

    document.addEventListener('click', function (e) { closeMenus(e.target.closest('details[data-pick]')); });
    document.addEventListener('keydown', function (e) { if (e.key === 'Escape') closeMenus(null); });

    // Rating dots: left half of a dot = half point. Clicking the current value clears the rating.
    form.addEventListener('click', function (e) {
        const dot = e.target.closest('[data-rating] i');
        if (!dot) return;
        const dots = Array.prototype.slice.call(dot.parentElement.children);
        const rect = dot.getBoundingClientRect();
        const value = dots.indexOf(dot) + 1 - ((e.clientX - rect.left) < rect.width / 2 ? 0.5 : 0);
        state.rating = value === state.rating ? 0 : value;
        render();
    });

    form.addEventListener('keydown', function (e) {
        if (!e.target.matches('[data-rating]')) return;
        const keys = { ArrowRight: 0.5, ArrowUp: 0.5, ArrowLeft: -0.5, ArrowDown: -0.5 };
        if (e.key in keys) state.rating = Math.min(5, Math.max(0, state.rating + keys[e.key]));
        else if (e.key === 'Home') state.rating = 0;
        else if (e.key === 'End') state.rating = 5;
        else return;
        e.preventDefault();
        render();
    });

    // ---------------------------------------------------------------- movie
    function showMovie(movie) {
        hidden.title.value = movie.title || '';
        hidden.director.value = movie.director || 'N/A';
        hidden.year.value = movie.releaseYear || '';
        hidden.poster.value = movie.posterPath || '';
        hidden.overview.value = movie.overview || '';
        hidden.tmdbId.value = movie.tmdbId || '';

        const meta = [movie.releaseYear, movie.director].filter(Boolean).join(' · ');
        form.querySelectorAll('[data-movie-title]').forEach(function (el) { el.textContent = movie.title || ''; });
        form.querySelectorAll('[data-movie-meta]').forEach(function (el) { el.textContent = meta; });

        // Poster and title open the movie page. Keep the movie in the URL so "back" from there
        // reloads this form with the movie already chosen.
        const previewUrl = '/Movies/Preview?tmdbId=' + encodeURIComponent(movie.tmdbId);
        form.querySelectorAll('[data-movie-link]').forEach(function (link) { link.href = previewUrl; });
        form.querySelector('.fr-log-poster').setAttribute('aria-label', 'More about ' + (movie.title || 'this movie'));
        history.replaceState(null, '', '/Movies/Add?tmdbId=' + encodeURIComponent(movie.tmdbId));

        const poster = form.querySelector('[data-movie-poster]');
        poster.hidden = !movie.posterPath;
        poster.src = movie.posterPath ? IMG + 'w342' + movie.posterPath : '';

        // A new movie starts a new log
        state.when = 'today';
        state.date = '';
        state.where = 'Home';
        state.rewatch = !!movie.alreadyLogged;
        state.rating = 0;
        render();

        searchStep.hidden = true;
        logStep.hidden = false;
        form.querySelector('[type=submit]').focus();
    }

    form.querySelectorAll('[data-change-movie]').forEach(function (button) {
        button.addEventListener('click', function () {
            logStep.hidden = true;
            searchStep.hidden = false;
            history.replaceState(null, '', '/Movies/Add');
            searchInput.focus();
            searchInput.select();
        });
    });

    form.addEventListener('submit', function (e) {
        if (!hidden.title.value || logStep.hidden) e.preventDefault();
    });

    // Edit: the delete button posts a separate form, so ask first
    const deleteForm = document.getElementById('deleteViewingForm');
    if (deleteForm) {
        deleteForm.addEventListener('submit', function (e) {
            if (!confirm('Delete this viewing of "' + deleteForm.dataset.confirmDelete + '"?')) e.preventDefault();
        });
    }

    // Edit has no search step: paint the saved values and stop here
    if (!searchInput) {
        render();
        return;
    }

    // FIX: Enter in the search box must never submit, even when a previous movie is still in the hidden
    // fields (after "Change movie"). Only the visible log step can be submitted.
    searchInput.addEventListener('keydown', function (e) {
        if (e.key === 'Enter') e.preventDefault();
    });

    // ---------------------------------------------------------------- search
    let timer = null;
    let lastQuery = '';

    function setResults(items) {
        resultsList.replaceChildren.apply(resultsList, items);
        resultsList.hidden = items.length === 0;
        searchInput.setAttribute('aria-expanded', items.length ? 'true' : 'false');
    }

    function messageItem(text) {
        const li = document.createElement('li');
        li.className = 'fr-log-results-msg';
        li.textContent = text;
        return li;
    }

    // Built with textContent: titles and overviews come from TMDB and must never be parsed as HTML.
    function resultItem(movie) {
        const li = document.createElement('li');
        li.setAttribute('role', 'option');

        const button = document.createElement('button');
        button.type = 'button';
        button.className = 'fr-log-result';
        button.dataset.tmdbId = movie.id;

        if (movie.posterPath) {
            const img = document.createElement('img');
            img.src = IMG + 'w92' + movie.posterPath;
            img.alt = '';
            img.width = 36;
            img.height = 54;
            img.loading = 'lazy';
            button.appendChild(img);
        } else {
            const blank = document.createElement('span');
            blank.className = 'fr-log-result-blank';
            button.appendChild(blank);
        }

        const text = document.createElement('span');
        text.className = 'fr-log-result-text';
        const title = document.createElement('span');
        title.className = 'fr-log-result-title';
        title.textContent = movie.title || 'Untitled';
        const meta = document.createElement('span');
        meta.className = 'fr-log-result-meta';
        const year = movie.releaseDate ? movie.releaseDate.substring(0, 4) : '';
        meta.textContent = [year, movie.overview].filter(Boolean).join(' · ');
        text.append(title, meta);
        button.appendChild(text);

        li.appendChild(button);
        return li;
    }

    searchInput.addEventListener('input', function () {
        clearTimeout(timer);
        const query = searchInput.value.trim();
        if (query.length < 2) {
            lastQuery = '';
            setResults([]);
            return;
        }
        timer = setTimeout(async function () {
            lastQuery = query;
            setResults([messageItem('Searching…')]);
            try {
                const response = await fetch('/Movies/SearchTmdbApi?query=' + encodeURIComponent(query));
                if (!response.ok) throw new Error('Search failed');
                const data = await response.json();
                if (query !== lastQuery) return; // a newer search is on its way
                const movies = (data.results || []).slice(0, 7);
                setResults(movies.length ? movies.map(resultItem) : [messageItem('No movies found.')]);
            } catch (err) {
                if (query === lastQuery) setResults([messageItem('Search is not available right now. Try again in a moment.')]);
            }
        }, 350);
    });

    resultsList.addEventListener('click', async function (e) {
        const button = e.target.closest('.fr-log-result');
        if (!button) return;
        button.disabled = true;
        try {
            const response = await fetch('/Movies/GetTmdbMovieDetailsJson?id=' + encodeURIComponent(button.dataset.tmdbId));
            if (!response.ok) throw new Error('Details failed');
            const movie = await response.json();
            setResults([]);
            showMovie(movie);
        } catch (err) {
            button.disabled = false;
            setResults([messageItem('Could not load that movie. Try again.')]);
        }
    });

    // Arrow keys move between the search box and the results.
    form.addEventListener('keydown', function (e) {
        if (e.key !== 'ArrowDown' && e.key !== 'ArrowUp' && e.key !== 'Escape') return;
        const buttons = Array.prototype.slice.call(resultsList.querySelectorAll('.fr-log-result'));
        if (e.target !== searchInput && buttons.indexOf(e.target) === -1) return;
        if (e.key === 'Escape') {
            setResults([]);
            searchInput.focus();
            return;
        }
        if (!buttons.length) return;
        e.preventDefault();
        const index = buttons.indexOf(e.target);
        const next = e.key === 'ArrowDown' ? index + 1 : index - 1;
        if (next < 0) searchInput.focus();
        else buttons[Math.min(next, buttons.length - 1)].focus();
    });

    render();
    if (!hidden.title.value) searchInput.focus();
})();
