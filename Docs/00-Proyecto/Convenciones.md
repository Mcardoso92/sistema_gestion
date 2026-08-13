# Convenciones de Desarrollo - Veltika

Versión: 1.0

Última actualización: 22/07/2026

---

# 1. Objetivo

Este documento establece las convenciones de desarrollo utilizadas en Veltika.

Su finalidad es mantener un código consistente, legible y fácil de mantener durante toda la vida del proyecto.

Todas las nuevas funcionalidades deberán respetar estas convenciones.

---

# 2. Principios de desarrollo

Durante el desarrollo del sistema se seguirán los siguientes principios.

- Simplicidad.
- Legibilidad.
- Escalabilidad.
- Reutilización.
- Modularidad.
- Mantenibilidad.

Siempre se priorizará un código fácil de entender antes que soluciones innecesariamente complejas.

---

# 3. Arquitectura

Veltika utiliza una arquitectura basada en ASP.NET Core MVC.

La responsabilidad de cada capa será la siguiente.

## Controllers

- Reciben las solicitudes del usuario.
- Validan el modelo.
- Invocan los Services.
- Devuelven la respuesta.

Los Controllers no deberán contener lógica de negocio.

---

## Services

Los Services serán responsables de implementar las reglas de negocio del sistema.

Toda lógica que no corresponda a la presentación deberá implementarse aquí.

---

## Entity Framework

Será el encargado del acceso a datos.

Las consultas y persistencia utilizarán Entity Framework Core.

---

## Views

Las Views únicamente mostrarán información al usuario.

No deberán contener lógica de negocio.

---

# 4. Convenciones de nombres

## Clases

Las clases utilizarán nombres en singular.

Ejemplos

- Empresa
- Producto
- Venta
- Cliente

---

## Tablas

Las tablas utilizarán nombres en plural.

Ejemplos

- Empresas
- Productos
- Ventas
- Clientes

---

## Propiedades

Las propiedades utilizarán PascalCase.

Ejemplos

- Nombre
- PrecioVenta
- FechaAlta

---

## Métodos

Los métodos utilizarán PascalCase y comenzarán con un verbo.

Ejemplos

- CrearProducto()
- ActualizarStock()
- RegistrarVenta()

---

## Variables locales

Las variables utilizarán camelCase.

Ejemplos

- producto
- cantidad
- precioFinal

---

# 5. Modelos

Todos los modelos deberán cumplir las siguientes reglas.

- Tendrán una clave primaria llamada Id.
- Utilizarán Data Annotations para las validaciones simples.
- Las relaciones se implementarán mediante propiedades de navegación.
- Las claves foráneas finalizarán con Id.

Ejemplo

- EmpresaId
- CategoriaId
- UsuarioId

---

# 6. Relaciones

Las relaciones utilizarán Entity Framework Core.

Ejemplo

Empresa

↓

Productos

↓

Ventas

↓

DetalleVenta

Las relaciones se implementarán mediante propiedades virtuales cuando corresponda.

---

# 7. Base de datos

La base de datos utilizada será SQL Server.

Las migraciones serán administradas mediante Entity Framework Core.

Las migraciones generadas no deberán modificarse manualmente.

---

# 8. Validaciones

Las validaciones se dividirán en dos grupos.

## Data Annotations

Se utilizarán para:

- Campos obligatorios.
- Longitud máxima.
- Rangos.
- Formatos.

## Services

Se utilizarán para reglas de negocio.

Ejemplos

- Producto duplicado.
- Stock insuficiente.
- Cliente inexistente.
- Caja cerrada.

---

# 9. Eliminación de información

Veltika utilizará eliminación lógica.

Los registros no serán eliminados físicamente.

Cada entidad que corresponda contará con un campo Estado.

La información histórica deberá conservarse.

---

# 10. Auditoría

Toda operación importante será registrada automáticamente.

Ejemplos

- Altas.
- Modificaciones.
- Activaciones.
- Desactivaciones.
- Ventas.
- Compras.
- Ajustes.
- Apertura y cierre de caja.

Los registros de auditoría nunca podrán modificarse ni eliminarse.

---

# 11. Multiempresa

Toda la información pertenecerá a una Empresa.

Ningún usuario podrá acceder a información perteneciente a otra empresa.

La separación de datos forma parte de la arquitectura principal del sistema.

---

# 12. Manejo de errores

Los errores deberán gestionarse de forma clara y consistente.

- Nunca mostrar mensajes técnicos al usuario.
- Mostrar mensajes comprensibles.
- Registrar errores críticos cuando corresponda.
- Validar siempre antes de guardar información.

---

# 13. Interfaz de usuario

La interfaz deberá cumplir las siguientes reglas.

- Diseño limpio.
- Navegación simple.
- Formularios consistentes.
- Mensajes claros.
- Diseño responsive.

Toda la aplicación deberá mantener la misma identidad visual.

---

# 14. Git

Cada nueva funcionalidad deberá desarrollarse de forma independiente.

Los commits deberán ser pequeños y descriptivos.

Ejemplos

- Agrega CRUD de Empresa
- Implementa validaciones de Producto
- Corrige filtro de Categorías

---

# 15. Documentación

Todo módulo nuevo deberá contar con documentación funcional antes de comenzar su desarrollo.

La documentación oficial del proyecto se organizará de la siguiente manera.

docs

- Arquitectura General
- Roadmap
- Convenciones
- Módulos

La documentación deberá mantenerse actualizada junto con el código.

---

# 16. Filosofía del proyecto

Veltika se desarrolla siguiendo una filosofía de crecimiento progresivo.

Cada módulo deberá implementarse completamente antes de comenzar el siguiente.

Se priorizará siempre:

- Calidad.
- Simplicidad.
- Escalabilidad.
- Código limpio.
- Facilidad de mantenimiento.

El objetivo no es desarrollar el sistema más complejo, sino construir una plataforma sólida, profesional y preparada para evolucionar durante muchos años.