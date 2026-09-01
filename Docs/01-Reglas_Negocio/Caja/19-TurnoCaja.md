# Módulo TurnoCaja

Última actualización: 01/09/2026

---

# 1. Objetivo

TurnoCaja representa el período operativo de apertura y cierre de una Caja que trabaja con turnos.

Su función es identificar quién abrió y cerró la Caja, registrar el fondo fijo aplicado, calcular el efectivo esperado, almacenar el arqueo realizado y conservar la diferencia resultante.

TurnoCaja no reemplaza a Caja.

```text
Caja = recurso financiero permanente
TurnoCaja = período operativo de uso
```

---

# 2. Modelo actual

`TurnoCaja` posee actualmente:

| Campo | Descripción |
|---|---|
| Id | Identificador |
| EmpresaId | Empresa propietaria |
| CajaId | Caja operada |
| UsuarioAperturaId | Usuario que abrió |
| FechaApertura | Fecha/hora de apertura |
| Estado | Abierto/Cerrado |
| FechaCierre | Fecha/hora de cierre |
| UsuarioCierreId | Usuario que cerró |
| CierreForzado | Indica cierre administrativo forzado |
| MotivoCierreForzado | Motivo opcional, máximo 500 caracteres |
| FondoFijoAplicado | Fondo fijo histórico del turno |
| EfectivoEsperado | Efectivo calculado al cierre |
| EfectivoContado | Efectivo informado en arqueo |
| Diferencia | Diferencia entre contado y esperado |
| ImporteRendido | Importe rendido al cerrar |

También se relaciona con `Empresa`, `Caja`, `UsuarioApertura`, `UsuarioCierre` y `MovimientosCaja`.

---

# 3. Autorización

`TurnoCajaController` utiliza:

```text
[Authorize(Roles = "SuperAdmin,AdminEmpresa")]
```

Sin embargo, distintas acciones aplican reglas operativas adicionales.

---

# 4. Multiempresa

AdminEmpresa sólo puede consultar turnos pertenecientes a:

```text
usuario.EmpresaId
```

SuperAdmin puede visualizar turnos globalmente y filtrar por Empresa en el listado.

Las operaciones sobre recursos concretos deben respetar la Empresa asociada al Usuario y a la Caja.

---

# 5. Listado

El listado permite filtrar por:

- Estado: abiertos, cerrados o todos.
- Empresa para SuperAdmin.
- Búsqueda por nombre de Caja o usuario de apertura.

El estado por defecto es:

```text
abiertos
```

La paginación actual es de:

```text
20 registros por página
```

Los resultados se ordenan por FechaApertura descendente.

---

# 6. Cajas que pueden utilizar Turnos

Para abrir un TurnoCaja, la Caja debe cumplir actualmente:

```text
Estado == true
Tipo == Efectivo
PermiteTurnos == true
```

Además debe pertenecer a la misma Empresa del Usuario y no poseer otro turno abierto.

Las Cajas de Banco, BilleteraVirtual u Otro no participan actualmente del flujo de apertura de TurnoCaja.

---

# 7. Restricción para SuperAdmin al abrir

SuperAdmin puede consultar Turnos, pero el flujo actual de apertura no permite que opere directamente sin contexto empresarial específico.

En `Abrir`, si el Usuario es SuperAdmin, el controller rechaza la operación.

Por lo tanto la apertura operativa está pensada actualmente para usuarios empresariales.

---

# 8. Un turno abierto por Usuario

Un mismo Usuario no puede poseer simultáneamente más de un TurnoCaja abierto.

Antes de abrir se valida:

```text
UsuarioAperturaId == usuario.Id
Estado == Abierto
```

Si existe, la nueva apertura es rechazada.

---

# 9. Un turno abierto por Caja

Una Caja tampoco puede poseer simultáneamente más de un TurnoCaja abierto.

Las Cajas ya ocupadas son excluidas del listado de disponibles y la condición se vuelve a validar antes de persistir.

---

# 10. Concurrencia de apertura

La apertura utiliza una transacción con:

```text
IsolationLevel.Serializable
```

Dentro de la transacción se revalida:

- que el Usuario siga sin turno abierto;
- que la Caja siga sin turno abierto.

Esto reduce el riesgo de que dos solicitudes concurrentes abran simultáneamente la misma Caja o un segundo turno para el mismo Usuario.

La base también posee una migración específica para índices de turnos abiertos, reforzando estas restricciones a nivel persistente.

---

# 11. Apertura

Al abrir un turno se registra:

```text
EmpresaId = caja.EmpresaId
CajaId = caja.Id
UsuarioAperturaId = usuario.Id
FechaApertura = DateTime.Now
Estado = Abierto
FondoFijoAplicado = caja.FondoFijo
```

Los datos de cierre comienzan en null/default.

---

# 12. Fondo fijo histórico

`FondoFijoAplicado` copia el valor de:

```text
Caja.FondoFijo
```

al momento de abrir el Turno.

Esto es importante porque el FondoFijo de Caja puede modificarse posteriormente sin alterar el valor histórico utilizado en un Turno ya abierto o cerrado.

---

# 13. Details

El detalle del Turno muestra actualmente, entre otros datos:

- Empresa.
- Caja.
- Usuario de apertura.
- Fecha de apertura.
- Estado.
- Fondo fijo aplicado.
- Usuario/fecha de cierre.
- Cierre forzado y motivo.
- Efectivo esperado.
- Efectivo contado.
- Diferencia.
- Importe rendido.
- Movimientos de Caja asociados.
- Resumen de CobrosVenta por MedioPago.
- eventual movimiento de regularización de diferencia.

---

# 14. Movimientos asociados

Los MovimientosCaja pueden vincularse a un Turno mediante:

```text
MovimientoCaja.TurnoCajaId
```

El detalle recupera los movimientos vinculados al turno y los ordena por fecha descendente.

Esto permite reconstruir la actividad financiera ocurrida durante el período operativo.

---

# 15. Cobros por Medio de Pago

El detalle agrupa los `CobroVenta` activos del Turno por MedioPago.

Para cada medio obtiene:

- Total cobrado.
- Cantidad de cobros.

Esto permite analizar cómo se compuso la recaudación durante el turno.

---

# 16. Cierre y arqueo

El flujo de cierre almacena los datos necesarios para comparar el efectivo teórico con el efectivamente contado.

Los conceptos principales son:

```text
EfectivoEsperado
EfectivoContado
Diferencia
ImporteRendido
```

La existencia de estos campos confirma que el arqueo y la diferencia de Caja ya forman parte de la implementación actual.

---

# 17. Efectivo esperado

`EfectivoEsperado` representa el valor que el sistema calcula para la Caja al cierre considerando el Turno y sus movimientos aplicables.

No debe ingresarse manualmente desde el cliente como fuente de verdad.

El cálculo debe permanecer en servidor.

---

# 18. Efectivo contado

`EfectivoContado` representa el importe informado durante el arqueo físico de Caja.

Es el valor que se compara con el esperado para determinar si existe diferencia.

---

# 19. Diferencia

La diferencia de cierre representa conceptualmente:

```text
EfectivoContado - EfectivoEsperado
```

Los valores posibles permiten identificar:

- diferencia cero;
- sobrante;
- faltante.

La diferencia queda persistida históricamente dentro del Turno.

---

# 20. Regularización de diferencia

El módulo contempla movimientos específicos para regularizar diferencias:

```text
AjusteSobranteCaja
AjusteFaltanteCaja
```

El detalle del Turno identifica el movimiento de regularización asociado, si existe.

Estos ajustes deben quedar registrados como MovimientosCaja y no modificando silenciosamente movimientos históricos anteriores.

---

# 21. Importe rendido

`ImporteRendido` permite registrar cuánto efectivo se rinde al cierre.

No debe confundirse con:

```text
EfectivoContado
```

porque contar el efectivo y determinar cuánto se retira/rinde son conceptos operativos distintos.

---

# 22. Cierre forzado

TurnoCaja posee soporte explícito para:

```text
CierreForzado
MotivoCierreForzado
```

El motivo admite hasta 500 caracteres.

Esto permite distinguir un cierre normal de un cierre administrativo excepcional.

La existencia del cierre forzado debe conservar trazabilidad y no sobrescribir silenciosamente el contexto original del Turno.

---

# 23. Estado

El Turno utiliza `EstadoTurnoCaja`.

Los estados operativos actuales son:

```text
Abierto
Cerrado
```

No existe Soft Delete de TurnoCaja.

Un Turno cerrado permanece como información histórica.

---

# 24. No edición histórica

Los Turnos no se comportan como un CRUD administrativo común.

Una vez ocurrida la apertura/cierre, los datos representan hechos operativos históricos.

No corresponde permitir una edición libre de fechas, usuarios, diferencias o movimientos asociados porque rompería trazabilidad.

---

# 25. Relación con Venta y CobroVenta

Venta no almacena directamente `TurnoCajaId`.

El vínculo operativo con el Turno ocurre a través de los cobros y movimientos generados durante la operación.

Esto permite que una Venta tenga múltiples CobroVenta y que cada cobro registre la Caja/Turno correspondiente cuando aplique.

---

# 26. Relación con Caja

Caja define características permanentes como:

```text
Tipo
PermiteTurnos
FondoFijo
```

TurnoCaja registra el uso concreto de esa Caja en un período determinado.

Modificar la configuración futura de Caja no debe modificar los Turnos históricos.

---

# 27. Seguridad

Las reglas críticas del Turno deben validarse en backend:

- Empresa correcta.
- Caja activa.
- Tipo Efectivo.
- PermiteTurnos.
- Usuario sin otro turno abierto.
- Caja sin otro turno abierto.
- Estado correcto para cerrar/regularizar.

La interfaz no constituye por sí sola una barrera de seguridad.

---

# 28. Reglas de negocio actuales

1. TurnoCaja pertenece a una Empresa y una Caja.
2. Sólo Cajas activas de tipo Efectivo y con PermiteTurnos pueden abrir Turnos.
3. Un Usuario no puede tener más de un Turno abierto simultáneamente.
4. Una Caja no puede tener más de un Turno abierto simultáneamente.
5. La apertura se protege con transacción Serializable y revalidación.
6. SuperAdmin puede consultar globalmente, pero no abre Turnos mediante el flujo operativo actual.
7. El FondoFijo se copia históricamente al abrir.
8. TurnoCaja registra Usuario y Fecha de apertura.
9. El cierre registra Usuario y Fecha de cierre.
10. El arqueo almacena EfectivoEsperado y EfectivoContado.
11. La Diferencia queda persistida.
12. Puede existir ImporteRendido.
13. Existe soporte para cierre forzado y motivo.
14. La diferencia puede regularizarse mediante MovimientosCaja específicos.
15. Los CobroVenta activos se pueden resumir por MedioPago dentro del Turno.
16. Los movimientos históricos no deben editarse libremente.
17. TurnoCaja no posee Soft Delete.
18. AdminEmpresa queda restringido a su Empresa.

---

# 29. Evolución futura

Posibles mejoras, sólo si la operación real lo requiere:

- indicadores históricos de diferencias por cajero;
- reportes de cierres y rendiciones;
- aprobaciones para cierres forzados;
- políticas configurables de tolerancia de diferencia;
- notificaciones por faltantes/sobrantes relevantes;
- integración con futuras Sucursales;
- conciliación más avanzada por MedioPago;
- permisos granulares para abrir/cerrar/regularizar.

No se consideran implementadas actualmente salvo las funcionalidades expresamente descritas antes.

---

# 30. Estado actual

✅ Apertura de Turno implementada.

✅ Cierre implementado.

✅ Fondo fijo histórico implementado.

✅ Arqueo implementado.

✅ Efectivo esperado/contado implementado.

✅ Diferencia implementada.

✅ Importe rendido implementado.

✅ Cierre forzado soportado.

✅ Regularización de sobrante/faltante soportada.

✅ MovimientosCaja asociados implementados.

✅ Resumen de cobros por MedioPago implementado.

✅ Restricciones multiempresa implementadas.

✅ Protección de concurrencia en apertura implementada.

🚧 Indicadores históricos avanzados por cajero pendientes.

🚧 Permisos granulares pendientes.

🚧 Integración multi-sucursal pendiente.