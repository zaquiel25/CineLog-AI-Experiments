// Get the app: device tabs (the visitor's device opens first) and the one-tap install button.
(function () {
    'use strict';

    const tabs = document.querySelectorAll('[data-device-tab]');
    const panels = document.querySelectorAll('[data-device-panel]');
    if (!tabs.length) return;

    // iPadOS reports itself as a Mac; touch support tells them apart
    function detectDevice() {
        const ua = navigator.userAgent;
        if (/iPhone|iPad|iPod/.test(ua) || (/Macintosh/.test(ua) && navigator.maxTouchPoints > 1)) return 'iphone';
        if (/Android/.test(ua)) return 'android';
        return 'computer';
    }

    function show(device) {
        tabs.forEach(function (tab) {
            const on = tab.dataset.deviceTab === device;
            tab.classList.toggle('is-active', on);
            tab.setAttribute('aria-selected', on ? 'true' : 'false');
        });
        panels.forEach(function (panel) { panel.hidden = panel.dataset.devicePanel !== device; });
    }

    const current = detectDevice();
    const currentTab = document.querySelector('[data-device-tab="' + current + '"]');
    const mark = document.createElement('em');
    mark.textContent = 'this device';
    currentTab.appendChild(mark);

    tabs.forEach(function (tab) {
        tab.addEventListener('click', function (e) {
            e.preventDefault();
            show(tab.dataset.deviceTab);
        });
    });

    show(location.hash && document.querySelector('[data-device-tab="' + location.hash.slice(1) + '"]') ? location.hash.slice(1) : current);

    // ---------------------------------------------------------------- install
    const installBox = document.querySelector('[data-install]');
    const installButton = document.querySelector('[data-install-button]');
    const installed = document.querySelector('[data-installed]');
    let deferredPrompt = null;

    function markInstalled() {
        installBox.hidden = true;
        installed.hidden = false;
    }

    if (window.matchMedia('(display-mode: standalone)').matches || navigator.standalone) markInstalled();

    // Chrome and Edge (Android and computers) fire this when the site can be installed; Safari never does
    window.addEventListener('beforeinstallprompt', function (e) {
        e.preventDefault();
        deferredPrompt = e;
        installBox.hidden = false;
    });

    installButton.addEventListener('click', async function () {
        if (!deferredPrompt) return;
        deferredPrompt.prompt();
        await deferredPrompt.userChoice;
        deferredPrompt = null;
        installBox.hidden = true;
    });

    window.addEventListener('appinstalled', markInstalled);
})();
