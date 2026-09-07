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

    const botonNuevaCategoria =
        document.getElementById("btnNuevaCategoria");
    const modalNuevaCategoria =
        document.getElementById("nuevaCategoriaModal");
    const formularioNuevaCategoria =
        document.getElementById("nuevaCategoriaForm");
    const nombreNuevaCategoria =
        document.getElementById("nuevaCategoriaNombre");
    const errorNuevaCategoria =
        document.getElementById("nuevaCategoriaError");
    const botonGuardarCategoria =
        document.getElementById("btnGuardarCategoria");
    const categoriaSelect =
        document.getElementById("CategoriaId");
    const empresaSelect =
        document.getElementById("EmpresaId");

    if (!botonNuevaCategoria ||
        !modalNuevaCategoria ||
        !formularioNuevaCategoria ||
        !nombreNuevaCategoria ||
        !errorNuevaCategoria ||
        !botonGuardarCategoria ||
        !categoriaSelect) {
        return;
    }

    const instanciaModal =
        bootstrap.Modal.getOrCreateInstance(modalNuevaCategoria);

    botonNuevaCategoria.addEventListener("click", () => {
        formularioNuevaCategoria.reset();
        errorNuevaCategoria.textContent = "";
        instanciaModal.show();
        modalNuevaCategoria.addEventListener(
            "shown.bs.modal",
            () => nombreNuevaCategoria.focus(),
            { once: true });
    });

    formularioNuevaCategoria.addEventListener("submit", async evento => {
        evento.preventDefault();

        if (!formularioNuevaCategoria.reportValidity()) {
            return;
        }

        if (empresaSelect && !empresaSelect.value) {
            errorNuevaCategoria.textContent =
                "Seleccioná una empresa antes de crear la categoría.";
            return;
        }

        botonGuardarCategoria.disabled = true;
        botonGuardarCategoria.textContent = "Creando...";
        errorNuevaCategoria.textContent = "";

        try {
            const datos = new FormData(formularioNuevaCategoria);

            if (empresaSelect) {
                datos.append("empresaId", empresaSelect.value);
            }

            const token = formulario.querySelector(
                'input[name="__RequestVerificationToken"]')?.value;
            const respuesta = await fetch(
                botonNuevaCategoria.dataset.crearUrl,
                {
                    method: "POST",
                    headers: token
                        ? { "RequestVerificationToken": token }
                        : {},
                    body: datos
                });
            const resultado = await respuesta.json();

            if (!respuesta.ok) {
                throw new Error(
                    resultado.mensaje ||
                    "No se pudo crear la categoría.");
            }

            const opcion = new Option(
                resultado.nombre,
                resultado.id,
                true,
                true);
            categoriaSelect.add(opcion);
            categoriaSelect.dispatchEvent(new Event("change"));
            instanciaModal.hide();
        }
        catch (error) {
            errorNuevaCategoria.textContent = error.message;
        }
        finally {
            botonGuardarCategoria.disabled = false;
            botonGuardarCategoria.textContent = "Crear categoría";
        }
    });
});
