# VELTIKA

> Sistema SaaS de gestión empresarial desarrollado con ASP.NET Core MVC.

![Estado](https://img.shields.io/badge/Estado-En%20Desarrollo-blue)
![.NET](https://img.shields.io/badge/.NET-9-purple)
![ASP.NET Core MVC](https://img.shields.io/badge/ASP.NET%20Core-MVC-blueviolet)
![Entity Framework Core](https://img.shields.io/badge/Entity%20Framework-Core-success)
![SQL Server](https://img.shields.io/badge/SQL%20Server-Database-red)

---

# 📖 Descripción

**VELTIKA** es un sistema SaaS de gestión empresarial (ERP) desarrollado como proyecto personal con el objetivo de profundizar conocimientos en **ASP.NET Core MVC**, **Entity Framework Core** y **ASP.NET Identity**, aplicando buenas prácticas de desarrollo, arquitectura y seguridad.

El sistema implementa una arquitectura **multiempresa**, donde cada organización administra exclusivamente su propia información mediante un aislamiento completo por empresa y un esquema de autorización basado en roles.

El desarrollo del proyecto se organiza mediante **Sprints** e **Issues** utilizando GitHub como herramienta de planificación y seguimiento.

---

# 🎯 Objetivos del proyecto

- Aplicar el patrón de arquitectura MVC.
- Desarrollar un sistema SaaS multiempresa.
- Implementar autenticación y autorización con ASP.NET Identity.
- Aplicar buenas prácticas con Entity Framework Core.
- Diseñar una arquitectura escalable y mantenible.
- Construir una base sólida para futuros módulos comerciales.

---

# 🚀 Tecnologías utilizadas

- ASP.NET Core MVC
- .NET 8
- Entity Framework Core
- ASP.NET Identity
- SQL Server
- LINQ
- Bootstrap 5
- Material Symbols
- Fluent API

---

# 🏗 Arquitectura

El proyecto sigue el patrón **Model - View - Controller (MVC)**.

La solución se encuentra organizada en:

```text
Controllers
Models
ViewModels
Views
Data
Migrations
wwwroot
```

Además se utilizan:

- ViewModels específicos para cada operación CRUD.
- Entity Framework Core (Code First).
- Fluent API para la configuración del modelo.
- ASP.NET Identity para autenticación y autorización.
- Soft Delete para preservar la integridad de la información.

---

# ✨ Funcionalidades implementadas

## 🔐 Seguridad

- Autenticación mediante ASP.NET Identity.
- Autorización basada en Roles.
- Arquitectura Multiempresa.
- Restricción de acceso por Empresa.
- Restricción de acceso por Rol.
- Protección contra acceso directo mediante URL.
- Validaciones de permisos en Controllers.
- Bloqueo de acceso para usuarios pertenecientes a empresas inactivas.

---

## 📦 Gestión de datos

- CRUD completo.
- Soft Delete.
- Reactivación de registros.
- Validaciones de negocio.
- Validaciones mediante DataAnnotations.
- Mensajes de éxito y error unificados.
- Prevención de registros duplicados.

---

## 🎨 Experiencia de usuario

- Buscadores dinámicos.
- Filtros combinables.
- Diseño responsive.
- Interfaz unificada entre todos los módulos.
- Breadcrumbs.
- Material Symbols.
- Mensajes de ayuda contextuales.

---

# 📦 Módulos implementados

## 🏢 Empresas

- CRUD completo.
- Soft Delete.
- Reactivación.
- Buscador.
- Filtro por Estado.

---

## 📂 Categorías

- CRUD completo.
- Soft Delete.
- Reactivación.
- Buscador.
- Filtro por Estado.
- Filtro por Empresa.

---

## 📦 Productos

- CRUD completo.
- Soft Delete.
- Reactivación.
- Buscador.
- Filtros por Estado, Empresa y Categoría.

---

## 👥 Usuarios

- CRUD completo.
- ASP.NET Identity.
- Gestión de Roles.
- Buscador.
- Filtros por Estado, Rol y Empresa.
- Restricciones por Empresa.
- Restricciones por Rol.

---

# 🔐 Modelo de seguridad

El sistema implementa un esquema de autorización basado en Roles.

## SuperAdmin

Tiene acceso completo al sistema.

Puede administrar:

- Empresas.
- Categorías.
- Productos.
- Usuarios.

---

## AdminEmpresa

Puede administrar únicamente la información perteneciente a su empresa.

No puede:

- Acceder a información de otras empresas.
- Administrar usuarios SuperAdmin.
- Asignar el rol SuperAdmin.
- Modificar su propio rol.
- Desactivar su propio usuario.

---

## Empresas inactivas

Cuando una empresa se encuentra inactiva:

- Conserva toda su información.
- Sus usuarios no pueden iniciar sesión.
- Puede reactivarse posteriormente.
- No aparece disponible en formularios de creación.

---

# 🗄 Base de datos

El proyecto utiliza **Entity Framework Core Code First**.

Características implementadas:

- Migraciones.
- Fluent API.
- Relaciones entre entidades.
- Índices únicos.
- Configuración de precisión decimal.
- Inicialización de colecciones.

---

# 📈 Estado del proyecto

## ✅ Sprint 1

- Empresas.
- Categorías.
- Productos.
- Usuarios.

---

## ✅ Sprint 1.5

- Refactorización completa.
- Optimización del código.
- Seguridad.
- Soft Delete.
- Reactivación.
- Buscadores.
- Filtros.
- Validaciones.
- Revisión general del proyecto.

---

## 🚧 Próximo Sprint

### Sprint 2

- Clientes.
- Punto de Venta.
- Ventas.
- Detalle de Venta.

---

# 📷 Capturas

Las capturas del sistema serán incorporadas una vez finalizado el Sprint 2.

---

# ⚙ Instalación

Clonar el repositorio:

```bash
git clone https://github.com/Mcardoso92/sistema_gestion.git
```

Ingresar al proyecto:

```bash
cd sistema_gestion/saas
```

Restaurar dependencias:

```bash
dotnet restore
```

Configurar la cadena de conexión en:

```text
appsettings.json
```

Aplicar migraciones:

```bash
dotnet ef database update
```

Ejecutar la aplicación:

```bash
dotnet run
```

---

# 📅 Roadmap

- ✅ Sprint 1 – Infraestructura y módulos administrativos.
- ✅ Sprint 1.5 – Refactorización, seguridad y optimización.
- 🚧 Sprint 2 – Ventas.
- ⏳ Sprint 3 – Compras e Inventario.
- ⏳ Sprint 4 – Reportes y Dashboard.

---

# 👨‍💻 Autor

**Mariano Cardoso**

Proyecto desarrollado con fines de aprendizaje, práctica profesional y portfolio.

GitHub: https://github.com/Mcardoso92
