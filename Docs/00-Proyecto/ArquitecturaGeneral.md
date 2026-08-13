# Arquitectura General - Veltika

Versión: 1.0

Autor: Mariano Gabriel Cardoso

Última actualización: 22/07/2026

---

# 1. Introducción

Veltika es un sistema de gestión comercial desarrollado bajo una arquitectura SaaS (Software as a Service), diseñado para pequeñas y medianas empresas que necesitan administrar sus operaciones comerciales desde una única plataforma.

El sistema busca centralizar la gestión de productos, clientes, proveedores, ventas, compras, stock, caja y reportes, ofreciendo una solución escalable, segura y preparada para crecer junto con cada empresa.

Desde su diseño inicial, Veltika fue concebido como un proyecto modular, permitiendo incorporar nuevas funcionalidades sin modificar la estructura principal del sistema.

---

# 2. Objetivos del proyecto

Los principales objetivos de Veltika son:

- Centralizar la administración del negocio.
- Simplificar los procesos comerciales.
- Mantener la información organizada.
- Automatizar tareas repetitivas.
- Garantizar la trazabilidad de todas las operaciones.
- Permitir el crecimiento futuro del sistema sin rediseñar la arquitectura.
- Brindar una experiencia moderna, rápida e intuitiva.

---

# 3. Arquitectura del sistema

Veltika se desarrolla utilizando una arquitectura en capas basada en el patrón MVC.

Arquitectura utilizada:

- ASP.NET Core MVC
- Entity Framework Core
- SQL Server
- Razor Views
- Bootstrap
- Servicios (Services)
- Repositorios (en futuras versiones)

La aplicación se divide en cuatro grandes capas.

```

Presentación

↓

Controllers

↓

Services

↓

Entity Framework

↓

SQL Server

```

Cada capa posee una única responsabilidad, evitando dependencias innecesarias.

---

# 4. Filosofía del proyecto

Durante el desarrollo se seguirán los siguientes principios.

## Simplicidad

Toda solución deberá ser lo más simple posible.

Se evitarán desarrollos complejos cuando exista una alternativa sencilla.

---

## Escalabilidad

Todo módulo deberá diseñarse pensando en futuras versiones.

El crecimiento del sistema no deberá requerir reescribir módulos existentes.

---

## Modularidad

Cada módulo será independiente.

Las funcionalidades deberán encontrarse correctamente separadas.

---

## Código limpio

Se priorizará:

- Legibilidad.
- Nombres descriptivos.
- Métodos pequeños.
- Baja complejidad.

---

## Separación de responsabilidades

Cada clase tendrá una única responsabilidad.

Controllers

→ reciben solicitudes.

Services

→ ejecutan reglas de negocio.

Entity Framework

→ acceso a datos.

Views

→ presentación.

---

# 5. Arquitectura SaaS

Toda la información pertenece a una empresa.

Cada registro del sistema deberá estar asociado a una Empresa.

Ejemplo

Empresa

↓

Productos

Clientes

Ventas

Compras

Caja

Usuarios

Reportes

Ninguna empresa podrá acceder a información perteneciente a otra.

La separación de datos constituye una regla fundamental del proyecto.

---

# 6. Arquitectura funcional

Actualmente Veltika se encuentra dividido en los siguientes módulos.

## Núcleo

- Empresa
- Sucursal
- Configuración

---

## Seguridad

- Usuario
- Rol
- Permiso
- Auditoría

---

## Catálogo

- Categoría
- Producto

---

## Comercial

- Cliente
- Proveedor

---

## Ventas

- Venta
- DetalleVenta

---

## Compras

- Compra
- DetalleCompra

---

## Inventario

- Stock
- MovimientoStock
- AjusteStock

---

## Caja

- Caja
- MovimientoCaja

---

## Inteligencia del negocio

- Reportes
- Dashboard

---

# 7. Principios generales del sistema

Durante el desarrollo deberán respetarse las siguientes reglas.

## Eliminación lógica

La información nunca será eliminada físicamente.

Se utilizará un campo Estado.

---

## Auditoría

Toda operación importante será registrada.

El historial nunca podrá modificarse.

---

## Stock

El stock nunca será modificado directamente.

Todo cambio generará un MovimientoStock.

---

## Caja

La caja nunca modificará manualmente su saldo.

Todo ingreso o egreso generará un MovimientoCaja.

---

## Seguridad

Todos los permisos serán controlados mediante Roles.

---

## Multiempresa

Cada empresa visualizará únicamente su propia información.

---

# 8. Convenciones de desarrollo

Se utilizarán las siguientes convenciones.

## Clases

Singular.

Ejemplo

Producto

Venta

Cliente

---

## Tablas

Plural.

Ejemplo

Productos

Ventas

Clientes

---

## Claves primarias

Id

---

## Claves foráneas

ProductoId

CategoriaId

EmpresaId

UsuarioId

---

## Navegación

Se utilizarán propiedades virtuales para Entity Framework.

Ejemplo

public virtual Empresa Empresa { get; set; }

---

## Validaciones

Se utilizarán Data Annotations siempre que sea posible.

Las reglas de negocio complejas se implementarán dentro de Services.

---

# 9. Roadmap de desarrollo

El desarrollo seguirá el siguiente orden.

Sprint 1

- Empresa
- Categoría
- Producto

Sprint 2

- Usuario
- Rol
- Permiso

Sprint 3

- Cliente
- Proveedor

Sprint 4

- Venta
- DetalleVenta

Sprint 5

- Compra
- DetalleCompra

Sprint 6

- Stock
- MovimientoStock
- AjusteStock

Sprint 7

- Caja
- MovimientoCaja

Sprint 8

- Reportes
- Dashboard

---

# 10. Tecnologías

Backend

- C#
- ASP.NET Core MVC
- Entity Framework Core

Base de datos

- SQL Server

Frontend

- Razor
- HTML
- CSS
- Bootstrap
- JavaScript

Control de versiones

- Git
- GitHub

IDE

- Visual Studio 2022

---

# 11. Versionado

Versión 1.0

Objetivo:

Sistema completamente funcional para una empresa.

Incluye:

- Productos
- Clientes
- Proveedores
- Ventas
- Compras
- Stock
- Caja
- Reportes

---

Versión 2.0

Objetivo:

Escalabilidad.

Incluye:

- Multi sucursal
- Facturación
- Configuración avanzada
- Reportes avanzados

---

Versión 3.0

Objetivo:

Convertir Veltika en una plataforma SaaS completa.

Incluye:

- API pública
- Integraciones
- Marketplace
- Inteligencia Artificial
- Automatizaciones

---

# 12. Objetivo final

Veltika no busca ser únicamente un sistema de gestión.

El objetivo es construir una plataforma moderna, escalable y profesional que pueda evolucionar continuamente incorporando nuevos módulos y funcionalidades sin comprometer la arquitectura existente.

Cada decisión tomada durante el desarrollo deberá priorizar la mantenibilidad, la simplicidad y la posibilidad de crecimiento del proyecto.

---

# 13. Estado actual del proyecto

Estado de la documentación:

✅ Arquitectura general finalizada.

✅ 22 módulos funcionales documentados.

Estado del desarrollo:

🚧 Inicio de implementación del módulo Empresa.

Próximo objetivo:

Desarrollar el primer sprint del sistema respetando la arquitectura definida en esta documentación.