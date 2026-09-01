# Módulo Configuración

Última actualización: 01/09/2026

---

# 1. Objetivo

El módulo Configuración permite administrar datos generales, comerciales, fiscales y de identidad visual de cada Empresa dentro de Veltika.

Su alcance actual no es un motor genérico de parámetros del sistema, sino una configuración concreta asociada a la Empresa.

La entidad vigente es:

```text
ConfiguracionEmpresa
```

---

# 2. Alcance actual

Actualmente permite configurar:

- Razón social.
- CUIT.
- Dirección.
- Teléfono.
- Email.
- Moneda.
- IVA por defecto.
- Monto para considerar una Venta importante.
- Logo de la Empresa.

No están implementados actualmente como parámetros de ConfiguracionEmpresa:

- Permitir stock negativo.
- Zona horaria.
- Idioma.
- Formato de fecha/hora.
- Cantidad de decimales.
- Numeración automática.
- Modo oscuro.
- Configuración fiscal avanzada.

---

# 3. Modelo ConfiguracionEmpresa

La entidad contiene:

| Campo | Descripción |
|---|---|
| Id | Identificador |
| EmpresaId | Empresa propietaria |
| RazonSocial | Razón social |
| Cuit | CUIT opcional |
| Direccion | Dirección opcional |
| Telefono | Teléfono opcional |
| Email | Email opcional |
| Moneda | Moneda principal |
| IvaPorcentaje | IVA configurado |
| MontoVentaImportante | Umbral opcional para Venta importante |
| LogoRuta | Ruta del logo opcional |

Relación:

```text
ConfiguracionEmpresa -> Empresa
```

---

# 4. Acceso

`ConfiguracionController` utiliza actualmente:

```text
[Authorize(Roles = "SuperAdmin,AdminEmpresa")]
```

## SuperAdmin

Puede consultar y modificar la configuración de diferentes Empresas.

Si no indica una Empresa al ingresar al módulo, el controller selecciona la primera Empresa activa disponible.

## AdminEmpresa

Sólo puede consultar y modificar la configuración de su propia Empresa.

El `EmpresaId` efectivo se obtiene desde el usuario autenticado y no se confía en un valor enviado por el navegador.

---

# 5. Existencia de la configuración

Una Empresa puede no poseer todavía un registro persistido de `ConfiguracionEmpresa`.

En ese caso, al consultar se utilizan valores de respaldo:

```text
RazonSocial = Empresa.Nombre
Moneda = "ARS"
IvaPorcentaje = 21
```

Al guardar por primera vez, si no existe la configuración, el sistema crea el registro automáticamente.

Por lo tanto, no es obligatorio que todas las Empresas tengan previamente una fila creada para poder ingresar al módulo.

---

# 6. Razón social

`RazonSocial`:

- Es obligatoria.
- Admite hasta 100 caracteres.
- Se normaliza mediante `Trim()` antes de persistirse.

Si todavía no existe ConfiguracionEmpresa, la pantalla toma inicialmente `Empresa.Nombre` como razón social.

`Empresa.Nombre` y `ConfiguracionEmpresa.RazonSocial` no deben asumirse como el mismo concepto funcional.

---

# 7. CUIT

`Cuit` es opcional.

Posee longitud máxima de:

```text
20 caracteres
```

Actualmente el módulo Configuración no implementa en este controller una validación específica del dígito verificador de CUIT.

El texto vacío o compuesto sólo por espacios se persiste como `null`.

---

# 8. Dirección

`Direccion` es opcional y admite hasta:

```text
150 caracteres
```

Se normaliza antes de persistirse.

Un valor vacío se almacena como `null`.

---

# 9. Teléfono

`Telefono` es opcional y admite hasta:

```text
30 caracteres
```

Se normaliza antes de persistirse.

Un valor vacío se almacena como `null`.

---

# 10. Email

`Email` es opcional.

Validaciones actuales:

- Formato de email válido.
- Máximo 100 caracteres.

Un valor vacío se almacena como `null`.

---

# 11. Moneda

La moneda es obligatoria.

El controller admite actualmente únicamente:

```text
ARS
USD
EUR
```

Antes de validar se normaliza mediante:

```text
Trim().ToUpperInvariant()
```

El valor por defecto es:

```text
ARS
```

No existe actualmente un catálogo dinámico de monedas.

---

# 12. IVA

`IvaPorcentaje` posee rango:

```text
0 <= IvaPorcentaje <= 100
```

El valor por defecto actual es:

```text
21
```

Este campo representa el porcentaje de IVA configurado para la Empresa.

La existencia del campo no implica por sí sola que Veltika posea actualmente un módulo fiscal completo o facturación electrónica integrada con ARCA.

---

# 13. Monto de Venta importante

`MontoVentaImportante` es opcional.

Cuando se informa debe ser mayor o igual a:

```text
0.01
```

Su objetivo es proporcionar un umbral configurable para identificar Ventas relevantes según los usos que hagan otros componentes del sistema.

No debe confundirse con un límite máximo de Venta ni con una autorización financiera.

---

# 14. Logo de Empresa

La configuración permite:

- Mantener el logo actual.
- Subir un nuevo logo.
- Eliminar el logo.

La persistencia almacena la ruta en:

```text
LogoRuta
```

El archivo se procesa mediante:

```text
IImagenService
```

El controller utiliza la carpeta lógica:

```text
logos
```

asociada al `EmpresaId`.

---

# 15. Reemplazo de logo

Si se carga un nuevo logo:

1. Se intenta guardar primero la nueva imagen.
2. Si la imagen no es válida, la configuración no se persiste.
3. Si el guardado de base de datos falla, se elimina la nueva imagen creada.
4. Sólo después de guardar correctamente se elimina el archivo anterior.

Este flujo evita perder el logo vigente ante un error durante la actualización.

---

# 16. Eliminación de logo

Si:

```text
EliminarLogo == true
```

y no se carga una nueva imagen, se establece:

```text
LogoRuta = null
```

Después de persistir correctamente, el archivo anterior se elimina mediante `IImagenService`.

---

# 17. Validación de Empresa

La Empresa seleccionada debe:

- Existir.
- Estar activa.

Para AdminEmpresa el contexto queda forzado a:

```text
usuario.EmpresaId
```

Para SuperAdmin el `EmpresaId` seleccionado se valida nuevamente en servidor.

---

# 18. Seguridad multiempresa

La configuración pertenece a una única Empresa mediante:

```text
ConfiguracionEmpresa.EmpresaId
```

Reglas:

1. AdminEmpresa no puede modificar la configuración de otra Empresa.
2. SuperAdmin puede cambiar de contexto empresarial.
3. Toda Empresa recibida desde formularios se valida en servidor.
4. Los archivos de logo se guardan utilizando el contexto de Empresa correspondiente.
5. La configuración de una Empresa no debe afectar a otra.

---

# 19. Persistencia

El flujo de guardado actual:

1. Obtiene el usuario autenticado.
2. Determina la Empresa permitida.
3. Normaliza y valida la Moneda.
4. Valida la Empresa activa.
5. Busca ConfiguracionEmpresa existente.
6. La crea si todavía no existe.
7. Procesa el logo si corresponde.
8. Normaliza textos.
9. Actualiza los campos.
10. Ejecuta `SaveChangesAsync()`.
11. Gestiona correctamente la imagen anterior/nueva según el resultado.

---

# 20. Valores por defecto

Los valores utilizados actualmente cuando no existe configuración son:

```text
RazonSocial = Empresa.Nombre
Moneda = ARS
IvaPorcentaje = 21
```

No existe actualmente un endpoint de "Restaurar configuración" que restablezca todos los parámetros a valores iniciales.

Por lo tanto, la restauración automática que figuraba en documentación anterior no debe considerarse funcionalidad implementada.

---

# 21. Auditoría

Actualmente ConfiguracionController no crea explícitamente un registro de auditoría por cada modificación.

Por lo tanto, no debe documentarse como regla vigente que toda actualización de configuración genera automáticamente una Auditoría.

La auditoría administrativa general puede incorporarse como evolución futura junto con el historial de actividad del sistema.

---

# 22. Configuración centralizada

No debe interpretarse este módulo como el único lugar posible para toda regla configurable de Veltika.

Actualmente existen configuraciones propias de otros dominios, por ejemplo:

- `Caja.FondoFijo`.
- `Caja.PermiteTurnos`.
- `Producto.PuntoReposicion`.
- Relaciones Caja ↔ MedioPago.

Por lo tanto, `ConfiguracionEmpresa` concentra configuración general de Empresa, pero no reemplaza configuraciones propias de cada módulo.

---

# 23. Stock negativo

Actualmente no existe en `ConfiguracionEmpresa` el campo:

```text
PermitirStockNegativo
```

Los flujos de inventario revisados impiden que el stock quede negativo.

Si en el futuro se desea volver configurable esta política, deberá diseñarse explícitamente y revisarse en todos los flujos de Venta, ajuste, Compra/anulación, reintegros y devoluciones que afecten stock.

---

# 24. Zona horaria e idioma

Actualmente no existen en ConfiguracionEmpresa:

```text
ZonaHoraria
Idioma
```

Las fechas se generan en distintos flujos mediante el horario del servidor, generalmente con `DateTime.Now`.

Una futura configuración de zona horaria requerirá una decisión transversal de arquitectura y no debería agregarse únicamente como un campo visual sin revisar persistencia y presentación de fechas.

---

# 25. Configuración por Sucursal

Actualmente no existe el módulo productivo Sucursal ni una relación:

```text
ConfiguracionEmpresa -> Sucursal
```

La configuración vigente es por Empresa.

La eventual configuración por Sucursal dependerá primero del diseño e implementación del dominio multi-sucursal.

---

# 26. Reglas de negocio

1. ConfiguracionEmpresa pertenece a una Empresa.
2. El módulo actual administra información general, comercial, fiscal básica e identidad visual.
3. Razón Social es obligatoria.
4. Moneda es obligatoria.
5. Las monedas admitidas actualmente son ARS, USD y EUR.
6. IVA debe estar entre 0 y 100.
7. MontoVentaImportante es opcional y, si existe, debe ser mayor a cero.
8. Email es opcional pero debe poseer formato válido.
9. Los textos opcionales vacíos se normalizan a `null`.
10. AdminEmpresa sólo configura su Empresa.
11. SuperAdmin puede configurar múltiples Empresas.
12. La Empresa debe estar activa.
13. Si no existe ConfiguracionEmpresa, se crea al guardar.
14. Sin configuración persistida se utilizan valores por defecto para Razón Social, Moneda e IVA.
15. El logo se administra mediante IImagenService.
16. Un error de persistencia no debe eliminar prematuramente el logo anterior.
17. No existe actualmente restauración automática a valores por defecto.
18. No existe auditoría automática explícita en este controller.
19. No existe actualmente PermitirStockNegativo.
20. No existen actualmente Idioma ni ZonaHoraria.
21. No existe actualmente configuración por Sucursal.

---

# 27. Casos de error relevantes

- Usuario no autenticado.
- Usuario sin rol autorizado.
- Empresa inexistente.
- Empresa inactiva.
- Empresa no seleccionada por SuperAdmin.
- Moneda fuera de ARS/USD/EUR.
- Razón social vacía.
- IVA fuera de rango.
- MontoVentaImportante inválido.
- Email con formato inválido.
- Logo inválido.
- Error de persistencia en base de datos.

---

# 28. Integraciones actuales

ConfiguracionEmpresa se relaciona actualmente con:

- Empresa.
- IImagenService.
- Componentes que consuman Moneda, IVA, datos comerciales, logo o MontoVentaImportante.

No debe asumirse integración automática con todos los módulos sólo por existir el campo de configuración.

---

# 29. Capacidades futuras

Entre las posibles evoluciones se encuentran:

- Configuración fiscal avanzada.
- Integración con ARCA.
- Zona horaria.
- Idioma.
- Formatos regionales.
- Numeración comercial configurable.
- Parámetros por Sucursal.
- Parámetros por Usuario.
- Plantillas de configuración.
- Historial y auditoría de cambios.
- Exportación/importación de configuración.
- Políticas configurables con permisos granulares.

Estas capacidades deben gestionarse mediante Roadmap e Issues y no asumirse como implementadas.

---

# 30. Estado actual

✅ Configuración por Empresa implementada.

✅ Razón social implementada.

✅ CUIT, dirección, teléfono y email implementados.

✅ Monedas ARS/USD/EUR implementadas.

✅ IVA configurable implementado.

✅ Umbral de Venta importante implementado.

✅ Logo de Empresa implementado.

✅ Alta automática de ConfiguracionEmpresa al primer guardado implementada.

✅ Seguridad SuperAdmin/AdminEmpresa implementada.

🚧 Auditoría de cambios pendiente.

🚧 Configuración regional avanzada pendiente.

🚧 Configuración fiscal avanzada/ARCA pendiente.

🚧 Configuración por Sucursal pendiente.