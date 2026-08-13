# Módulo Auditoría

---

# 1. Objetivo

El módulo Auditoría permite registrar todas las acciones relevantes realizadas por los usuarios dentro de Veltika.

Su finalidad es garantizar la trazabilidad completa de las operaciones del sistema, permitiendo conocer quién realizó una acción, cuándo la realizó, desde dónde y sobre qué información.

Este módulo constituye uno de los pilares de seguridad y control del sistema.

---

# 2. Alcance

El módulo registra automáticamente las operaciones realizadas sobre los distintos módulos de Veltika.

Los registros de auditoría son generados por el sistema y no pueden crearse, modificarse ni eliminarse manualmente.

---

# 3. Actores

- Super Administrador
- Administrador de Empresa
- Auditor

---

# 4. Permisos

## Super Administrador

✅ Consultar todos los registros.

✅ Exportar auditorías.

## Administrador de Empresa

✅ Consultar auditorías de su empresa.

## Auditor

✅ Consultar auditorías.

❌ Ningún usuario puede modificar o eliminar registros.

---

# 5. Funcionalidades

Actualmente

- Registrar acciones automáticamente.
- Consultar auditorías.
- Buscar auditorías.
- Filtrar auditorías.
- Visualizar detalle.

Versiones futuras

- Exportación a Excel.
- Exportación PDF.
- Alertas automáticas.
- Auditoría avanzada.
- Firma digital.
- Integración con SIEM.

---

# 6. Campos

| Campo | Descripción |
|---------|-------------|
| Id | Identificador único |
| EmpresaId | Empresa afectada |
| UsuarioId | Usuario responsable |
| Modulo | Módulo afectado |
| Accion | Acción realizada |
| RegistroId | Registro involucrado |
| FechaHora | Fecha y hora |
| DireccionIP | Dirección IP |
| Observaciones | Información adicional |

Campos futuros

- Navegador
- SistemaOperativo
- Dispositivo
- SessionId
- ValorAnterior
- ValorNuevo

---

# 7. Validaciones

- Debe existir un usuario autenticado.
- Debe existir una acción.
- El módulo es obligatorio.
- La fecha será generada automáticamente.
- El registro nunca podrá modificarse.

---

# 8. Reglas de negocio

- Toda acción relevante generará un registro.
- Los registros nunca podrán eliminarse.
- Los registros nunca podrán modificarse.
- Toda auditoría conservará el historial completo.
- La auditoría será automática.

---

# 9. Casos de uso

## Registrar acción

El sistema registra automáticamente una operación realizada por un usuario.

Resultado esperado:

- Acción almacenada.
- Historial actualizado.

---

## Consultar auditoría

Permite visualizar todas las acciones registradas.

---

## Buscar auditoría

Permite localizar acciones mediante distintos filtros.

---

# 10. Casos de error

- Usuario inexistente.
- Acción inválida.
- Registro inexistente.
- Usuario sin permisos.

---

# 11. Flujo funcional

1. El usuario realiza una operación.
2. El sistema ejecuta la acción.
3. La operación finaliza correctamente.
4. Se genera automáticamente el registro de auditoría.
5. El historial queda disponible para futuras consultas.

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
- Configuración

---

# 13. Mejoras futuras

- Firma digital.
- Exportación.
- Alertas automáticas.
- Dashboard de auditoría.
- Integración con herramientas de monitoreo.
- Auditoría de API.
- Historial de cambios por campo.

---

# 14. Roadmap

Versión 1.0

- Registro automático.
- Consulta.
- Filtros.

Versión 2.0

- Exportación.
- Alertas.
- Historial avanzado.

Versión 3.0

- Firma digital.
- Integración SIEM.
- IA para detección de anomalías.

---

# 15. Decisiones de Arquitectura

## Registro automático

La auditoría será generada exclusivamente por el sistema.

Los usuarios no podrán registrar auditorías manualmente.

---

## Historial inmutable

Una vez almacenado un registro, no podrá modificarse ni eliminarse.

La auditoría constituye evidencia histórica del sistema.

---

## Acciones auditadas

Inicialmente se registrarán:

- Inicio de sesión.
- Cierre de sesión.
- Creación.
- Modificación.
- Activación.
- Desactivación.
- Anulación.
- Ajustes de stock.
- Apertura de caja.
- Cierre de caja.

En futuras versiones podrán incorporarse nuevas acciones.

---

## Alcance

No todas las operaciones del sistema serán auditadas.

Únicamente aquellas consideradas relevantes desde el punto de vista funcional y de seguridad.

---

## Información registrada

Cada auditoría almacenará:

- Usuario.
- Empresa.
- Módulo.
- Acción.
- Registro afectado.
- Fecha y hora.
- Dirección IP.
- Observaciones.

---

## Escalabilidad

El diseño permitirá incorporar nuevos módulos sin modificar la estructura de Auditoría.

Bastará con registrar el nombre del módulo y la acción correspondiente.

---

## Integridad

Toda operación crítica del sistema deberá poder reconstruirse consultando la Auditoría.

La información registrada servirá como respaldo para controles internos, investigaciones y seguimiento de actividades.