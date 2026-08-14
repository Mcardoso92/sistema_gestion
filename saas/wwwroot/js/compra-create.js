document.addEventListener("DOMContentLoaded", () => {
    const compraForm = document.getElementById("compraForm");
    const detalleBody = document.getElementById("detalleCompraBody");
    const detalleTemplate = document.getElementById("detalleCompraTemplate");
    const agregarProductoBtn = document.getElementById("agregarProductoBtn");
    const emptyState = document.getElementById("compraEmptyState");
    const totalCompra = document.getElementById("totalCompra");
    const empresaSelect = document.getElementById("empresaSelect");

    if (!compraForm || !detalleBody || !detalleTemplate || !agregarProductoBtn) {
        return;
    }

    const formatearMoneda = valor => {
        const numero = Number(valor) || 0;

        return numero.toLocaleString("es-AR", {
            style: "currency",
            currency: "ARS"
        });
    };

    const obtenerEmpresaId = () => {
        if (!empresaSelect) {
            return null;
        }

        return empresaSelect.value || null;
    };

    const obtenerFilas = () =>
        Array.from(detalleBody.querySelectorAll(".compra-detalle-row"));

    const actualizarEstadoVacio = () => {
        const hayFilas = obtenerFilas().length > 0;

        if (emptyState) {
            emptyState.classList.toggle("d-none", hayFilas);
        }
    };

    const recalcularTotal = () => {
        let total = 0;

        obtenerFilas().forEach(fila => {
            const cantidadInput = fila.querySelector(".cantidad-input");
            const costoInput = fila.querySelector(".costo-input");
            const subtotalCell = fila.querySelector(".subtotal-cell");

            const cantidad = Number(cantidadInput?.value) || 0;
            const costo = Number(costoInput?.value) || 0;
            const subtotal = cantidad * costo;

            if (subtotalCell) {
                subtotalCell.textContent = formatearMoneda(subtotal);
            }

            total += subtotal;
        });

        if (totalCompra) {
            totalCompra.textContent = formatearMoneda(total);
        }
    };

    const renumerarFilas = () => {
        obtenerFilas().forEach((fila, index) => {
            fila.querySelectorAll("[data-field]").forEach(control => {
                const field = control.dataset.field;

                control.name = `Detalles[${index}].${field}`;
            });
        });
    };

    const productoYaSeleccionado = (productoId, filaActual) => {
        if (!productoId) {
            return false;
        }

        return obtenerFilas().some(fila => {
            if (fila === filaActual) {
                return false;
            }

            const select = fila.querySelector(".producto-select");

            return select?.value === productoId;
        });
    };

    const limpiarPrecioActual = fila => {
        const precioActualInput = fila.querySelector(".precio-actual-input");
        const nuevoPrecioInput = fila.querySelector(".nuevo-precio-input");

        if (precioActualInput) {
            precioActualInput.value = "";
        }

        if (nuevoPrecioInput) {
            nuevoPrecioInput.value = "";
        }

        actualizarAdvertencias(fila);
    };

    const consultarProducto = async (fila, productoId) => {
        if (!productoId) {
            limpiarPrecioActual(fila);
            return;
        }

        const precioActualInput = fila.querySelector(".precio-actual-input");

        try {
            const params = new URLSearchParams({
                id: productoId
            });

            const empresaId = obtenerEmpresaId();

            if (empresaId) {
                params.append("empresaId", empresaId);
            }

            const response = await fetch(
                `/Compra/ObtenerProducto?${params.toString()}`,
                {
                    method: "GET",
                    headers: {
                        Accept: "application/json"
                    }
                }
            );

            if (!response.ok) {
                throw new Error("No fue posible obtener el producto.");
            }

            const producto = await response.json();

            if (precioActualInput) {
                precioActualInput.value = producto.precioVenta;
            }

            actualizarAdvertencias(fila);
        }
        catch {
            limpiarPrecioActual(fila);

            alert(
                "No fue posible obtener la información actual del producto."
            );
        }
    };

    const actualizarAdvertencias = fila => {
        const costoInput = fila.querySelector(".costo-input");
        const precioActualInput = fila.querySelector(".precio-actual-input");
        const nuevoPrecioInput = fila.querySelector(".nuevo-precio-input");
        const ventaWarning = fila.querySelector(".venta-warning");

        if (!ventaWarning) {
            return;
        }

        ventaWarning.classList.add("d-none");
        ventaWarning.textContent = "";

        const costo = Number(costoInput?.value);
        const precioActual = Number(precioActualInput?.value);
        const nuevoPrecioTexto = nuevoPrecioInput?.value;

        if (!nuevoPrecioTexto) {
            return;
        }

        const nuevoPrecio = Number(nuevoPrecioTexto);

        if (Number.isNaN(nuevoPrecio)) {
            return;
        }

        const advertencias = [];

        if (!Number.isNaN(precioActual) &&
            nuevoPrecio < precioActual) {
            advertencias.push(
                `El nuevo precio es menor al precio actual (${formatearMoneda(precioActual)}).`
            );
        }

        if (!Number.isNaN(costo) &&
            nuevoPrecio < costo) {
            advertencias.push(
                `El nuevo precio es menor al costo de compra (${formatearMoneda(costo)}).`
            );
        }

        if (advertencias.length === 0) {
            return;
        }

        ventaWarning.innerHTML = advertencias
            .map(texto => `<div>⚠ ${texto}</div>`)
            .join("");

        ventaWarning.classList.remove("d-none");
    };

    const configurarFila = fila => {
        const productoSelect = fila.querySelector(".producto-select");
        const cantidadInput = fila.querySelector(".cantidad-input");
        const costoInput = fila.querySelector(".costo-input");
        const nuevoPrecioInput = fila.querySelector(".nuevo-precio-input");
        const eliminarBtn = fila.querySelector(".eliminar-linea-btn");

        productoSelect?.addEventListener("change", async () => {
            const productoId = productoSelect.value;

            if (!productoId) {
                limpiarPrecioActual(fila);
                return;
            }

            if (productoYaSeleccionado(productoId, fila)) {
                productoSelect.value = "";

                limpiarPrecioActual(fila);

                alert(
                    "Ese producto ya fue agregado a la compra."
                );

                return;
            }

            await consultarProducto(fila, productoId);
        });

        cantidadInput?.addEventListener("input", () => {
            recalcularTotal();
        });

        costoInput?.addEventListener("input", () => {
            recalcularTotal();
            actualizarAdvertencias(fila);
        });

        nuevoPrecioInput?.addEventListener("input", () => {
            actualizarAdvertencias(fila);
        });

        eliminarBtn?.addEventListener("click", () => {
            fila.remove();

            renumerarFilas();
            recalcularTotal();
            actualizarEstadoVacio();
        });
    };

    const agregarFila = () => {
        if (empresaSelect && !empresaSelect.value) {
            alert(
                "Debe seleccionar una empresa antes de agregar productos."
            );

            return;
        }

        const fragment = detalleTemplate.content.cloneNode(true);
        const fila = fragment.querySelector(".compra-detalle-row");

        configurarFila(fila);

        detalleBody.appendChild(fragment);

        renumerarFilas();
        recalcularTotal();
        actualizarEstadoVacio();
    };

    agregarProductoBtn.addEventListener("click", agregarFila);

    empresaSelect?.addEventListener("change", () => {
        if (obtenerFilas().length === 0) {
            return;
        }

        const confirmar = confirm(
            "Al cambiar de empresa se eliminarán los productos agregados. ¿Desea continuar?"
        );

        if (!confirmar) {
            return;
        }

        detalleBody.innerHTML = "";

        recalcularTotal();
        actualizarEstadoVacio();
    });

    compraForm.addEventListener("submit", event => {
        const filas = obtenerFilas();

        if (filas.length === 0) {
            event.preventDefault();

            alert(
                "Debe agregar al menos un producto a la compra."
            );

            return;
        }

        const hayProductoVacio = filas.some(fila => {
            const productoSelect = fila.querySelector(".producto-select");

            return !productoSelect?.value;
        });

        if (hayProductoVacio) {
            event.preventDefault();

            alert(
                "Todas las líneas deben tener un producto seleccionado."
            );

            return;
        }

        renumerarFilas();
    });

    obtenerFilas().forEach(fila => {
        configurarFila(fila);
        actualizarAdvertencias(fila);
    });

    renumerarFilas();
    recalcularTotal();
    actualizarEstadoVacio();
});