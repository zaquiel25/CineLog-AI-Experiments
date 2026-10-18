// Account pages (log in, sign up, password forms): show / hide password buttons.
(function () {
    'use strict';

    document.querySelectorAll('[data-pw-toggle]').forEach(function (button) {
        button.addEventListener('click', function () {
            const input = button.parentElement.querySelector('input');
            const icon = button.querySelector('i');
            const show = input.type === 'password';
            input.type = show ? 'text' : 'password';
            icon.classList.toggle('bi-eye', !show);
            icon.classList.toggle('bi-eye-slash', show);
            button.setAttribute('aria-label', show ? 'Hide password' : 'Show password');
        });
    });
})();
