# Módulo Ajuste de Stock

---

# 1. Objetivo

El módulo Ajuste de Stock permite corregir diferencias entre el stock físico y el stock registrado en el sistema.

Todo ajuste genera automáticamente un Movimiento de Stock, garantizando la trazabilidad completa del inventario.

Este módulo constituye el único mecanismo autorizado para modificar el stock fuera de los procesos normales de compra, venta, devolución o transferencia.

---

# 2. Alcance

El módulo permite registrar ajustes de stock positivos o negativos sobre uno o varios productos.

Cada ajuste deberá registrar el motivo de la modificación y quedará almacenado permanentemente para futuras auditorías.

---

# 3. Actores

- Super Administrador
- Administrador de Empresa
- Responsable de Depósito

---

# 4. Permisos

## Super Administrador

✅ Registrar ajustes

✅ Consultar ajustes

✅ Visualizar historial

## Administrador de Empresa

✅ Registrar ajustes

✅ Consultar ajustes

## Responsable de Depósito

✅ Registrar ajustes

✅ Consultar ajustes

❌ No puede eliminar ajustes

---

# 5. Funcionalidades

Actualmente

- Registrar ajuste
- Consultar ajustes
- Buscar ajustes
- Filtrar ajustes
- Visualizar detalle del ajuste

Versiones futuras

- Ajustes masivos
- Importación desde Excel
- Aprobación de ajustes
- Firma digital
- Ajustes programados
- Inventarios físicos
- Conteos cíclicos

---

# 6. Campos

| Campo | Descripción |
|---------|-------------|
| Id | Identificador único |
| EmpresaId | Empresa propietaria |
| SucursalId | Sucursal afectada |
| ProductoId | Producto ajustado |
| UsuarioId | Usuario responsable |
| TipoAjuste | Positivo o Negativo |
| Cantidad | Cantidad ajustada |
| Motivo | Justificación del ajuste |
| Observaciones | Información adicional |
| FechaAjuste | Fecha y hora |

Campos futuros

- Estado
- UsuarioAprobador
- FechaAprobacion
- DocumentoAdjunto
- EvidenciaFotografica
- InventarioId

---

# 7. Validaciones

- Debe existir un producto.
- Debe existir un usuario autorizado.
- La cantidad debe ser mayor a cero.
- El motivo es obligatorio.
- El tipo de ajuste es obligatorio.
- No podrá dejar el stock en negativo (según configuración).
- El sistema registrará automáticamente la fecha del ajuste.

---

# 8. Reglas de negocio

- Todo ajuste genera un Movimiento de Stock.
- Ningún ajuste modifica directamente el stock.
- Todo ajuste debe poseer un motivo.
- Todo ajuste conservará el historial completo.
- Los ajustes nunca podrán eliminarse físicamente.
- Un ajuste no podrá modificarse una vez confirmado.

---

# 9. Casos de uso

## Registrar ajuste positivo

Permite incrementar el stock de un producto cuando existe una diferencia favorable.

Resultado esperado:

- Stock actualizado.
- Movimiento registrado.
- Ajuste almacenado.

---

## Registrar ajuste negativo

Permite disminuir el stock cuando existe una diferencia desfavorable.

Resultado esperado:

- Stock actualizado.
- Movimiento registrado.
- Ajuste almacenado.

---

## Consultar ajustes

Permite visualizar todos los ajustes registrados.

---

## Buscar ajustes

Permite localizar ajustes utilizando diferentes criterios.

---

# 10. Casos de error

- Producto inexistente.
- Cantidad inválida.
- Motivo vacío.
- Usuario sin permisos.
- Ajuste inexistente.
- Stock insuficiente para un ajuste negativo.

---

# 11. Flujo funcional

1. El usuario ingresa al módulo Ajustes de Stock.
2. Selecciona el producto.
3. Indica el tipo de ajuste.
4. Ingresa la cantidad.
5. Selecciona el motivo.
6. Agrega observaciones (opcional).
7. Confirma el ajuste.
8. El sistema genera un Movimiento de Stock.
9. Se actualiza el stock.
10. El ajuste queda registrado para auditoría.

---

# 12. Integraciones

Este módulo se relaciona con:

- Producto
- Stock
- MovimientoStock
- Sucursal
- Usuario
- Auditoría
- Reportes

---

# 13. Mejoras futuras

- Ajustes por inventario.
- Aprobación por supervisor.
- Evidencia fotográfica.
- Firma digital.
- Ajustes masivos.
- Integración con lectores de códigos de barras.
- Integración con dispositivos móviles.

---

# 14. Roadmap

Versión 1.0

- Registro
- Consulta
- Historial
- Integración con MovimientoStock

Versión 2.0

- Inventarios
- Ajustes masivos
- Aprobaciones

Versión 3.0

- Firma digital
- Evidencia multimedia
- Automatización de inventarios

---

# 15. Decisiones de Arquitectura

## El ajuste no modifica el stock directamente

Todo ajuste generará un Movimiento de Stock.

El stock siempre será actualizado mediante dicho movimiento.

---

## Motivo obligatorio

Cada ajuste deberá indicar la causa de la modificación.

Motivos iniciales:

- Rotura
- Robo
- Producto vencido
- Error de carga
- Diferencia de inventario
- Donación
- Consumo interno
- Otro

---

## Historial permanente

Los ajustes nunca podrán eliminarse ni modificarse.

Conservarán permanentemente toda la información registrada.

---

## Auditoría completa

Cada ajuste almacenará:

- Usuario responsable.
- Fecha y hora.
- Producto.
- Cantidad.
- Tipo de ajuste.
- Motivo.
- Observaciones.

---

## Ajustes negativos

En la versión 1.0 no será posible registrar un ajuste negativo que deje el stock por debajo de cero.

---

## Aprobaciones futuras

En versiones futuras, determinados ajustes podrán requerir la aprobación de un usuario con mayor jerarquía antes de impactar en el inventario.

---

## Integridad del inventario

Todo ajuste deberá poder justificarse y reconstruirse mediante el historial de auditoría y los movimientos de stock asociados.