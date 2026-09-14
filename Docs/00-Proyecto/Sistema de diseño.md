# Sistema de Diseño Veltika

Versión: 1.0

Última actualización: 13/09/2026

## 1. Objetivo

Este documento define la base visual reutilizable de Veltika. El Manual de Marca Veltika v1.0 es la fuente de verdad para identidad, paleta, tipografía y personalidad. Cuando el manual no cubra una necesidad propia del producto digital, la extensión debe documentarse aquí y reutilizarse en todos los componentes equivalentes.

## 2. Principios

- Reutilizar tokens y componentes antes de crear estilos particulares.
- Mantener las excepciones al mínimo y documentar su motivo.
- No repetir valores oficiales como literales cuando exista un token.
- Migrar de manera progresiva y validar los consumidores en cada etapa.
- No eliminar variables o clases antiguas sin confirmar que dejaron de utilizarse.
- Mantener una interfaz simple, confiable, cercana, profesional y práctica.

## 3. Fuente técnica

Los tokens se definen exclusivamente en `saas/wwwroot/css/design-tokens.css`.

Los nombres oficiales utilizan el prefijo `--veltika-`. Las familias antiguas, como `--ui-*`, `--app-*`, `--v-*` y `--vt-*`, se consideran compatibilidad temporal y deben migrarse progresivamente; no deben utilizarse como referencia para componentes nuevos.

## 4. Tokens oficiales

### Identidad

- `--veltika-primary`: acción y confianza.
- `--veltika-dark`: estabilidad y profundidad.
- `--veltika-accent`: dinamismo y foco.
- `--veltika-bg`: fondo general claro.
- `--veltika-surface`: superficies y contenedores.
- `--veltika-text`: texto principal.
- `--veltika-muted`: texto secundario.
- `--veltika-border`: separación suave.
- `--veltika-font`: Inter con respaldo sans-serif.

### Estados semánticos

- `--veltika-success`: resultados y confirmaciones correctas.
- `--veltika-warning`: advertencias que requieren atención.
- `--veltika-danger`: errores y acciones destructivas.
- `--veltika-info`: información contextual.
- `--veltika-income`: importes o movimientos de ingreso.
- `--veltika-expense`: importes o movimientos de egreso.
- `--veltika-disabled`: controles y estados deshabilitados.

Los colores semánticos son extensiones del manual para el producto digital. Sus valores son centrales y no deben redefinirse por módulo.

### Escalas compartidas

- Espaciado: `--veltika-space-1`, `--veltika-space-2`, `--veltika-space-3`, `--veltika-space-4`, `--veltika-space-6` y `--veltika-space-8`.
- Radios: `--veltika-radius-sm`, `--veltika-radius-md`, `--veltika-radius` y `--veltika-radius-card`.
- Sombras: `--veltika-shadow-sm` y `--veltika-shadow-md`.
- Iconos: `--veltika-icon-xs`, `--veltika-icon-sm`, `--veltika-icon-md` y `--veltika-icon-lg`.
- Controles: `--veltika-control-height`.
- Transiciones: `--veltika-transition`.

## 5. Uso en componentes

Cada familia reutilizable debe conservar una responsabilidad clara: botones, formularios, filtros, tarjetas, tablas, alertas, tipografía y guías. Los componentes nuevos deben consumir tokens oficiales y evitar estilos inline, selectores innecesariamente específicos o variantes duplicadas.

Un componente no debe depender de una sobrescritura accidental causada por el orden de carga de los archivos CSS.

## 6. Cómo extender el sistema

Antes de incorporar una nueva decisión visual:

1. Comprobar si ya existe un token o componente equivalente.
2. Confirmar que la necesidad se repite o tiene valor transversal.
3. Si falta un token, agregarlo a `design-tokens.css` con un nombre semántico.
4. Documentar aquí su propósito y alcance.
5. Aplicarlo primero en una pantalla piloto.
6. Validar desktop, móvil, zoom, foco y contraste.
7. Migrar otros consumidores de forma progresiva.
8. Eliminar estilos reemplazados únicamente después de verificar que no tengan uso.

## 7. Estado de la migración

La base oficial está disponible, pero las variables históricas todavía poseen consumidores. Durante la transición deben coexistir sin alterar el aspecto actual. Cada bloque posterior deberá indicar qué componente migra, qué consumidores fueron revisados y qué compatibilidad continúa pendiente.

## 8. Tarjetas canónicas

Cada contenedor debe utilizar la clase correspondiente a su responsabilidad:

- `form-card`: formularios y controles de entrada.
- `detail-card`: información de una entidad u operación.
- `content-card`: paneles de contenido general; su encabezado reutilizable utiliza `content-card__header`.
- `filter-card`: filtros de listados y reportes.
- `table-card`: tablas y sus estados vacíos.
- `metric-card`: indicadores y valores resumidos.
- `metric-card--featured`: variante destacada con icono, etiqueta, valor y detalle.
- `info-callout`: aviso informativo integrado al contenido, sin apariencia de alerta crítica.

Las variantes semánticas se agregan al componente base, por ejemplo `detail-card detail-card--danger`, y sólo deben existir cuando comunican una diferencia real de estado.

No deben crearse clases de tarjeta por pantalla cuando alguno de estos componentes cubra la misma necesidad. La antigua clase genérica `card-custom` ya no debe utilizarse en vistas nuevas.

## 9. Autocompletado

Las búsquedas con sugerencias utilizan `autocomplete` como ancla y `autocomplete-results` como lista superpuesta. La tarjeta contenedora debe incorporar `form-card--dropdown` para permitir que la lista se muestre por encima del contenido siguiente sin modificar la altura de la grilla.

Las listas extensas deben limitar la cantidad de resultados visibles y permitir que el usuario refine la búsqueda. El Punto de Venta muestra un máximo de cinco productos por consulta.

## 10. Iconos

Los iconos Material Symbols deben usar la escala compartida `icon-xs`, `icon-sm`, `icon-md` o `icon-lg`. No debe declararse su tamaño mediante estilos inline ni mediante reglas propias de una pantalla.

Los tamaños especiales vinculados a una visualización concreta, como gráficos o estados vacíos destacados, pueden conservar una clase de componente específica cuando la escala general no represente su función.

## 11. Avatares y miniaturas

- `avatar`: representa una persona y siempre utiliza forma circular.
- `avatar--xs`: avatar de 32 px para filas densas y controles compactos.
- `avatar--sm`: avatar de 40 px para tablas y selecciones compactas.
- `avatar--md`: avatar de 48 px para bloques destacados de información.
- `avatar--lg`: avatar de 140 px para vistas de edición o perfil.
- `media-thumbnail`: representa visualmente una entidad sin imponer forma circular.
- `media-thumbnail--sm`: miniatura compacta de 40 px.
- `media-preview`: limita una imagen cargada a 180 px y conserva su proporción sin deformarla.

Las imágenes y sus alternativas con inicial o icono deben compartir el mismo componente y modificador para conservar dimensiones y alineación.

## 12. Controles de estado

Los campos booleanos presentados como interruptor utilizan `status-switch`. La clase centraliza dimensiones, cursor interactivo y cursor deshabilitado; debe acompañar a `form-check-input` y `role="switch"`.

Los indicadores textuales utilizan `status-badge`, que genera su propio punto de estado. No debe agregarse otro punto decorativo dentro del elemento.

## 13. Controles compactos

Los campos numéricos ubicados al final de una tabla utilizan `quantity-input` cuando deben conservar un ancho máximo de 110 px y alinearse al extremo de la celda.

## 14. Tipografía auxiliar

- `text-caption`: texto auxiliar de 12 px.
- `text-micro`: metadatos muy compactos de 11,2 px.
- `section-title--divided`: variante de título de sección con icono y separador inferior.
- `detail-label` y `detail-value`: par reutilizable para etiquetas y valores de una ficha de detalle.

Los totales destacados utilizan `summary-total__label` y `summary-total__value` para centralizar el espaciado entre caracteres sin repetir estilos en cada resumen.

## 15. Notificaciones

El desplegable utiliza `notifications-menu`. Sus listados utilizan `notifications-scroll` y pueden sumar `notifications-scroll--compact` cuando requieren una altura visible menor. Estas clases conservan los límites y el desplazamiento interno sin estilos inline.

## 16. Bloques ilustrativos

Las ilustraciones auxiliares de una guía utilizan `context-visual`. Su contenido visual y su superposición utilizan `context-visual__media` y `context-visual__overlay`. Las imágenes propias de un módulo se expresan mediante una variante, como `context-visual--users`, y se comparten entre todas sus vistas equivalentes.

## 17. Ancho de página

El contenido interno utiliza el ancho general definido por el layout. Las operaciones que necesitan limitar una superficie especialmente amplia pueden agregar `page-width-wide`, que establece un máximo reutilizable de 1440 px y conserva el centrado horizontal.

## 18. Tablas y movimientos

- `table--medium` y `table--wide`: anchos mínimos compartidos para tablas con muchas columnas.
- `table-text-truncate`: limita textos extensos dentro de una celda sin alterar el ancho de la tabla.
- `movement-badge`: identifica movimientos y utiliza las variantes semánticas `movement-badge--income` y `movement-badge--expense`.

## 19. Información operativa

- `info-panel`: panel neutro para información relacionada con una operación.
- `metric-box`: valor destacado dentro de un formulario o detalle.
- `result-box`: resultado calculado; `result-box--danger` comunica un resultado inválido.
- `summary-total-group`: agrupa y alinea la etiqueta y el importe total.
- `breadcrumb-meta`, `field-label-compact` y `section-title--compact`: tipografía auxiliar reutilizable.
- `text-brand`: aplica el color principal interno a textos que requieren énfasis de marca.
- `textarea-fixed`: evita el redimensionamiento manual cuando la estructura del formulario requiere una altura estable.
- `readonly-value`: diferencia visualmente un valor de sólo lectura.
- `field-warning`: conserva la legibilidad de advertencias breves asociadas a un campo.

Las clases utilizadas exclusivamente como selectores de JavaScript pueden coexistir con el componente visual, pero no deben contener decisiones de presentación.

## 20. Autenticación pública

Las pantallas públicas de acceso utilizan `auth-page` como estructura centrada y `auth-card` como superficie común. Sus acciones principales reutilizan `btn-v-primary`. No deben recrearse tarjetas o botones particulares para Login, Registro o recuperación de contraseña.

## 21. Estados vacíos y exportaciones

- `empty-state`: contenido centrado que comunica la ausencia de resultados dentro de paneles, tablas y reportes.
- `empty-state-icon`: icono destacado que puede acompañar al estado vacío.
- `btn-export-action`: acción compartida para descargar o exportar información desde los reportes.

Los nombres de estos componentes deben describir su función y no la primera pantalla donde fueron utilizados. Por ese motivo, los reportes y el Dashboard comparten `empty-state`.

## 22. Compatibilidad y limpieza

Las vistas deben consumir únicamente los nombres vigentes del sistema de diseño. No se conservan alias antiguos sin consumidores: cuando una migración termina, se actualizan todas las vistas y se elimina la definición anterior para evitar reglas duplicadas o contradictorias.
