# Veltika

<p align="center">
  <img src="saas/wwwroot/brand/logos/veltika-logo-horizontal.svg" alt="Veltika" width="260">
</p>

<p align="center">
  Sistema SaaS de gestión para comercios, desarrollado con ASP.NET Core MVC y arquitectura multiempresa.
</p>

<p align="center">
  <img src="https://img.shields.io/badge/Estado-Piloto-15558d" alt="Estado: Piloto">
  <img src="https://img.shields.io/badge/.NET-9-512bd4" alt=".NET 9">
  <img src="https://img.shields.io/badge/ASP.NET%20Core-MVC-512bd4" alt="ASP.NET Core MVC">
  <img src="https://img.shields.io/badge/Entity%20Framework-Core-6c3483" alt="Entity Framework Core">
  <img src="https://img.shields.io/badge/SQL%20Server-Database-cc2927" alt="SQL Server">
</p>

## Descripción

**Veltika** centraliza la operación cotidiana de pequeños comercios en una única plataforma: ventas, compras, inventario, caja, clientes, proveedores y reportes.

El producto utiliza una arquitectura **multiempresa**. Cada organización trabaja con sus propios usuarios y datos, con aislamiento por empresa y autorización basada en roles. Veltika se encuentra en una etapa de **validación y piloto**, con foco en consolidar la versión 1.0 a partir del uso real.

El proyecto nació como una iniciativa de aprendizaje y evolucionó hasta convertirse en un producto SaaS funcional, desplegado en ambientes separados de producción y QA.

## Propuesta de valor

- Reunir la información comercial y operativa en un solo lugar.
- Mantener trazabilidad de ventas, compras, stock y movimientos financieros.
- Ofrecer una experiencia consistente y accesible para tareas frecuentes.
- Facilitar la toma de decisiones mediante indicadores y reportes.
- Permitir que distintas empresas operen de forma aislada sobre la misma aplicación.

## Módulos actuales

- **Empresas y usuarios:** administración de empresas, usuarios, roles, estados y permisos.
- **Productos y categorías:** catálogo, precios, costos, imágenes, categorías e importación de productos.
- **Clientes y proveedores:** datos comerciales, estado, historial y búsqueda.
- **Ventas / POS:** búsqueda de productos y clientes, múltiples pagos, cobros y seguimiento de saldos.
- **Compras:** registro de compras, productos, proveedores, pagos y seguimiento de deuda.
- **Stock:** control de existencias, ajustes e historial de movimientos.
- **Caja:** cajas, medios de pago, turnos, movimientos, transferencias y regularizaciones.
- **Devoluciones y reintegros:** devoluciones de compras, reintegros de ventas y proveedores, con sus anulaciones.
- **Reportes:** análisis de ventas, stock, productos y clientes, con filtros y exportación cuando corresponde.
- **Dashboard:** resumen operativo, indicadores y evolución reciente del negocio.
- **Configuración de empresa:** identidad visual y preferencias propias de cada comercio.
- **Notificaciones:** avisos internos vinculados con eventos relevantes del sistema.

## Experiencia de uso

- Diseño responsive y consistente entre módulos.
- Buscadores dinámicos y filtros combinables.
- Tablas paginadas y ordenables.
- Formularios con validaciones y mensajes en español.
- Guías de ayuda contextuales.
- Conservación del contexto de navegación al volver de una operación.
- Interfaz alineada con la identidad visual de Veltika.

## Stack técnico

- .NET 9
- ASP.NET Core MVC
- Entity Framework Core 9
- ASP.NET Core Identity
- SQL Server
- LINQ
- Bootstrap 5
- Razor Views y View Components
- ClosedXML para generación de archivos Excel
- MailKit para correo electrónico
- SkiaSharp para procesamiento de imágenes

## Arquitectura

La aplicación mantiene el patrón MVC y separa las responsabilidades principales en:

```text
saas/
├── Controllers/       # Entrada HTTP, autorización y coordinación de operaciones
├── Services/          # Lógica compartida y servicios de aplicación
├── Models/            # Entidades del dominio
├── ViewModel/         # Modelos específicos para vistas y operaciones
├── Views/             # Interfaz Razor MVC
├── ViewComponents/    # Componentes visuales reutilizables
├── Data/              # DbContext, configuración y datos iniciales controlados
├── Migrations/        # Evolución versionada del esquema de base de datos
├── Configuracion/     # Configuración transversal de la aplicación
├── Helpers/           # Utilidades compartidas
├── Settings/          # Opciones tipadas de configuración
└── wwwroot/           # Estilos, scripts y recursos públicos
```

Entre las decisiones implementadas se encuentran:

- Entity Framework Core con migraciones y configuración Fluent API.
- Servicios reutilizables para operaciones compartidas entre módulos.
- ViewModels específicos para no exponer directamente las entidades en formularios complejos.
- Bajas lógicas y trazabilidad en los módulos donde corresponde.
- Componentes y estilos centralizados para mantener consistencia visual.

## Seguridad y aislamiento

- Autenticación y gestión de usuarios mediante ASP.NET Core Identity.
- Autorización basada en roles.
- Aislamiento de información por empresa.
- Validaciones de permisos y pertenencia del lado servidor.
- Validaciones de entrada mediante ViewModels, Data Annotations y reglas de negocio.
- Protección de cookies, bloqueo por intentos fallidos y limitación de solicitudes de autenticación.
- Encabezados de seguridad HTTP y HTTPS obligatorio fuera del ambiente de desarrollo.
- Configuración sensible mediante variables de entorno o User Secrets, sin valores reales en el repositorio.
- Inicialización controlada de roles, sin credenciales predeterminadas versionadas.

## Instalación local

### Requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- SQL Server o SQL Server Express
- Herramientas de Entity Framework Core 9

### Pasos

1. Clonar el repositorio:

   ```bash
   git clone https://github.com/Mcardoso92/sistema_gestion.git
   cd sistema_gestion/saas
   ```

2. Restaurar las dependencias:

   ```bash
   dotnet restore
   ```

3. Instalar la herramienta de migraciones si todavía no está disponible:

   ```bash
   dotnet tool install --global dotnet-ef --version 9.0.16
   ```

4. Configurar una conexión local sin modificar ni versionar archivos con secretos:

   ```bash
   dotnet user-secrets set "ConnectionStrings:SaasDbContext" "Server=.\\SQLEXPRESS;Database=Saas_DB;Integrated Security=true;Trust Server Certificate=true;"
   ```

5. Aplicar las migraciones:

   ```bash
   dotnet ef database update
   ```

6. Ejecutar la aplicación:

   ```bash
   dotnet run
   ```

La aplicación crea los roles necesarios al iniciar. Las cuentas se registran mediante los flujos controlados del sistema; el repositorio no incluye usuarios ni contraseñas predeterminadas.

## Configuración sensible

Los valores reales de conexión, correo y demás secretos deben configurarse mediante:

- **User Secrets** durante el desarrollo local.
- **Variables de entorno** en QA y producción.

No deben incorporarse al repositorio cadenas de conexión reales, contraseñas, certificados, backups ni paquetes de despliegue. El `.gitignore` incluye protecciones para estos archivos.

## Capturas

Esta sección está preparada para incorporar capturas actuales del producto durante el piloto. Antes de publicar una imagen se deben utilizar datos ficticios y verificar que no aparezcan usuarios, comercios, correos, documentos ni información operativa real.

Capturas previstas:

- Dashboard general.
- Punto de venta.
- Control de stock.
- Gestión de caja.
- Reportes.

## Estado y roadmap

### Veltika 1.0

La etapa actual está enfocada en consolidación funcional, QA, estabilización, centralización de componentes y validación con usuarios reales.

### Programa Piloto 2026

El piloto permitirá validar los flujos principales, detectar oportunidades de mejora y priorizar el backlog con evidencia de uso.

### Próximas etapas

- Mejoras post-MVP registradas y priorizadas en el backlog del proyecto.
- Evolución continua de rendimiento, seguridad, experiencia de uso y operación.
- **Facturación** como próximo gran módulo previsto; todavía no forma parte de las funcionalidades implementadas.

## Autor

**Mariano Cardoso** · [GitHub](https://github.com/Mcardoso92)

---

Veltika es un producto en evolución. La documentación refleja el estado actual del sistema y se actualiza junto con sus funcionalidades.
