document.addEventListener("DOMContentLoaded", () => {

    const form =
        document.getElementById("formRegistrarCobro");

    const medioPagoSelect =
        document.getElementById("medioPagoSelect");

    const cajaSelect =
        document.getElementById("cajaSelect");

    const importeCobro =
        document.getElementById("importeCobro");

    const bloqueEfectivo =
        document.getElementById("bloqueEfectivo");

    const efectivoRecibido =
        document.getElementById("efectivoRecibido");

    const efectivoVuelto =
        document.getElementById("efectivoVuelto");

    if (!form ||
        !medioPagoSelect ||
        !cajaSelect) {
        return;
    }

    const getCajasUrl =
        form.dataset.getCajasUrl;

    const ventaId =
        form.dataset.ventaId;

    function esEfectivo() {

        const option =
            medioPagoSelect.options[
            medioPagoSelect.selectedIndex
            ];

        if (!option) {
            return false;
        }

        return Number(option.dataset.tipo) === 1;
    }

    function actualizarBloqueEfectivo() {

        if (esEfectivo()) {

            bloqueEfectivo.classList.remove(
                "d-none");

            actualizarVuelto();
        }
        else {

            bloqueEfectivo.classList.add(
                "d-none");

            efectivoRecibido.value = "";

            efectivoVuelto.textContent =
                "$0,00";
        }
    }

    function actualizarVuelto() {

        const importe =
            parseFloat(
                importeCobro.value
            ) || 0;

        const recibido =
            parseFloat(
                efectivoRecibido.value
            ) || 0;

        const vuelto =
            Math.max(
                0,
                recibido - importe);

        efectivoVuelto.textContent =
            vuelto.toLocaleString(
                "es-AR",
                {
                    style: "currency",
                    currency: "ARS"
                });
    }

    async function cargarCajas() {

        const medioPagoId =
            medioPagoSelect.value;

        cajaSelect.innerHTML =
            '<option value="">Seleccionar caja</option>';

        if (!medioPagoId) {

            cajaSelect.innerHTML =
                '<option value="">Primero seleccione un medio de pago</option>';

            return;
        }

        const url =
            getCajasUrl
            + "?ventaId="
            + encodeURIComponent(
                ventaId)
            + "&medioPagoId="
            + encodeURIComponent(
                medioPagoId);

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
                    document.createElement(
                        "option");

                option.value =
                    caja.id;

                option.textContent =
                    caja.nombre;

                cajaSelect.appendChild(
                    option);
            });

            if (cajas.length === 1) {

                cajaSelect.value =
                    cajas[0].id;
            }
        }
        catch {

            cajaSelect.innerHTML =
                '<option value="">No disponible</option>';
        }
    }

    medioPagoSelect.addEventListener(
        "change",
        async () => {

            await cargarCajas();

            actualizarBloqueEfectivo();
        });

    if (importeCobro) {

        importeCobro.addEventListener(
            "input",
            actualizarVuelto);
    }

    if (efectivoRecibido) {

        efectivoRecibido.addEventListener(
            "input",
            actualizarVuelto);
    }

    actualizarBloqueEfectivo();
});