document.addEventListener("DOMContentLoaded", () => {
    const tarjeta = document.querySelector(".js-onboarding");

    if (!tarjeta) {
        return;
    }

    const contenido = tarjeta.querySelector(".collapse");
    const botonAlternar = tarjeta.querySelector(".js-onboarding-toggle");

    contenido?.addEventListener("hidden.bs.collapse", () => {
        if (botonAlternar) {
            botonAlternar.textContent = "Mostrar";
        }
    });

    contenido?.addEventListener("shown.bs.collapse", () => {
        if (botonAlternar) {
            botonAlternar.textContent = "Minimizar";
        }
    });

    const botonFinalizar = tarjeta.querySelector(".js-onboarding-finalizar");
    const token = tarjeta.querySelector(
        '.js-onboarding-token input[name="__RequestVerificationToken"]');
    const mensajeError = tarjeta.querySelector(".js-onboarding-error");
    const urlFinalizar = tarjeta.dataset.finalizarUrl;

    if (!botonFinalizar || !token || !urlFinalizar) {
        return;
    }

    botonFinalizar.addEventListener("click", async () => {
        botonFinalizar.setAttribute("disabled", "disabled");
        mensajeError?.classList.add("d-none");

        try {
            const respuesta = await fetch(urlFinalizar, {
                method: "POST",
                headers: {
                    "RequestVerificationToken": token.value
                }
            });

            if (!respuesta.ok) {
                throw new Error("No se pudo finalizar el onboarding.");
            }

            tarjeta.remove();
        } catch {
            mensajeError?.classList.remove("d-none");
            botonFinalizar.removeAttribute("disabled");
        }
    });
});
