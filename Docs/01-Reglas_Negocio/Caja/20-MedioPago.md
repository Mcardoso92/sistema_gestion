# Módulo MedioPago

Última actualización: 01/09/2026

---

# 1. Objetivo

MedioPago representa la forma utilizada para cobrar, pagar o reintegrar dinero dentro de Veltika.

Ejemplos actuales de tipos soportados:

```text
Efectivo
Transferencia
TarjetaDebito
TarjetaCredito
QR
Cheque
Otro
```

MedioPago no representa por sí mismo una cuenta financiera ni una Caja.

```text
MedioPago = forma de pago
Caja = destino/origen financiero
```

Ambos conceptos se relacionan, pero cumplen responsabilidades distintas.

---

# 2. Modelo actual

`MedioPago` posee actualmente:

| Campo | Descripción |
|---|---|
| Id | Identificador |
| Nombre | Nombre visible, obligatorio, máximo 100 caracteres |
| Descripcion | Texto opcional, máximo 250 caracteres |
| Tipo | TipoMedioPago |
| Estado | Activo/Inactivo |
| FechaAlta | Fecha de creación |
| EmpresaId | Empresa propietaria |

Relaciones actuales:

- Empresa.
- CajaMedioPago.
- MovimientoCaja.
- CobroVenta.
- PagoProveedor.
- ReintegroVenta.
- ReintegroProveedor.

---

# 3. Tipos soportados

El enum `TipoMedioPago` define actualmente:

```text
Efectivo = 1
Transferencia = 2
TarjetaDebito = 3
TarjetaCredito = 4
QR = 5
Cheque = 6
Otro = 7
```

El valor recibido desde formularios se valida en servidor mediante `Enum.IsDefined`.

---

# 4. Autorización

`MedioPagoController` utiliza:

```text
[Authorize(Roles = "SuperAdmin,AdminEmpresa")]
```

Por lo tanto el mantenimiento administrativo actual está disponible para esos roles.

---

# 5. Multiempresa

Cada MedioPago pertenece a una única Empresa mediante:

```text
EmpresaId
```

AdminEmpresa queda restringido a:

```text
usuario.EmpresaId
```

SuperAdmin puede operar globalmente y seleccionar/filtrar Empresa cuando el flujo lo permite.

La Empresa enviada desde el cliente nunca constituye fuente de confianza para AdminEmpresa.

---

# 6. Listado

El listado permite actualmente filtrar por:

- Estado: activos, inactivos o todos.
- Empresa para SuperAdmin.
- búsqueda por Nombre o Descripcion.

El estado por defecto es:

```text
activos
```

La paginación actual es de:

```text
20 registros por página
```

Los resultados se ordenan por Nombre.

---

# 7. Alta

Al crear un MedioPago se valida:

- Usuario autenticado.
- Empresa válida y activa.
- alcance multiempresa.
- Nombre obligatorio.
- longitud del Nombre.
- longitud de Descripcion.
- Tipo válido.
- unicidad del Nombre dentro de la Empresa.

Al persistir se establece:

```text
Estado = true
FechaAlta = DateTime.Now
```

---

# 8. Normalización de datos

Antes de guardar:

```text
Nombre = Trim()
```

La Descripcion:

- se recorta con `Trim()` si posee contenido;
- se guarda como `null` si está vacía o contiene sólo espacios.

---

# 9. Unicidad del Nombre

No puede existir otro MedioPago con el mismo Nombre dentro de la misma Empresa.

La comparación actual se realiza de forma case-insensitive mediante `ToLower()`.

La validación incluye deliberadamente registros activos e inactivos.

Esto evita crear un duplicado cuando ya existe uno desactivado.

---

# 10. Reactivación

Si al crear se encuentra un MedioPago inactivo con el mismo Nombre, el sistema no crea otro registro.

Informa que el MedioPago existente puede reactivarse desde Edit.

La reactivación se realiza estableciendo nuevamente:

```text
Estado = true
```

---

# 11. Edición

Desde Edit pueden modificarse actualmente:

- Nombre.
- Descripcion.
- Tipo, sujeto a restricciones históricas.
- Estado.

La Empresa no se modifica desde este flujo.

El `EmpresaId` real se obtiene nuevamente desde la entidad almacenada en base de datos.

---

# 12. Cambio de Tipo

Existe una regla importante de trazabilidad financiera:

Si el MedioPago ya posee `MovimientoCaja`, no puede cambiarse su `Tipo`.

La razón es que cambiar, por ejemplo:

```text
Efectivo -> Transferencia
```

sobre un medio ya utilizado alteraría la interpretación histórica de operaciones financieras existentes.

La validación se realiza en servidor antes de persistir el cambio.

---

# 13. Estado y Soft Delete

MedioPago utiliza desactivación lógica mediante:

```text
Estado = false
```

No corresponde eliminar físicamente un MedioPago utilizado históricamente.

La reactivación se realiza desde Edit.

---

# 14. Details

El detalle muestra actualmente:

- Nombre.
- Descripcion.
- Tipo.
- Estado.
- FechaAlta.
- Empresa.
- Cajas asociadas.

Las Cajas asociadas se obtienen mediante la relación `CajaMedioPago`.

---

# 15. Relación Caja ↔ MedioPago

La relación entre Caja y MedioPago es de muchos a muchos mediante:

```text
CajaMedioPago
```

Conceptualmente:

```text
Una Caja puede aceptar varios MediosPago
Un MedioPago puede estar asociado a varias Cajas
```

La configuración de estas asociaciones se administra actualmente desde el flujo de Caja.

No existe un módulo independiente de asignación dentro de MedioPago.

---

# 16. Compatibilidad con Caja

La asociación Caja-MedioPago determina qué combinaciones pueden utilizarse en operaciones financieras.

La validación concreta depende también del tipo de Caja y del flujo operativo.

Ejemplo conceptual:

```text
MedioPago Efectivo
    -> Caja de efectivo

MedioPago Transferencia
    -> Caja bancaria o equivalente configurada
```

La compatibilidad efectiva se valida en los controllers correspondientes y no debe confiarse sólo a la interfaz.

---

# 17. Relación con CobroVenta

`CobroVenta` referencia el MedioPago utilizado para registrar el ingreso asociado a una Venta.

Una Venta puede poseer múltiples CobroVenta y, por lo tanto, utilizar más de un MedioPago.

Esto permite actualmente pagos combinados.

---

# 18. Relación con PagoProveedor

`PagoProveedor` utiliza MedioPago para identificar cómo se efectuó un pago relacionado con una Compra/Proveedor.

El MedioPago debe pertenecer a la misma Empresa y ser válido para la operación.

---

# 19. Relación con Reintegros

Los módulos:

```text
ReintegroVenta
ReintegroProveedor
```

pueden utilizar MedioPago para registrar el canal financiero de devolución del dinero.

El MedioPago histórico asociado a estas operaciones no debe reinterpretarse posteriormente mediante cambios incompatibles de Tipo.

---

# 20. Relación con MovimientoCaja

MovimientoCaja utiliza `MedioPagoId` cuando corresponde para describir la naturaleza financiera de un ingreso o egreso.

Esta relación es también la que actualmente bloquea el cambio de Tipo de MedioPago una vez que existen movimientos.

---

# 21. MedioPago vs Caja

No deben confundirse los conceptos.

Ejemplo:

```text
MedioPago: Transferencia
Caja: Cuenta Banco Nación
```

Otro ejemplo:

```text
MedioPago: Efectivo
Caja: Caja Principal
```

La separación permite que una misma forma de pago se vincule a distintas Cajas según la configuración empresarial.

---

# 22. No edición de Empresa

Una vez creado el MedioPago, el flujo actual de Edit no permite moverlo a otra Empresa.

Esto evita trasladar accidentalmente un registro con relaciones financieras históricas entre tenants.

Si una Empresa diferente necesita un MedioPago equivalente, debe poseer su propio registro.

---

# 23. Seguridad

Las validaciones críticas se realizan en servidor:

- Usuario autenticado.
- rol autorizado.
- Empresa correcta.
- Empresa activa en Create.
- Tipo válido.
- Nombre no duplicado.
- recurso perteneciente a la Empresa.
- restricción histórica de cambio de Tipo.

Ocultar opciones en la UI no reemplaza estas reglas.

---

# 24. Reglas de negocio actuales

1. MedioPago pertenece a una única Empresa.
2. Nombre es obligatorio y tiene máximo 100 caracteres.
3. Descripcion es opcional y tiene máximo 250 caracteres.
4. Tipo debe pertenecer a TipoMedioPago.
5. No puede repetirse Nombre dentro de una Empresa.
6. La unicidad incluye activos e inactivos.
7. Un registro inactivo con el mismo Nombre debe reactivarse en lugar de duplicarse.
8. Estado implementa desactivación lógica.
9. FechaAlta se establece al crear.
10. AdminEmpresa sólo opera dentro de su Empresa.
11. SuperAdmin puede operar sobre distintas Empresas.
12. La Empresa no se cambia desde Edit.
13. El Tipo no puede cambiar si existen MovimientosCaja asociados.
14. MedioPago puede asociarse a múltiples Cajas mediante CajaMedioPago.
15. La asociación con Cajas se administra desde Caja.
16. MedioPago participa en CobroVenta.
17. MedioPago participa en PagoProveedor.
18. MedioPago participa en ReintegroVenta y ReintegroProveedor.
19. MedioPago no representa una Caja ni una cuenta financiera por sí mismo.
20. Las validaciones críticas se mantienen en backend.

---

# 25. Evolución futura

Posibles mejoras futuras:

- reglas de conciliación por MedioPago;
- comisiones por tarjetas/plataformas;
- plazos de acreditación;
- integraciones directas con Mercado Pago u otros proveedores;
- datos adicionales para Cheques;
- conciliación bancaria;
- reportes históricos más avanzados por MedioPago;
- permisos granulares de configuración;
- parametrización más detallada por futura Sucursal.

No se consideran implementadas actualmente salvo lo expresamente indicado antes.

---

# 26. Estado actual

✅ CRUD administrativo implementado.

✅ Soft Delete/desactivación implementada.

✅ Reactivación desde Edit implementada.

✅ Tipos de MedioPago implementados.

✅ Multiempresa implementado.

✅ Relación con Caja implementada.

✅ Relación con CobroVenta implementada.

✅ Relación con PagoProveedor implementada.

✅ Relación con Reintegros implementada.

✅ Protección histórica del Tipo implementada cuando existen MovimientosCaja.

✅ Búsqueda, filtros y paginación implementados.

🚧 Conciliación avanzada pendiente.

🚧 Integraciones directas con proveedores de pago pendientes.

🚧 Permisos granulares pendientes.