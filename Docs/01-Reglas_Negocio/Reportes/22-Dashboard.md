# Módulo Dashboard

---

# 1. Objetivo

El módulo Dashboard proporciona una visión general del estado actual de la empresa mediante indicadores, gráficos y accesos rápidos a la información más relevante del sistema.

Su finalidad es permitir que el usuario conozca rápidamente la situación del negocio y pueda tomar decisiones sin necesidad de consultar múltiples módulos.

Este módulo constituye la pantalla principal de Veltika.

---

# 2. Alcance

El Dashboard reúne información proveniente de distintos módulos del sistema y la presenta de forma resumida mediante tarjetas, gráficos e indicadores.

Toda la información mostrada corresponde exclusivamente a la empresa del usuario autenticado.

---

# 3. Actores

- Super Administrador
- Administrador de Empresa
- Usuario

---

# 4. Permisos

## Super Administrador

✅ Visualizar el Dashboard global.

## Administrador de Empresa

✅ Visualizar el Dashboard de su empresa.

## Usuario

✅ Visualizar el Dashboard según sus permisos.

❌ No podrá acceder a indicadores restringidos.

---

# 5. Funcionalidades

Actualmente

- Visualizar indicadores generales.
- Consultar gráficos.
- Acceder rápidamente a los módulos principales.
- Visualizar alertas del sistema.

Versiones futuras

- Dashboard personalizable.
- Widgets configurables.
- Múltiples dashboards.
- Indicadores en tiempo real.
- Comparativas entre períodos.
- Dashboard móvil.

---

# 6. Campos

| Campo | Descripción |
|---------|-------------|
| Id | Identificador único |
| EmpresaId | Empresa propietaria |
| UsuarioId | Usuario |
| FechaActualizacion | Última actualización |

Indicadores iniciales

- Ventas del día
- Ventas del mes
- Compras del mes
- Productos registrados
- Clientes registrados
- Proveedores registrados
- Stock bajo mínimo
- Caja actual

Campos futuros

- Rentabilidad
- Productos más vendidos
- Clientes frecuentes
- Compras pendientes
- Productos sin movimiento
- Ranking de sucursales
- Indicadores financieros

---

# 7. Validaciones

- Debe existir una empresa.
- Debe existir un usuario autenticado.
- El usuario visualizará únicamente la información permitida por sus permisos.

---

# 8. Reglas de negocio

- El Dashboard será únicamente informativo.
- No permitirá modificar información.
- Los indicadores utilizarán datos actualizados.
- Cada empresa visualizará exclusivamente su propia información.

---

# 9. Casos de uso

## Consultar Dashboard

El usuario ingresa al sistema.

Resultado esperado:

- Se muestran los indicadores principales.
- Se presentan los gráficos disponibles.
- Se visualizan las alertas del sistema.

---

## Acceder a un módulo

El usuario selecciona un indicador o acceso rápido.

Resultado esperado:

- El sistema redirige al módulo correspondiente.

---

# 10. Casos de error

- Usuario sin permisos.
- Empresa inexistente.
- Error al obtener indicadores.
- Sin información disponible.

---

# 11. Flujo funcional

1. El usuario inicia sesión.
2. El sistema identifica la empresa.
3. Se consultan los indicadores correspondientes.
4. Se generan los gráficos.
5. Se muestran las alertas.
6. El Dashboard queda disponible para su consulta.

---

# 12. Integraciones

Este módulo se relaciona con:

- Empresa
- Usuario
- Producto
- Categoría
- Cliente
- Proveedor
- Venta
- Compra
- Stock
- Caja
- Reportes
- Auditoría

---

# 13. Mejoras futuras

- Dashboard personalizable.
- Widgets configurables.
- Indicadores en tiempo real.
- Comparativas históricas.
- Dashboard por sucursal.
- Dashboard por usuario.
- Inteligencia Artificial.
- Predicciones de ventas.

---

# 14. Roadmap

Versión 1.0

- Indicadores generales.
- Accesos rápidos.
- Alertas básicas.

Versión 2.0

- Gráficos avanzados.
- Personalización.
- Comparativas.

Versión 3.0

- IA.
- Predicciones.
- Dashboard totalmente configurable.

---

# 15. Decisiones de Arquitectura

## Pantalla principal

El Dashboard será la primera pantalla visualizada por el usuario luego del inicio de sesión.

---

## Información resumida

El Dashboard mostrará únicamente indicadores resumidos.

La información detallada deberá consultarse desde los módulos correspondientes.

---

## Accesos rápidos

Cada indicador podrá actuar como acceso directo hacia el módulo relacionado.

Ejemplos:

- Ventas del día → Módulo Ventas.
- Stock bajo mínimo → Módulo Stock.
- Caja actual → Módulo Caja.

---

## Actualización automática

Los indicadores reflejarán la información actual del sistema.

En futuras versiones podrán actualizarse automáticamente sin necesidad de recargar la página.

---

## Personalización

En versiones futuras cada usuario podrá decidir:

- Qué indicadores visualizar.
- El orden de los widgets.
- Los gráficos mostrados.
- Los accesos rápidos.

---

## Seguridad

El Dashboard respetará los permisos del usuario.

No mostrará información correspondiente a módulos sobre los cuales el usuario no posea autorización.

---

## Escalabilidad

El diseño permitirá incorporar nuevos indicadores y widgets sin modificar la estructura general del Dashboard.

Cada nuevo módulo de Veltika podrá aportar información al Dashboard de forma independiente.