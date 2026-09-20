// Se carga antes de unobtrusive validation para que los mensajes que no
// provienen de un DataAnnotation con texto propio mantengan el idioma del sitio.
(function ($) {
    "use strict";

    function obtenerEtiqueta(control) {
        const etiqueta = Array.from(
            document.querySelectorAll("label[for]"))
            .find(label =>
                label.htmlFor === control.id ||
                label.htmlFor === control.name);

        return etiqueta?.textContent
            ?.replace(/\s+/g, " ")
            .trim() || "";
    }

    function traducirMensajesGenerados() {
        const mensajes = {
            required: campo => campo
                ? `El campo ${campo} es obligatorio.`
                : "Este campo es obligatorio.",
            number: campo => campo
                ? `El campo ${campo} debe ser numérico.`
                : "Debe ingresar un número válido.",
            range: campo => campo
                ? `El valor ingresado para ${campo} no es válido.`
                : "El valor ingresado no es válido.",
            regex: campo => campo
                ? `El formato de ${campo} no es válido.`
                : "El formato ingresado no es válido."
        };

        Object.entries(mensajes).forEach(([regla, traducir]) => {
            document
                .querySelectorAll(`[data-val-${regla}]`)
                .forEach(control => {
                    const atributo = `data-val-${regla}`;
                    const mensaje = control.getAttribute(atributo);

                    // Solo reemplazamos los textos generados por el framework.
                    // Los mensajes propios en español de cada ViewModel se conservan.
                    if (mensaje?.startsWith("The ")) {
                        control.setAttribute(
                            atributo,
                            traducir(obtenerEtiqueta(control)));
                    }
                });
        });
    }

    $.extend($.validator.messages, {
        required: "Este campo es obligatorio.",
        remote: "Revisá este campo.",
        email: "Ingresá un email válido.",
        url: "Ingresá una URL válida.",
        date: "Ingresá una fecha válida.",
        dateISO: "Ingresá una fecha válida.",
        number: "Ingresá un número válido.",
        digits: "Ingresá solo dígitos.",
        equalTo: "Los valores no coinciden.",
        maxlength: $.validator.format("No puede superar los {0} caracteres."),
        minlength: $.validator.format("Debe tener al menos {0} caracteres."),
        rangelength: $.validator.format("Debe tener entre {0} y {1} caracteres."),
        range: $.validator.format("Debe estar entre {0} y {1}."),
        max: $.validator.format("Debe ser menor o igual a {0}."),
        min: $.validator.format("Debe ser mayor o igual a {0}.")
    });

    traducirMensajesGenerados();

    // La validación nativa del navegador no respeta los mensajes del servidor
    // ni los de jQuery. Se conserva la validación unobtrusive ya usada por Veltika.
    document.querySelectorAll("form").forEach(formulario => {
        formulario.noValidate = true;
    });
})(jQuery);
