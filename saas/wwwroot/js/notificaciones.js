document.addEventListener("DOMContentLoaded", () => {
    document.querySelectorAll(".js-notifications").forEach((contenedor) => {
        const boton = contenedor.querySelector('[data-bs-toggle="dropdown"]');
        const token = contenedor.querySelector(
            '.js-notifications-token input[name="__RequestVerificationToken"]');
        const url = contenedor.dataset.markReadUrl;

        if (!boton || !token || !url) {
            return;
        }

        let solicitudEnCurso = false;
        let revisionRegistrada = false;

        boton.addEventListener("shown.bs.dropdown", async () => {
            const insignias = contenedor.querySelectorAll(
                ".js-notification-new-badge");

            if (!insignias.length || solicitudEnCurso || revisionRegistrada) {
                return;
            }

            solicitudEnCurso = true;

            try {
                const respuesta = await fetch(url, {
                    method: "POST",
                    headers: {
                        "RequestVerificationToken": token.value
                    }
                });

                if (!respuesta.ok) {
                    return;
                }

                revisionRegistrada = true;
                insignias.forEach((insignia) => insignia.remove());
            } catch {
                // Conserva la insignia para no aparentar una revisión que falló.
            } finally {
                solicitudEnCurso = false;
            }
        });
    });
});
