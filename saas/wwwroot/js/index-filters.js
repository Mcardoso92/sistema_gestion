document.addEventListener("DOMContentLoaded", () => {
    const forms = document.querySelectorAll("form[data-filter-autosubmit]");
    const focusStorageKey = `veltika:index-filter-focus:${window.location.pathname}`;

    forms.forEach((form) => {
        const searchInput = form.querySelector("[data-filter-search]");
        if (!searchInput) {
            return;
        }

        const delay = Number.parseInt(form.dataset.filterDelay ?? "450", 10);
        let timerId = null;
        let isComposing = false;

        const cancelPendingSubmit = () => {
            if (timerId !== null) {
                window.clearTimeout(timerId);
                timerId = null;
            }
        };

        const scheduleSubmit = () => {
            cancelPendingSubmit();
            if (isComposing) {
                return;
            }

            timerId = window.setTimeout(() => {
                timerId = null;

                // Si la vista incorpora la página como campo, toda búsqueda nueva vuelve al inicio.
                const pageInput = form.querySelector("[name='pagina'], [name='paginaActual']");
                if (pageInput) {
                    pageInput.value = "1";
                }

                // La navegación conserva el campo activo y la posición del cursor.
                try {
                    window.sessionStorage.setItem(focusStorageKey, JSON.stringify({
                        inputName: searchInput.name,
                        selectionStart: searchInput.selectionStart,
                        selectionEnd: searchInput.selectionEnd,
                        savedAt: Date.now()
                    }));
                } catch {
                    // La búsqueda debe continuar aunque el navegador bloquee sessionStorage.
                }

                if (typeof form.requestSubmit === "function") {
                    form.requestSubmit();
                } else {
                    form.submit();
                }
            }, Number.isFinite(delay) ? delay : 450);
        };

        searchInput.addEventListener("compositionstart", () => {
            isComposing = true;
            cancelPendingSubmit();
        });
        searchInput.addEventListener("compositionend", () => {
            isComposing = false;
            scheduleSubmit();
        });
        searchInput.addEventListener("input", scheduleSubmit);
        form.addEventListener("submit", cancelPendingSubmit);
    });

    let savedFocus = null;
    try {
        savedFocus = window.sessionStorage.getItem(focusStorageKey);
        window.sessionStorage.removeItem(focusStorageKey);
    } catch {
        // Sin almacenamiento disponible, la búsqueda conserva su comportamiento base.
    }

    if (savedFocus) {
        try {
            const focusState = JSON.parse(savedFocus);
            const isRecent = Date.now() - focusState.savedAt < 10000;
            const searchInput = Array.from(forms)
                .map((form) => form.querySelector("[data-filter-search]"))
                .find((input) => input?.name === focusState.inputName);

            if (isRecent && searchInput) {
                searchInput.focus({ preventScroll: true });

                if (typeof searchInput.setSelectionRange === "function") {
                    const textLength = searchInput.value.length;
                    const selectionStart = Math.min(focusState.selectionStart ?? textLength, textLength);
                    const selectionEnd = Math.min(focusState.selectionEnd ?? selectionStart, textLength);
                    searchInput.setSelectionRange(selectionStart, selectionEnd);
                }
            }
        } catch {
            // Un valor inválido o bloqueado en sessionStorage no debe afectar los filtros.
        }
    }
});
