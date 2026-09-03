document.addEventListener("DOMContentLoaded", () => {

    const form =
        document.getElementById("formRegistrarReintegroProveedor");

    const medioPagoSelect =
        document.getElementById("medioPagoSelect");

    const cajaSelect =
        document.getElementById("cajaSelect");

    const botonConfirmar =
        document.getElementById("btnConfirmarReintegroProveedor");

    if (!form ||
        !medioPagoSelect ||
        !cajaSelect) {
        return;
    }

    const getCajasUrl =
        form.dataset.getCajasUrl;

    const compraId =
        form.dataset.compraId;

    const cajaSeleccionada =
        form.dataset.cajaSeleccionada;

    async function cargarCajas() {

        const medioPagoId =
            medioPagoSelect.value;

        if (!medioPagoId) {

            cajaSelect.innerHTML =
                '<option value="">Primero seleccione un medio de pago</option>';

            return;
        }

        cajaSelect.innerHTML =
            '<option value="">Seleccionar caja</option>';

        const url =
            getCajasUrl
            + "?compraId="
            + encodeURIComponent(compraId)
            + "&medioPagoId="
            + encodeURIComponent(medioPagoId);

        try {

            const response =
                await fetch(url);

            if (!response.ok) {
                throw new Error();
            }

            const cajas =
                await response.json();

            cajas.forEach(caja => {

                const option =
                    document.createElement("option");

                option.value =
                    caja.id;

                option.textContent =
                    caja.nombre;

                cajaSelect.appendChild(option);
            });

            if (cajaSeleccionada &&
                cajas.some(caja =>
                    String(caja.id) ===
                    String(cajaSeleccionada))) {

                cajaSelect.value =
                    cajaSeleccionada;
            }
            else if (cajas.length === 1) {

                cajaSelect.value =
                    cajas[0].id;
            }

            if (cajas.length === 0) {

                cajaSelect.innerHTML =
                    '<option value="">No hay cajas disponibles</option>';
            }
        }
        catch {

            cajaSelect.innerHTML =
                '<option value="">No disponible</option>';
        }
    }

    medioPagoSelect.addEventListener(
        "change",
        cargarCajas);

    if (medioPagoSelect.value) {
        cargarCajas();
    }

    form.addEventListener("submit", () => {
        if (botonConfirmar) {
            botonConfirmar.disabled = true;
            botonConfirmar.textContent = "Procesando...";
        }
    });
});
