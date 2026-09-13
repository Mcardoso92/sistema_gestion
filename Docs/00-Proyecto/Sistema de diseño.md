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
- Iconos: `--veltika-icon-sm`, `--veltika-icon-md` y `--veltika-icon-lg`.
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

