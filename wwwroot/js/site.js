// ── Dark mode ────────────────────────────────────────────
(function () {
    const html   = document.documentElement;
    const stored = localStorage.getItem('theme') || 'light';
    html.setAttribute('data-theme', stored);

    document.addEventListener('DOMContentLoaded', function () {
        const btn  = document.getElementById('darkToggle');
        const icon = document.getElementById('darkIcon');

        function applyTheme(theme) {
            html.setAttribute('data-theme', theme);
            localStorage.setItem('theme', theme);
            if (icon) {
                icon.className = theme === 'dark' ? 'bi bi-sun-fill' : 'bi bi-moon-fill';
            }
        }

        applyTheme(stored);

        if (btn) {
            btn.addEventListener('click', function () {
                const next = html.getAttribute('data-theme') === 'dark' ? 'light' : 'dark';
                applyTheme(next);
            });
        }

        // Auto-ocultar toast después de 4 segundos
        const toast = document.getElementById('toastGlobal');
        if (toast) {
            setTimeout(function () {
                toast.style.transition = 'opacity .4s';
                toast.style.opacity    = '0';
                setTimeout(function () { toast.remove(); }, 400);
            }, 4000);
        }
    });
})();
