document.addEventListener("DOMContentLoaded", function () {
    const tipoAjuste = document.getElementById("tipoAjuste");
    const cantidadAjuste = document.getElementById("cantidadAjuste");
    const stockResultante = document.getElementById("stockResultante");
    const motivo = document.getElementById("motivo");
    const motivoContador = document.getElementById("motivoContador");
    const stockActualElement = document.getElementById("stockActual");
    const stockResultanteError = document.getElementById("stockResultanteError");
    const btnGuardarAjuste = document.getElementById("btnGuardarAjuste");

    if (!tipoAjuste || !cantidadAjuste || !stockResultante || !motivo || !motivoContador || !stockActualElement || !stockResultanteError || !btnGuardarAjuste) {
        return;
    }

    const stockActual = parseInt(stockActualElement.dataset.stock) || 0;
    const tipoEntrada = parseInt(stockActualElement.dataset.tipoEntrada);
    const tipoSalida = parseInt(stockActualElement.dataset.tipoSalida);

    function actualizarStockResultante() {
        const cantidad = parseInt(cantidadAjuste.value) || 0;
        const tipo = parseInt(tipoAjuste.value);
        let resultado = stockActual;

        if (tipo === tipoEntrada) {
            resultado = stockActual + cantidad;
        } else if (tipo === tipoSalida) {
            resultado = stockActual - cantidad;
        }

        const salidaInvalida =
            tipo === tipoSalida && cantidad > stockActual;

        stockResultante.textContent = salidaInvalida ? "—" : resultado;
        stockResultante.classList.toggle("ajuste-resultado-negativo", salidaInvalida);
        stockResultanteError.classList.toggle("d-none", !salidaInvalida);
        cantidadAjuste.classList.toggle("is-invalid", salidaInvalida);
        btnGuardarAjuste.disabled = salidaInvalida;
    }

    function actualizarContador() {
        motivoContador.textContent = motivo.value.length;
    }

    tipoAjuste.addEventListener("change", actualizarStockResultante);
    cantidadAjuste.addEventListener("input", actualizarStockResultante);
    motivo.addEventListener("input", actualizarContador);

    actualizarStockResultante();
    actualizarContador();
});
