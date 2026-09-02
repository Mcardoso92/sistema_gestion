document.addEventListener("DOMContentLoaded", () => {
    const precioCosto = document.getElementById("PrecioCosto");
    const precioVenta = document.getElementById("PrecioVenta");
    const formulario = precioVenta?.closest("form");

    if (!precioCosto || !precioVenta || !formulario) {
        return;
    }

    // Solicita confirmación al guardar, sin interrumpir la carga normal de los precios.
    formulario.addEventListener("submit", evento => {
        if (evento.defaultPrevented || !formulario.checkValidity()) {
            return;
        }

        const hayValores = precioCosto.value !== "" && precioVenta.value !== "";
        const margenNegativo = Number(precioVenta.value) < Number(precioCosto.value);

        if (hayValores && margenNegativo && !window.confirm("El precio de venta es inferior al precio de compra. El producto tendrá margen negativo. ¿Desea continuar?")) {
            evento.preventDefault();
            precioVenta.focus();
        }
    });
});
