# Arquitectura General - Veltika

Versión: 2.0

Autor: Mariano Gabriel Cardoso

Última actualización: 01/09/2026

---

# 1. Introducción

Veltika es una plataforma SaaS de gestión comercial orientada a pequeños y medianos comercios.

El sistema centraliza la operación diaria de cada empresa mediante módulos de productos, clientes, proveedores, ventas, compras, inventario, caja, gastos, reportes y configuración.

La arquitectura prioriza:

- Seguridad multiempresa.
- Simplicidad.
- Código mantenible.
- Trazabilidad de operaciones.
- Evolución incremental.
- Consistencia entre módulos.
- Preparación para operar como producto SaaS.

Veltika ya no se encuentra en una etapa inicial de construcción. Actualmente dispone de una base funcional amplia y de una infraestructura productiva operativa, por lo que el foco arquitectónico está puesto en estabilizar el MVP, mantener la calidad técnica y preparar el sistema para usuarios reales.

---

# 2. Objetivos arquitectónicos

La arquitectura debe permitir:

- Aislar completamente los datos de cada empresa.
- Mantener las reglas de negocio en el servidor.
- Evitar duplicación de lógica.
- Mantener Controllers y Views simples y consistentes.
- Preservar información histórica de operaciones comerciales.
- Garantizar trazabilidad de stock y caja.
- Incorporar nuevos módulos sin romper funcionalidades existentes.
- Mantener una infraestructura suficientemente simple para la etapa actual.
- Escalar únicamente cuando exista una necesidad técnica o comercial real.

La prioridad no es construir una arquitectura teóricamente perfecta, sino una arquitectura clara, segura y sostenible para el tamaño actual de Veltika.

---

# 3. Stack tecnológico actual

## Backend

- C#.
- ASP.NET Core MVC (.NET 9).
- Entity Framework Core.
- ASP.NET Core Identity.

## Base de datos

- SQL Server.
- Entity Framework Core Code First.
- Migraciones de Entity Framework.

## Frontend

- Razor Views.
- HTML.
- CSS.
- JavaScript.
- Bootstrap.

## Infraestructura

- AWS.
- Amazon EC2.
- Windows Server.
- IIS.
- SQL Server Express en la infraestructura productiva actual.
- Amazon S3 para backups externos de base de datos.

## Herramientas

- Git.
- GitHub.
- Visual Studio / Visual Studio Code según el entorno de trabajo.
- PowerShell para automatización de deploy y tareas operativas.

---

# 4. Arquitectura de aplicación

Veltika utiliza actualmente una arquitectura MVC monolítica modular.

Flujo general:

```text
Usuario
  ↓
Razor View / JavaScript
  ↓
Controller
  ↓
Reglas de negocio / Servicios cuando corresponde
  ↓
Entity Framework Core
  ↓
SQL Server
```

No se utilizan repositorios genéricos como abstracción obligatoria sobre Entity Framework Core.

Los Services se incorporan cuando permiten encapsular lógica reutilizable o reducir responsabilidades de un Controller, pero no se fuerza una capa adicional cuando no aporta valor.

La arquitectura debe evolucionar por necesidad real y no por cantidad de capas.

---

# 5. Responsabilidades

## Views

Responsables de:

- Presentación de información.
- Formularios.
- Interacción del usuario.
- Validaciones de experiencia de usuario.
- Comportamiento visual mediante JavaScript cuando corresponda.

Las Views no deben implementar reglas de negocio críticas.

## Controllers

Responsables de:

- Recibir solicitudes HTTP.
- Validar acceso y contexto de empresa.
- Coordinar operaciones.
- Aplicar o delegar reglas de negocio.
- Persistir cambios mediante Entity Framework Core.
- Preparar ViewModels y respuestas.

Los Controllers deben mantenerse legibles y evitar duplicación de lógica.

## Services

Se utilizan cuando una regla o proceso:

- Es reutilizado por distintos Controllers.
- Posee suficiente complejidad como para justificar una responsabilidad independiente.
- Representa una integración externa.
- Permite simplificar significativamente un Controller.

No toda operación necesita obligatoriamente un Service.

## Entity Framework Core

Responsable de:

- Persistencia.
- Consultas.
- Relaciones entre entidades.
- Transacciones cuando corresponda.
- Migraciones de esquema.

---

# 6. Arquitectura SaaS multiempresa

Veltika utiliza una arquitectura SaaS multiempresa con una base de datos compartida y separación lógica de información mediante `EmpresaId`.

Principio fundamental:

> Un usuario nunca debe poder acceder, modificar o relacionar información perteneciente a otra empresa.

Las entidades dependientes de una empresa deben encontrarse asociadas a ella directa o indirectamente.

Ejemplos:

```text
Empresa
├── Usuarios
├── Categorías
├── Productos
├── Clientes
├── Proveedores
├── Ventas
├── Compras
├── Stock
├── Cajas
├── Gastos
└── Configuración
```

La seguridad multiempresa debe aplicarse en el servidor. Nunca debe confiarse únicamente en IDs enviados desde formularios, URLs o JavaScript.

Toda consulta y operación sensible debe validar el contexto de empresa correspondiente.

---

# 7. Seguridad y autorización

Veltika utiliza ASP.NET Core Identity para autenticación y gestión de usuarios.

La autorización combina:

- Usuario autenticado.
- Empresa asociada.
- Roles.
- Reglas específicas según la operación.

Principios:

- Las reglas de seguridad se validan en servidor.
- No se confía en datos sensibles enviados por el cliente.
- Las operaciones deben validar pertenencia a empresa.
- Las credenciales y secretos productivos no se almacenan en Git.
- Las funcionalidades administrativas deben respetar los roles habilitados.

Los permisos granulares por empleado forman parte de la evolución futura y deberán incorporarse sin debilitar las reglas actuales de autorización.

---

# 8. Persistencia y reglas de datos

## Baja lógica

Los módulos administrativos utilizan baja lógica cuando corresponde.

Un registro desactivado puede mantenerse para preservar relaciones e historial y, cuando la regla de negocio lo permite, reactivarse desde su edición.

La eliminación física no debe utilizarse cuando pueda comprometer trazabilidad o relaciones históricas.

## Datos históricos

Las operaciones comerciales deben conservar la información necesaria para interpretar correctamente el pasado aunque cambien datos maestros posteriores.

Ejemplo:

`DetalleVenta` conserva el precio aplicado durante la operación para que una modificación posterior del producto no altere la venta histórica.

## Transacciones

Las operaciones que modifican múltiples entidades relacionadas deben utilizar transacciones cuando sea necesario para evitar estados parciales.

Especialmente en procesos como:

- Ventas.
- Compras.
- Cobros.
- Pagos.
- Reintegros.
- Devoluciones.
- Movimientos financieros.
- Operaciones de stock relacionadas.

---

# 9. Arquitectura de inventario

El inventario debe mantener trazabilidad de sus modificaciones.

Los cambios relevantes de stock generan movimientos que permiten identificar:

- Producto.
- Cantidad.
- Tipo de movimiento.
- Origen de la operación.
- Fecha.
- Empresa.

El stock puede verse afectado por distintos procesos:

- Compras.
- Ventas.
- Devoluciones.
- Reintegros.
- Ajustes.
- Importaciones.

El sistema también dispone de reportes de stock, punto de reposición y valorización de inventario.

La evolución futura puede incorporar inventario físico, múltiples depósitos, lotes, vencimientos y otras capacidades sin reemplazar la trazabilidad existente.

---

# 10. Arquitectura de caja

Los movimientos financieros operativos se registran mediante cajas y movimientos de caja.

Actualmente la arquitectura contempla:

- Cajas.
- Medios de pago.
- Asociación entre cajas y medios de pago.
- Movimientos de caja.
- Transferencias entre cajas.
- Cobros de ventas.
- Pagos de compras.
- Gastos.
- Turnos de caja.
- Apertura y cierre.
- Fondo fijo.
- Efectivo esperado.
- Efectivo contado.
- Diferencia de caja.
- Importe rendido.

El saldo no debe alterarse mediante modificaciones arbitrarias que eliminen la trazabilidad del origen del movimiento.

---

# 11. Arquitectura funcional actual

## Núcleo y administración

- Empresa.
- Configuración.
- Usuarios.
- Roles.

## Catálogo

- Categorías.
- Productos.
- Imágenes de productos.
- Importación masiva de productos y stock.

## Comercial

- Clientes.
- Proveedores.

## Ventas

- Ventas.
- Detalles de venta.
- POS.
- Múltiples medios de pago.
- Cobros.
- Saldos pendientes.
- Reintegros/devoluciones de venta.

## Compras

- Compras.
- Detalles de compra.
- Pagos a proveedores.
- Saldos pendientes.
- Devoluciones/reintegros de compra.

## Inventario

- Stock.
- Movimientos de stock.
- Ajustes.
- Punto de reposición.
- Valorización.

## Caja y finanzas operativas

- Cajas.
- Medios de pago.
- Movimientos.
- Transferencias.
- Turnos de caja.
- Apertura, cierre y arqueo.
- Gastos.
- Categorías de gasto.

## Información de gestión

- Dashboard.
- Reportes de ventas.
- Reportes de stock.
- Exportaciones a Excel.

## Web pública

Veltika dispone de una experiencia pública separada de la aplicación autenticada.

La web pública tiene como objetivo presentar el producto, facilitar el acceso al inicio de sesión y evolucionar hacia una herramienta de adquisición, posicionamiento y conversión.

---

# 12. ViewModels

Las Views que requieren información específica no deben depender innecesariamente de entidades de persistencia completas.

Se utilizan ViewModels para:

- Formularios complejos.
- Combinar información de múltiples entidades.
- Exponer únicamente los campos necesarios.
- Evitar overposting.
- Adaptar datos a necesidades concretas de la interfaz.

Los ViewModels forman parte del contrato entre Controller y View y no representan tablas de base de datos.

---

# 13. Frontend y layouts

La aplicación utiliza Razor Views con estilos y componentes compartidos.

La arquitectura visual distingue dos contextos principales:

## Web pública

Utiliza un layout orientado a visitantes no autenticados y presentación comercial de Veltika.

## Aplicación

Utiliza un layout orientado a usuarios autenticados y navegación entre módulos operativos.

Ambos contextos pueden compartir componentes y estilos generales sin convertir el layout en una estructura única cargada de condiciones.

Los estilos reutilizables deben centralizar patrones comunes como:

- Botones.
- Formularios.
- Inputs.
- Tablas.
- Filtros.
- Títulos.
- Estados visuales.
- Componentes compartidos.

Los estilos específicos de una pantalla o módulo deben mantenerse separados cuando corresponda.

---

# 14. Infraestructura productiva

La arquitectura productiva actual es:

```text
Internet
  ↓
Dominio / DNS
  ↓
AWS EC2
  ↓
IIS
  ↓
ASP.NET Core MVC
  ↓
SQL Server Express
```

Componentes complementarios:

- Elastic IP.
- Variables de entorno de Windows para configuración sensible.
- Amazon S3 para backups externos.
- Scripts PowerShell para empaquetado e instalación.
- Backups previos y posteriores al deploy.
- Smoke tests posteriores al despliegue.

El detalle operativo se mantiene en:

- `Infraestructura Veltika.md`.
- `Configuracion de produccion.md`.
- `Guia de deploy Veltika.md`.

La infraestructura actual debe mantenerse simple mientras sea suficiente para la cantidad real de usuarios y carga del sistema.

---

# 15. Despliegue

El despliegue productivo utiliza un proceso asistido mediante PowerShell.

El flujo contempla:

1. Build y tests en Release.
2. Publicación de la aplicación.
3. Generación de migraciones idempotentes.
4. Creación del paquete de deploy.
5. Validación de integridad mediante SHA256.
6. Backup previo.
7. Aplicación de migraciones.
8. Reemplazo controlado de archivos.
9. Conservación de uploads productivos.
10. Reinicio de IIS.
11. Verificación HTTP.
12. Backup posterior.

La automatización debe reducir errores humanos sin eliminar controles importantes antes de modificar producción.

---

# 16. Principios de evolución arquitectónica

Veltika seguirá estas reglas al crecer:

1. No agregar una capa arquitectónica únicamente por patrón o moda.
2. Refactorizar cuando exista duplicación o una responsabilidad haya crecido demasiado.
3. Crear Services cuando exista una necesidad concreta de reutilización o separación.
4. Mantener Entity Framework Core como herramienta principal de persistencia mientras continúe siendo adecuada.
5. No introducir repositorios genéricos si sólo duplican las capacidades de EF Core.
6. No dividir el sistema en microservicios sin una necesidad técnica comprobada.
7. Mantener seguridad multiempresa como requisito transversal.
8. Mantener trazabilidad en operaciones comerciales, financieras y de inventario.
9. Priorizar compatibilidad y migraciones seguras sobre rediseños innecesarios.
10. Escalar infraestructura según métricas y uso real.

---

# 17. Decisiones arquitectónicas

Las decisiones relevantes se documentan mediante Architecture Decision Records (ADR) en:

`Docs/02-Decisiones/`

Los ADR permiten registrar por qué se tomó una decisión y evitar que futuras modificaciones desconozcan restricciones o razones históricas importantes.

Cuando una decisión arquitectónica existente cambie, deberá actualizarse o reemplazarse mediante un nuevo ADR según corresponda.

---

# 18. Roadmap y alcance futuro

Este documento describe la arquitectura actual y sus principios, no el backlog funcional.

El detalle de evolución futura se mantiene en:

- `Roadmap.md` para la visión general.
- GitHub Issues para funcionalidades concretas.
- Issue #27 para mejoras y evolución post-MVP.

Entre las capacidades futuras previstas se encuentran, sujetas a validación:

- Sucursales y depósitos.
- Inventario avanzado.
- Listas de precios.
- Órdenes de compra.
- Cuenta corriente avanzada.
- Inteligencia de negocio.
- Automatizaciones.
- Facturación electrónica.
- Integraciones externas.
- API pública.
- Evolución comercial del SaaS.

Estas funcionalidades no deben considerarse implementadas hasta que el código y la documentación correspondiente lo confirmen.

---

# 19. Estado actual del proyecto

Estado general:

✅ Arquitectura SaaS multiempresa operativa.

✅ Módulos administrativos principales operativos.

✅ Ventas y POS operativos.

✅ Compras y proveedores operativos.

✅ Inventario y trazabilidad de stock operativos.

✅ Caja, movimientos y turnos operativos.

✅ Dashboard y reportes base operativos.

✅ Aplicación desplegada en infraestructura AWS productiva.

✅ Dominio y configuración de producción disponibles.

✅ Proceso de deploy y recuperación documentado.

🚧 Web pública y experiencia visual en evolución.

🚧 Preparación y estabilización del MVP para pruebas con usuarios reales.

🚧 Documentación funcional en proceso de sincronización con el estado actual del código.

---

# 20. Próximo objetivo arquitectónico

El próximo objetivo no es incorporar nuevas capas o rediseñar la solución.

La prioridad arquitectónica es consolidar el MVP existente:

1. Mantener sincronizadas reglas de negocio y código.
2. Completar pruebas de flujos críticos.
3. Revisar seguridad multiempresa antes de pilotos reales.
4. Continuar reduciendo duplicación cuando aparezca.
5. Mantener Controllers y Views manejables mediante refactorizaciones puntuales.
6. Validar rendimiento con datos y uso reales.
7. Mejorar observabilidad y recuperación productiva.
8. Incorporar nuevas capacidades únicamente cuando el producto las necesite.

La arquitectura deberá continuar evolucionando de forma incremental, evitando tanto la deuda técnica como la sobrearquitectura.