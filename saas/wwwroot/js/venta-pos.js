document.addEventListener("DOMContentLoaded", () => {

    const puntoVenta =
        document.getElementById("puntoVenta");

    const empresaId =
        Number(puntoVenta.dataset.empresaId);

    const buscarProductosUrl =
        puntoVenta.dataset.buscarProductosUrl;

    const buscarClientesUrl =
        puntoVenta.dataset.buscarClientesUrl;

    const ventasIndexUrl =
        puntoVenta.dataset.ventasIndexUrl;

    const getCajasUrl =
        puntoVenta.dataset.getCajasUrl;

    const getMediosUrl =
        puntoVenta.dataset.getMediosUrl;

    const btnVolverVentas =
        document.getElementById("btnVolverVentas");

    const formVenta =
        document.getElementById("formVenta");

    const inputBuscarProducto =
        document.getElementById("buscarProducto");

    const btnBuscarProducto =
        document.getElementById("btnBuscarProducto");

    const resultadosProductos =
        document.getElementById("resultadosProductos");

    const btnBuscarCliente =
        document.getElementById("btnBuscarCliente");

    const panelBusquedaCliente =
        document.getElementById("panelBusquedaCliente");

    const inputBuscarCliente =
        document.getElementById("buscarCliente");

    const resultadosClientes =
        document.getElementById("resultadosClientes");

    const btnQuitarCliente =
        document.getElementById("btnQuitarCliente");

    const inputClienteId =
        document.getElementById("clienteId");

    const inputClienteNombre =
        document.getElementById("clienteNombre");

    const clienteNombreVisible =
        document.getElementById("clienteNombreVisible");

    const clienteDetalleVisible =
        document.getElementById("clienteDetalleVisible");

    const carritoVenta =
        document.getElementById("carritoVenta");

    const totalLineas =
        document.getElementById("totalLineas");

    const totalUnidades =
        document.getElementById("totalUnidades");

    const totalVenta =
        document.getElementById("totalVenta");

    const btnConfirmarVenta =
        document.getElementById("btnConfirmarVenta");

    const btnCancelarVenta =
        document.getElementById("btnCancelarVenta");

    // PAGOS

    const pagosContainer =
        document.getElementById("pagosContainer");

    const btnAgregarPago =
        document.getElementById("btnAgregarPago");

    const totalPagadoVisual =
        document.getElementById("totalPagado");

    const saldoPendienteVisual =
        document.getElementById("saldoPendiente");

    const alertaClientePendiente =
        document.getElementById("alertaClientePendiente");

    const carrito = [];

    let temporizadorProductos;
    let temporizadorClientes;
    let productosEncontrados = [];
    let indiceProductoSeleccionado = -1;

    const formatoMoneda = new Intl.NumberFormat("es-AR", {
        style: "currency",
        currency: "ARS",
        minimumFractionDigits: 2
    });

    inicializarCarritoDesdeVista();
    actualizarClienteVisible();
    renderizarCarrito();

    function obtenerTotalVentaNumerico() {

        return carrito.reduce(
            (total, detalle) =>
                total +
                (detalle.precioUnitario * detalle.cantidad),
            0
        );
    }

    function obtenerTotalPagado() {

        let total = 0;

        document
            .querySelectorAll(".pago-importe")
            .forEach(input => {

                const valor =
                    parseFloat(input.value);

                if (Number.isFinite(valor)) {
                    total += valor;
                }
            });

        return total;
    }
    function obtenerSaldoRestante() {

        const totalVenta =
            obtenerTotalVentaNumerico();

        const totalPagado =
            obtenerTotalPagado();

        return Math.max(
            0,
            totalVenta - totalPagado);
    }

    function actualizarResumenPagos() {

        const totalVenta =
            obtenerTotalVentaNumerico();

        const totalPagado =
            obtenerTotalPagado();

        const saldo =
            Math.max(
                0,
                totalVenta - totalPagado);

        totalPagadoVisual.textContent =
            totalPagado.toLocaleString(
                "es-AR",
                {
                    style: "currency",
                    currency: "ARS"
                });

        saldoPendienteVisual.textContent =
            saldo.toLocaleString(
                "es-AR",
                {
                    style: "currency",
                    currency: "ARS"
                });

        const clienteId =
            document.getElementById("clienteId")?.value;

        if (saldo > 0 &&
            !clienteId) {

            alertaClientePendiente.classList.remove(
                "d-none");
        }
        else {

            alertaClientePendiente.classList.add(
                "d-none");
        }
    }

    async function cargarCajasPorMedioPago(
        medioSelect,
        cajaSelect) {

        const medioPagoId =
            medioSelect.value;

        cajaSelect.innerHTML =
            '<option value="">Caja</option>';

        if (!medioPagoId) {
            return;
        }

        const url =
            getCajasUrl
            + "?medioPagoId="
            + encodeURIComponent(medioPagoId)
            + "&empresaId="
            + encodeURIComponent(empresaId);

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

    function reindexarPagos() {

        const pagos =
            pagosContainer
                .querySelectorAll(".pago-item");

        pagos.forEach((pago, index) => {

            pago.dataset.index =
                index;

            const medio =
                pago.querySelector(
                    ".medio-pago-select");

            const caja =
                pago.querySelector(
                    ".caja-pago-select");

            const importe =
                pago.querySelector(
                    ".pago-importe");

            medio.name =
                `Pagos[${index}].MedioPagoId`;

            caja.name =
                `Pagos[${index}].CajaId`;

            importe.name =
                `Pagos[${index}].Importe`;
        });
    }

    function crearPago() {

        const saldoRestante =
            obtenerSaldoRestante();

        if (saldoRestante <= 0) {
            return;
        }

        const index =
            pagosContainer
                .querySelectorAll(
                    ".pago-item")
                .length;

        const pago =
            document.createElement("div");

        pago.className =
            "pago-item bg-white text-dark rounded p-2";

        pago.dataset.index =
            index;

        pago.innerHTML = `
        <div class="row g-2">

            <div class="col-12">

                <select
                    name="Pagos[${index}].MedioPagoId"
                    class="form-select form-select-sm medio-pago-select">

                    <option value="">
                        Medio de pago
                    </option>

                    ${window.veltikaMediosPagoOptions ?? ""}
                </select>

            </div>

            <div class="col-12">

                <select
                    name="Pagos[${index}].CajaId"
                    class="form-select form-select-sm caja-pago-select">

                    <option value="">
                        Caja
                    </option>

                </select>

            </div>

            <div class="col-9">

                <input
                    name="Pagos[${index}].Importe"
                    type="number"
                    min="0.01"
                    step="0.01"
                    class="form-control form-control-sm pago-importe"
                    placeholder="Importe" />

            </div>

            <div class="col-12 bloque-efectivo d-none">

                <div class="row g-2">

                    <div class="col-6">

                        <label class="form-label small mb-1">
                            Recibido
                        </label>

                        <input type="number"
                               min="0"
                               step="0.01"
                               class="form-control form-control-sm efectivo-recibido"
                               placeholder="0,00" />

                    </div>

                    <div class="col-6">

                        <label class="form-label small mb-1">
                            Vuelto
                        </label>

                        <div class="form-control form-control-sm bg-light efectivo-vuelto">
                            $0,00
                        </div>

                    </div>

                </div>

            </div>

            <div class="col-3 d-grid">

                <button
                    type="button"
                    class="btn btn-sm btn-outline-danger btn-eliminar-pago">
                    ×
                </button>

            </div>

        </div>
    `;

        pagosContainer.appendChild(
            pago);

        configurarPago(
            pago);

        const importeInput =
            pago.querySelector(
                ".pago-importe");

        if (importeInput) {

            importeInput.value =
                saldoRestante.toFixed(2);
        }

        actualizarResumenPagos();
    }

    function configurarPago(pago) {

        const medioSelect =
            pago.querySelector(
                ".medio-pago-select");

        const cajaSelect =
            pago.querySelector(
                ".caja-pago-select");

        const importe =
            pago.querySelector(
                ".pago-importe");

        const eliminar =
            pago.querySelector(
                ".btn-eliminar-pago");

        const bloqueEfectivo =
            pago.querySelector(".bloque-efectivo");

        const recibidoInput =
            pago.querySelector(".efectivo-recibido");

        medioSelect.addEventListener(
            "change",
            function () {

                cargarCajasPorMedioPago(
                    medioSelect,
                    cajaSelect);

                actualizarBloqueEfectivo(
                    pago);
            });

        importe.addEventListener(
            "input",
            function () {

                actualizarResumenPagos();
                actualizarVuelto(pago);
            });

        if (recibidoInput) {

            recibidoInput.addEventListener(
                "input",
                function () {

                    actualizarVuelto(pago);
                });
        }

        eliminar.addEventListener(
            "click",
            function () {

                pago.remove();

                reindexarPagos();
                actualizarResumenPagos();
            });

        actualizarBloqueEfectivo(
            pago);
    }

    function inicializarCarritoDesdeVista() {
        const filas = carritoVenta.querySelectorAll("tr[data-producto-id]");

        filas.forEach(fila => {
            const productoId = Number(fila.dataset.productoId);

            const productoNombre =
                fila.querySelector('input[name$=".ProductoNombre"]')?.value ?? "";

            const codigoBarra =
                fila.querySelector('input[name$=".CodigoBarra"]')?.value ?? null;

            const precioUnitario = convertirDecimal(
                fila.querySelector('input[name$=".PrecioUnitario"]')?.value
            );

            const cantidad = Number(
                fila.querySelector('input[name$=".Cantidad"]')?.value ?? 1
            );

            const stockDisponible = Number(
                fila.querySelector('input[name$=".StockDisponible"]')?.value ?? 0
            );

            if (productoId > 0) {
                carrito.push({
                    productoId,
                    productoNombre,
                    codigoBarra,
                    precioUnitario,
                    cantidad,
                    stockDisponible
                });
            }
        });
    }

    function convertirDecimal(valor) {
        if (!valor) {
            return 0;
        }

        return Number(
            valor
                .toString()
                .replace(/\./g, "")
                .replace(",", ".")
        );
    }

    function renderizarCarrito() {
        carritoVenta.innerHTML = "";

        if (carrito.length === 0) {
            carritoVenta.appendChild(
                crearFilaCarritoVacio());

            actualizarResumen();
            actualizarResumenPagos();

            return;
        }

        carrito.forEach((detalle, indice) => {
            carritoVenta.appendChild(crearFilaProducto(detalle, indice));
        });

        actualizarResumen();
        actualizarResumenPagos();
    }

    function crearFilaCarritoVacio() {
        const fila = document.createElement("tr");
        fila.id = "carritoVacio";

        const celda = document.createElement("td");
        celda.colSpan = 6;
        celda.className = "text-center py-5";

        const icono = document.createElement("span");
        icono.className = "material-symbols-outlined text-muted mb-2";
        icono.style.fontSize = "42px";
        icono.textContent = "shopping_cart";

        const titulo = document.createElement("div");
        titulo.className = "fw-medium text-dark";
        titulo.textContent = "El carrito está vacío";

        const descripcion = document.createElement("div");
        descripcion.className = "small text-muted";
        descripcion.textContent =
            "Busque o escanee un producto para comenzar la venta.";

        celda.append(icono, titulo, descripcion);
        fila.appendChild(celda);

        return fila;
    }

    function crearFilaProducto(detalle, indice) {
        const stockSuficiente =
            detalle.cantidad <= detalle.stockDisponible;

        const subtotal =
            detalle.precioUnitario * detalle.cantidad;

        const fila = document.createElement("tr");
        fila.dataset.productoId = detalle.productoId;

        if (!stockSuficiente) {
            fila.classList.add("row-error");
        }

        fila.appendChild(
            crearCeldaProducto(detalle, indice, stockSuficiente)
        );

        fila.appendChild(
            crearCeldaTexto(
                formatoMoneda.format(detalle.precioUnitario),
                "text-end fw-medium small"
            )
        );

        fila.appendChild(
            crearCeldaCantidad(detalle, indice)
        );

        fila.appendChild(
            crearCeldaTexto(
                detalle.stockDisponible.toString(),
                "text-center small"
            )
        );

        fila.appendChild(
            crearCeldaTexto(
                formatoMoneda.format(subtotal),
                `text-end fw-semibold small ${stockSuficiente ? "" : "text-danger"
                }`
            )
        );

        fila.appendChild(
            crearCeldaEliminar(indice)
        );

        return fila;
    }

    function crearCeldaProducto(detalle, indice, stockSuficiente) {
        const celda = document.createElement("td");
        celda.className = "ps-3";

        celda.appendChild(
            crearInputOculto(
                `Detalles[${indice}].ProductoId`,
                detalle.productoId
            )
        );

        const encabezado = document.createElement("div");
        encabezado.className = "d-flex align-items-center gap-1";

        if (!stockSuficiente) {
            const advertencia = document.createElement("span");
            advertencia.className =
                "material-symbols-outlined text-danger";
            advertencia.style.fontSize = "16px";
            advertencia.textContent = "warning";

            encabezado.appendChild(advertencia);
        }

        const nombre = document.createElement("div");
        nombre.className = "fw-medium text-dark small";
        nombre.textContent = detalle.productoNombre;

        encabezado.appendChild(nombre);

        const codigo = document.createElement("div");
        codigo.className = `${stockSuficiente ? "text-muted" : "text-danger"
            } font-monospace`;

        codigo.style.fontSize = "0.7rem";
        codigo.textContent = detalle.codigoBarra
            ? `Código: ${detalle.codigoBarra}`
            : "Sin código de barras";

        celda.append(encabezado, codigo);

        if (!stockSuficiente) {
            const errorStock = document.createElement("div");
            errorStock.className = "text-danger mt-1";
            errorStock.style.fontSize = "0.7rem";
            errorStock.textContent =
                `Stock disponible: ${detalle.stockDisponible} ` +
                `(Solicitado: ${detalle.cantidad})`;

            celda.appendChild(errorStock);
        }

        return celda;
    }

    function crearCeldaCantidad(detalle, indice) {
        const celda = document.createElement("td");
        celda.className = "text-center";

        const grupo = document.createElement("div");
        grupo.className = "qty-group";

        const btnDisminuir = document.createElement("button");
        btnDisminuir.type = "button";
        btnDisminuir.className =
            "btn btn-link text-muted btn-quantity btn-disminuir";
        btnDisminuir.dataset.indice = indice;
        btnDisminuir.setAttribute(
            "aria-label",
            `Disminuir cantidad de ${detalle.productoNombre}`
        );

        const iconoDisminuir = document.createElement("span");
        iconoDisminuir.className = "material-symbols-outlined";
        iconoDisminuir.style.fontSize = "16px";
        iconoDisminuir.textContent = "remove";

        btnDisminuir.appendChild(iconoDisminuir);

        const inputCantidad = document.createElement("input");
        inputCantidad.type = "number";
        inputCantidad.min = "1";
        inputCantidad.max = detalle.stockDisponible.toString();
        inputCantidad.value = detalle.cantidad;
        inputCantidad.name = `Detalles[${indice}].Cantidad`;
        inputCantidad.className =
            "input-quantity small fw-medium border-start border-end";
        inputCantidad.dataset.indice = indice;
        inputCantidad.setAttribute(
            "aria-label",
            `Cantidad de ${detalle.productoNombre}`
        );

        const btnAumentar = document.createElement("button");
        btnAumentar.type = "button";
        btnAumentar.className =
            "btn btn-link text-muted btn-quantity btn-aumentar";
        btnAumentar.dataset.indice = indice;
        btnAumentar.setAttribute(
            "aria-label",
            `Aumentar cantidad de ${detalle.productoNombre}`
        );

        const iconoAumentar = document.createElement("span");
        iconoAumentar.className = "material-symbols-outlined";
        iconoAumentar.style.fontSize = "16px";
        iconoAumentar.textContent = "add";

        btnAumentar.appendChild(iconoAumentar);

        grupo.append(
            btnDisminuir,
            inputCantidad,
            btnAumentar
        );

        celda.appendChild(grupo);

        return celda;
    }

    function crearCeldaEliminar(indice) {
        const celda = document.createElement("td");
        celda.className = "pe-3 text-center";

        const boton = document.createElement("button");
        boton.type = "button";
        boton.className =
            "btn btn-link text-danger p-1 btn-eliminar-producto";
        boton.dataset.indice = indice;
        boton.title = "Quitar producto";
        boton.setAttribute("aria-label", "Quitar producto");

        const icono = document.createElement("span");
        icono.className = "material-symbols-outlined";
        icono.textContent = "delete";

        boton.appendChild(icono);
        celda.appendChild(boton);

        return celda;
    }

    function crearCeldaTexto(texto, clases) {
        const celda = document.createElement("td");
        celda.className = clases;
        celda.textContent = texto;

        return celda;
    }

    function crearInputOculto(nombre, valor) {
        const input = document.createElement("input");
        input.type = "hidden";
        input.name = nombre;
        input.value = valor;

        return input;
    }

    function agregarProducto(producto) {
        const productoExistente = carrito.find(
            item => item.productoId === Number(producto.id)
        );

        if (productoExistente) {
            if (
                productoExistente.cantidad + 1 >
                productoExistente.stockDisponible
            ) {
                mostrarMensaje(
                    `No hay stock suficiente de "${productoExistente.productoNombre}". ` +
                    `Stock disponible: ${productoExistente.stockDisponible}.`,
                    "warning"
                );

                return;
            }

            productoExistente.cantidad++;
        } else {
            const stockDisponible = Number(producto.stockDisponible);

            if (stockDisponible <= 0) {
                mostrarMensaje(
                    `El producto "${producto.nombre}" no tiene stock disponible.`,
                    "warning"
                );

                return;
            }

            carrito.push({
                productoId: Number(producto.id),
                productoNombre: producto.nombre,
                codigoBarra: producto.codigoBarra ?? null,
                precioUnitario: Number(producto.precioVenta),
                cantidad: 1,
                stockDisponible
            });
        }

        inputBuscarProducto.value = "";
        ocultarResultadosProductos();
        renderizarCarrito();
        inputBuscarProducto.focus();
    }

    function aumentarCantidad(indice) {
        const detalle = carrito[indice];

        if (!detalle) {
            return;
        }

        if (detalle.cantidad + 1 > detalle.stockDisponible) {
            mostrarMensaje(
                `No hay stock suficiente de "${detalle.productoNombre}". ` +
                `Stock disponible: ${detalle.stockDisponible}.`,
                "warning"
            );

            return;
        }

        detalle.cantidad++;
        renderizarCarrito();
    }

    function disminuirCantidad(indice) {
        const detalle = carrito[indice];

        if (!detalle) {
            return;
        }

        if (detalle.cantidad === 1) {
            eliminarProducto(indice);
            return;
        }

        detalle.cantidad--;
        renderizarCarrito();
    }

    function modificarCantidad(indice, nuevaCantidad) {
        const detalle = carrito[indice];

        if (!detalle) {
            return;
        }

        if (!Number.isInteger(nuevaCantidad) || nuevaCantidad < 1) {
            mostrarMensaje(
                "La cantidad debe ser un número entero mayor a cero.",
                "warning"
            );

            renderizarCarrito();
            return;
        }

        if (nuevaCantidad > detalle.stockDisponible) {
            mostrarMensaje(
                `No hay stock suficiente de "${detalle.productoNombre}". ` +
                `Stock disponible: ${detalle.stockDisponible}.`,
                "warning"
            );

            renderizarCarrito();
            return;
        }

        detalle.cantidad = nuevaCantidad;
        renderizarCarrito();
    }

    function eliminarProducto(indice) {
        if (!carrito[indice]) {
            return;
        }

        carrito.splice(indice, 1);
        renderizarCarrito();
    }

    function actualizarResumen() {
        const cantidadLineas = carrito.length;

        const cantidadUnidades = carrito.reduce(
            (acumulado, detalle) =>
                acumulado + detalle.cantidad,
            0
        );

        const importeTotal = carrito.reduce(
            (acumulado, detalle) =>
                acumulado +
                detalle.precioUnitario * detalle.cantidad,
            0
        );

        const stockValido = carrito.every(
            detalle =>
                detalle.cantidad > 0 &&
                detalle.cantidad <= detalle.stockDisponible
        );

        totalLineas.textContent = cantidadLineas;
        totalUnidades.textContent = cantidadUnidades;
        totalVenta.textContent = formatoMoneda.format(importeTotal);

        btnConfirmarVenta.disabled =
            cantidadLineas === 0 || !stockValido;
    }

    async function buscarProductos() {
        const termino = inputBuscarProducto.value.trim();

        if (!termino) {
            ocultarResultadosProductos();
            return [];
        }

        try {
            const url = new URL(
                buscarProductosUrl,
                window.location.origin
            );

            url.searchParams.set("termino", termino);

            if (empresaId > 0) {
                url.searchParams.set(
                    "empresaId",
                    empresaId.toString()
                );
            }

            const respuesta = await fetch(url, {
                headers: {
                    "X-Requested-With": "XMLHttpRequest"
                }
            });

            if (!respuesta.ok) {
                throw new Error(
                    "No fue posible buscar los productos."
                );
            }

            const productos = await respuesta.json();

            mostrarResultadosProductos(productos);
            return productos;
        } catch (error) {
            ocultarResultadosProductos();
            mostrarMensaje(error.message, "danger");
            return [];
        }
    }

    function mostrarResultadosProductos(productos) {
        resultadosProductos.innerHTML = "";
        productosEncontrados = Array.isArray(productos) ? productos : [];
        indiceProductoSeleccionado = productosEncontrados.length > 0 ? 0 : -1;

        if (!Array.isArray(productos) || productos.length === 0) {
            const item = document.createElement("div");
            item.className =
                "list-group-item small text-muted";
            item.textContent =
                "No se encontraron productos activos.";

            resultadosProductos.appendChild(item);
            resultadosProductos.classList.remove("d-none");
            return;
        }

        productosEncontrados.forEach(producto => {
            const boton = document.createElement("button");
            boton.type = "button";
            boton.className =
                "list-group-item list-group-item-action";

            const contenido = document.createElement("div");
            contenido.className =
                "d-flex justify-content-between align-items-start gap-3";

            const datos = document.createElement("div");

            const nombre = document.createElement("div");
            nombre.className = "fw-medium small";
            nombre.textContent = producto.nombre;

            const detalle = document.createElement("div");
            detalle.className = "text-muted";
            detalle.style.fontSize = "0.75rem";
            detalle.textContent = producto.codigoBarra
                ? `Código: ${producto.codigoBarra}`
                : "Sin código de barras";

            datos.append(nombre, detalle);

            const valores = document.createElement("div");
            valores.className = "text-end";

            const precio = document.createElement("div");
            precio.className = "fw-semibold small";
            precio.textContent =
                formatoMoneda.format(producto.precioVenta);

            const stock = document.createElement("div");
            stock.className =
                Number(producto.stockDisponible) > 0
                    ? "text-muted"
                    : "text-danger";

            stock.style.fontSize = "0.75rem";
            stock.textContent =
                `Stock: ${producto.stockDisponible}`;

            valores.append(precio, stock);
            contenido.append(datos, valores);
            boton.appendChild(contenido);

            boton.addEventListener("click", () => {
                agregarProducto(producto);
            });

            resultadosProductos.appendChild(boton);
        });

        resultadosProductos.classList.remove("d-none");
        actualizarProductoSeleccionado();
    }

    function actualizarProductoSeleccionado() {
        const botones = resultadosProductos.querySelectorAll("button");

        botones.forEach((boton, indice) => {
            const seleccionado = indice === indiceProductoSeleccionado;
            boton.classList.toggle("active", seleccionado);
        });
    }

    function moverSeleccionProducto(direccion) {
        if (productosEncontrados.length === 0) {
            return;
        }

        indiceProductoSeleccionado += direccion;

        if (indiceProductoSeleccionado < 0) {
            indiceProductoSeleccionado = productosEncontrados.length - 1;
        }

        if (indiceProductoSeleccionado >= productosEncontrados.length) {
            indiceProductoSeleccionado = 0;
        }

        actualizarProductoSeleccionado();

        const botonSeleccionado = resultadosProductos.querySelector(".active");
        botonSeleccionado?.scrollIntoView({ block: "nearest" });
    }

    function agregarProductoSeleccionado() {
        const producto = productosEncontrados[indiceProductoSeleccionado];

        if (!producto) {
            return false;
        }

        agregarProducto(producto);
        return true;
    }

    function ocultarResultadosProductos() {
        productosEncontrados = [];
        indiceProductoSeleccionado = -1;
        resultadosProductos.innerHTML = "";
        resultadosProductos.classList.add("d-none");
    }

    async function buscarClientes() {
        const termino = inputBuscarCliente.value.trim();

        if (!termino) {
            ocultarResultadosClientes();
            return;
        }

        try {
            const url = new URL(
                buscarClientesUrl,
                window.location.origin
            );

            url.searchParams.set("termino", termino);

            if (empresaId > 0) {
                url.searchParams.set(
                    "empresaId",
                    empresaId.toString()
                );
            }

            const respuesta = await fetch(url, {
                headers: {
                    "X-Requested-With": "XMLHttpRequest"
                }
            });

            if (!respuesta.ok) {
                throw new Error(
                    "No fue posible buscar los clientes."
                );
            }

            const clientes = await respuesta.json();

            mostrarResultadosClientes(clientes);
        } catch (error) {
            ocultarResultadosClientes();
            mostrarMensaje(error.message, "danger");
        }
    }

    function mostrarResultadosClientes(clientes) {
        resultadosClientes.innerHTML = "";

        if (!Array.isArray(clientes) || clientes.length === 0) {
            const item = document.createElement("div");
            item.className =
                "list-group-item small text-muted";
            item.textContent =
                "No se encontraron clientes activos.";

            resultadosClientes.appendChild(item);
            resultadosClientes.classList.remove("d-none");
            return;
        }

        clientes.forEach(cliente => {
            const boton = document.createElement("button");
            boton.type = "button";
            boton.className =
                "list-group-item list-group-item-action";

            const nombre = document.createElement("div");
            nombre.className = "fw-medium small";
            nombre.textContent = cliente.nombreCompleto;

            const detalles = [];

            if (cliente.documento) {
                detalles.push(`Documento: ${cliente.documento}`);
            }

            if (cliente.email) {
                detalles.push(cliente.email);
            }

            const descripcion = document.createElement("div");
            descripcion.className = "text-muted";
            descripcion.style.fontSize = "0.75rem";
            descripcion.textContent =
                detalles.length > 0
                    ? detalles.join(" · ")
                    : "Sin datos adicionales";

            boton.append(nombre, descripcion);

            boton.addEventListener("click", () => {
                seleccionarCliente(cliente);
            });

            resultadosClientes.appendChild(boton);
        });

        resultadosClientes.classList.remove("d-none");
    }

    function seleccionarCliente(cliente) {
        inputClienteId.value = cliente.id;
        inputClienteNombre.value = cliente.nombreCompleto;

        clienteNombreVisible.textContent =
            cliente.nombreCompleto;

        const datos = [];

        if (cliente.documento) {
            datos.push(`Documento: ${cliente.documento}`);
        }

        if (cliente.email) {
            datos.push(cliente.email);
        }

        clienteDetalleVisible.textContent =
            datos.length > 0
                ? datos.join(" · ")
                : "Cliente registrado";

        btnQuitarCliente.classList.remove("d-none");
        btnBuscarCliente.innerHTML = "";

        const icono = document.createElement("span");
        icono.className = "material-symbols-outlined";
        icono.style.fontSize = "16px";
        icono.textContent = "edit";

        btnBuscarCliente.append(icono, " Cambiar");

        panelBusquedaCliente.classList.add("d-none");
        inputBuscarCliente.value = "";
        ocultarResultadosClientes();
        inputBuscarProducto.focus();
    }

    function quitarCliente() {
        inputClienteId.value = "";
        inputClienteNombre.value = "Cliente ocasional";

        clienteNombreVisible.textContent =
            "Cliente ocasional";

        clienteDetalleVisible.textContent =
            "Venta sin cliente registrado";

        btnQuitarCliente.classList.add("d-none");

        btnBuscarCliente.innerHTML = "";

        const icono = document.createElement("span");
        icono.className = "material-symbols-outlined";
        icono.style.fontSize = "16px";
        icono.textContent = "search";

        btnBuscarCliente.append(icono, " Buscar cliente");
    }

    function actualizarClienteVisible() {
        const clienteSeleccionado =
            inputClienteId.value.trim() !== "";

        if (clienteSeleccionado) {
            clienteNombreVisible.textContent =
                inputClienteNombre.value || "Cliente registrado";

            clienteDetalleVisible.textContent =
                "Cliente registrado";

            btnQuitarCliente.classList.remove("d-none");
        } else {
            quitarCliente();
        }
    }

    function ocultarResultadosClientes() {
        resultadosClientes.innerHTML = "";
        resultadosClientes.classList.add("d-none");
    }

    function mostrarMensaje(mensaje, tipo) {
        let contenedor =
            document.getElementById("mensajePuntoVenta");

        if (!contenedor) {
            contenedor = document.createElement("div");
            contenedor.id = "mensajePuntoVenta";

            const encabezado =
                puntoVenta.firstElementChild;

            encabezado.insertAdjacentElement(
                "afterend",
                contenedor
            );
        }

        contenedor.className =
            `alert alert-${tipo} alert-dismissible fade show mb-3`;

        contenedor.setAttribute("role", "alert");
        contenedor.textContent = mensaje;

        const cerrar = document.createElement("button");
        cerrar.type = "button";
        cerrar.className = "btn-close";
        cerrar.setAttribute("aria-label", "Cerrar");

        cerrar.addEventListener("click", () => {
            contenedor.remove();
        });

        contenedor.appendChild(cerrar);
    }
    function esMedioEfectivo(medioSelect) {

        const option =
            medioSelect.options[
            medioSelect.selectedIndex
            ];

        if (!option) {
            return false;
        }

        return Number(option.dataset.tipo) === 1;
    }

    function actualizarVuelto(pago) {

        const importeInput =
            pago.querySelector(".pago-importe");

        const recibidoInput =
            pago.querySelector(".efectivo-recibido");

        const vueltoVisual =
            pago.querySelector(".efectivo-vuelto");

        if (!importeInput ||
            !recibidoInput ||
            !vueltoVisual) {
            return;
        }

        const importe =
            parseFloat(importeInput.value) || 0;

        const recibido =
            parseFloat(recibidoInput.value) || 0;

        const vuelto =
            Math.max(
                0,
                recibido - importe);

        vueltoVisual.textContent =
            vuelto.toLocaleString(
                "es-AR",
                {
                    style: "currency",
                    currency: "ARS"
                });
    }

    function actualizarBloqueEfectivo(pago) {

        const medioSelect =
            pago.querySelector(".medio-pago-select");

        const bloque =
            pago.querySelector(".bloque-efectivo");

        const recibidoInput =
            pago.querySelector(".efectivo-recibido");

        const vueltoVisual =
            pago.querySelector(".efectivo-vuelto");

        if (!medioSelect ||
            !bloque) {
            return;
        }

        if (esMedioEfectivo(medioSelect)) {

            bloque.classList.remove("d-none");

            actualizarVuelto(pago);
        }
        else {

            bloque.classList.add("d-none");

            if (recibidoInput) {
                recibidoInput.value = "";
            }

            if (vueltoVisual) {
                vueltoVisual.textContent =
                    "$0,00";
            }
        }
    }

    carritoVenta.addEventListener("click", evento => {
        const btnAumentar =
            evento.target.closest(".btn-aumentar");

        const btnDisminuir =
            evento.target.closest(".btn-disminuir");

        const btnEliminar =
            evento.target.closest(".btn-eliminar-producto");

        if (btnAumentar) {
            aumentarCantidad(
                Number(btnAumentar.dataset.indice)
            );

            return;
        }

        if (btnDisminuir) {
            disminuirCantidad(
                Number(btnDisminuir.dataset.indice)
            );

            return;
        }

        if (btnEliminar) {
            eliminarProducto(
                Number(btnEliminar.dataset.indice)
            );
        }
    });

    carritoVenta.addEventListener("change", evento => {
        if (!evento.target.matches(".input-quantity")) {
            return;
        }

        modificarCantidad(
            Number(evento.target.dataset.indice),
            Number(evento.target.value)
        );
    });

    btnBuscarProducto.addEventListener(
        "click",
        buscarProductos
    );

    inputBuscarProducto.addEventListener("input", () => {
        clearTimeout(temporizadorProductos);
        ocultarResultadosProductos();

        temporizadorProductos = setTimeout(
            buscarProductos,
            300
        );
    });

    inputBuscarProducto.addEventListener("keydown", async evento => {
        if (evento.key === "ArrowDown") {
            evento.preventDefault();
            moverSeleccionProducto(1);
            return;
        }

        if (evento.key === "ArrowUp") {
            evento.preventDefault();
            moverSeleccionProducto(-1);
            return;
        }

        if (evento.key !== "Enter") {
            return;
        }

        evento.preventDefault();
        clearTimeout(temporizadorProductos);

        const termino = inputBuscarProducto.value.trim().toLowerCase();

        if (!termino) {
            return;
        }

        if (resultadosProductos.classList.contains("d-none")) {
            await buscarProductos();
        }

        const coincidenciaExacta = productosEncontrados.find(producto =>
            producto.codigoBarra?.trim().toLowerCase() === termino
        );

        if (coincidenciaExacta) {
            agregarProducto(coincidenciaExacta);
            return;
        }

        agregarProductoSeleccionado();
    });

    btnBuscarCliente.addEventListener("click", () => {
        panelBusquedaCliente.classList.toggle("d-none");

        if (
            !panelBusquedaCliente.classList.contains("d-none")
        ) {
            inputBuscarCliente.focus();
        }
    });

    inputBuscarCliente.addEventListener("input", () => {
        clearTimeout(temporizadorClientes);

        temporizadorClientes = setTimeout(
            buscarClientes,
            300
        );
    });

    inputBuscarCliente.addEventListener(
        "keydown",
        evento => {
            if (evento.key === "Enter") {
                evento.preventDefault();
                buscarClientes();
            }
        }
    );

    btnQuitarCliente.addEventListener(
        "click",
        quitarCliente
    );

    btnCancelarVenta.addEventListener("click", () => {
        if (carrito.length === 0) {
            quitarCliente();
            inputBuscarProducto.focus();
            return;
        }

        const confirmar = window.confirm(
            "¿Está seguro de que desea cancelar la venta actual?"
        );

        if (!confirmar) {
            return;
        }

        carrito.splice(0, carrito.length);
        quitarCliente();
        ocultarResultadosProductos();
        ocultarResultadosClientes();
        renderizarCarrito();
        inputBuscarProducto.value = "";
        inputBuscarProducto.focus();
    });

    btnVolverVentas.addEventListener("click", evento => {
        evento.preventDefault();

        if (carrito.length === 0) {
            window.location.href = ventasIndexUrl;
            return;
        }

        const confirmar = window.confirm(
            "Hay una venta en curso. ¿Está seguro de que desea volver al listado? Se perderán los productos cargados."
        );

        if (!confirmar) return;

        window.location.href = ventasIndexUrl;
    });

    formVenta.addEventListener("formdata", evento => {
        document.querySelectorAll(".pago-importe").forEach(input => {
            if (input.name && input.value) {
                evento.formData.set(input.name, input.value.replace(".", ","));
            }
        });
    });

    formVenta.addEventListener("submit", evento => {
        if (carrito.length === 0) {
            evento.preventDefault();

            mostrarMensaje(
                "Debe agregar al menos un producto a la venta.",
                "warning"
            );

            return;
        }

        const stockInvalido = carrito.some(
            detalle =>
                detalle.cantidad < 1 ||
                detalle.cantidad >
                detalle.stockDisponible
        );

        if (stockInvalido) {
            evento.preventDefault();

            mostrarMensaje(
                "La venta contiene productos con cantidades inválidas.",
                "danger"
            );
        }
    });

    document.addEventListener("keydown", evento => {
        if (evento.key === "F2") {
            evento.preventDefault();
            inputBuscarProducto.focus();
            inputBuscarProducto.select();
        }
    });

    document.addEventListener("click", evento => {
        if (
            !resultadosProductos.contains(evento.target) &&
            evento.target !== inputBuscarProducto &&
            evento.target !== btnBuscarProducto
        ) {
            ocultarResultadosProductos();
        }

        if (
            !resultadosClientes.contains(evento.target) &&
            evento.target !== inputBuscarCliente &&
            !btnBuscarCliente.contains(evento.target)
        ) {
            ocultarResultadosClientes();
        }
    });

    document
        .querySelectorAll(".pago-item")
        .forEach(configurarPago);

    btnAgregarPago.addEventListener(
        "click",
        crearPago);

    actualizarResumenPagos();
    inputBuscarProducto.focus();

});