document.addEventListener("DOMContentLoaded", () => {
    const elementoModal = document.querySelector(".js-bienvenida-modal");

    if (!elementoModal) {
        return;
    }

    const urlCompletar = elementoModal.dataset.completarUrl;
    const token = elementoModal.querySelector(
        '.js-bienvenida-token input[name="__RequestVerificationToken"]');
    const acciones = elementoModal.querySelectorAll(".js-bienvenida-accion");
    const mensajeError = elementoModal.querySelector(".js-bienvenida-error");
    const modal = bootstrap.Modal.getOrCreateInstance(elementoModal);
    let solicitudEnCurso = false;

    if (!urlCompletar || !token) {
        return;
    }

    modal.show();

    acciones.forEach((accion) => {
        accion.addEventListener("click", async () => {
            if (solicitudEnCurso) {
                return;
            }

            solicitudEnCurso = true;
            mensajeError?.classList.add("d-none");
            acciones.forEach((boton) => boton.setAttribute("disabled", "disabled"));

            try {
                const respuesta = await fetch(urlCompletar, {
                    method: "POST",
                    headers: {
                        "RequestVerificationToken": token.value
                    }
                });

                if (!respuesta.ok) {
                    throw new Error("No se pudo registrar la bienvenida.");
                }

                const destino = accion.dataset.destino;

                if (destino) {
                    window.location.assign(destino);
                    return;
                }

                modal.hide();
            } catch {
                mensajeError?.classList.remove("d-none");
                acciones.forEach((boton) => boton.removeAttribute("disabled"));
                solicitudEnCurso = false;
            }
        });
    });
});
