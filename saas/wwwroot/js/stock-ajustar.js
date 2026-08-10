document.addEventListener("DOMContentLoaded", function () {
    const tipoAjuste = document.getElementById("tipoAjuste");
    const cantidadAjuste = document.getElementById("cantidadAjuste");
    const stockResultante = document.getElementById("stockResultante");
    const motivo = document.getElementById("motivo");
    const motivoContador = document.getElementById("motivoContador");
    const stockActualElement = document.getElementById("stockActual");

    if (!tipoAjuste || !cantidadAjuste || !stockResultante || !motivo || !motivoContador || !stockActualElement) {
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

        stockResultante.textContent = resultado;
        stockResultante.classList.toggle("ajuste-resultado-negativo", resultado < 0);
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