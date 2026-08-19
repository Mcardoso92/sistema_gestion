document.addEventListener(
    "DOMContentLoaded",
    () => {

        const inputs =
            document.querySelectorAll(
                ".cantidad-reintegrar");

        const totalVisual =
            document.getElementById(
                "totalReintegro");

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
    });