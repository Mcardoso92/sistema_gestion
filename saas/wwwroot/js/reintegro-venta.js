document.addEventListener(
    "DOMContentLoaded",
    () => {

        const inputs =
            document.querySelectorAll(
                ".cantidad-reintegrar");

    const totalVisual =
            document.getElementById(
                "totalReintegro");

        const formulario = document.getElementById("formReintegroVenta");
        const botonConfirmar = document.getElementById("btnConfirmarReintegroVenta");

        function actualizarTotal() {

            let total = 0;

            document
                .querySelectorAll(
                    ".reintegro-detalle")
                .forEach(row => {

                    const precio =
                        parseFloat(
                            row.dataset.precio)
                        || 0;

                    const input =
                        row.querySelector(
                            ".cantidad-reintegrar");

                    const cantidad =
                        parseInt(
                            input?.value)
                        || 0;

                    total +=
                        precio * cantidad;
                });

            totalVisual.textContent =
                total.toLocaleString(
                    "es-AR",
                    {
                        style: "currency",
                        currency: "ARS"
                    });
        }

        inputs.forEach(input => {

            input.addEventListener(
                "input",
                actualizarTotal);
        });

        actualizarTotal();

        formulario?.addEventListener("submit", () => {
            if (botonConfirmar) {
                botonConfirmar.disabled = true;
                botonConfirmar.textContent = "Procesando...";
            }
        });
    });
