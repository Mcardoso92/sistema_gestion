# 📚 Documentación Oficial - VELTIKA

Bienvenido a la documentación oficial de **VELTIKA**.

Esta carpeta reúne toda la documentación funcional, técnica y de arquitectura del sistema. Su objetivo es servir como fuente única de información durante el desarrollo, mantenimiento y evolución del proyecto.

La documentación evoluciona junto con el código y forma parte del proyecto.

---

# 📁 Estructura

```text
Docs/
│
├── 00-Proyecto/
│
├── 01-Reglas_Negocio/
│   ├── Core/
│   ├── Comercial/
│   ├── Inventario/
│   ├── Caja/
│   ├── Seguridad/
│   ├── Configuracion/
│   └── Reportes/
│
├── 02-Decisiones/
│
└── README.md
```

---

# 00 - Proyecto

Contiene la documentación general de VELTIKA.

Incluye:

- Arquitectura General
- Convenciones de Desarrollo
- Roadmap
- Documentación técnica del proyecto

Esta sección describe **cómo está construido el sistema**.

---

# 01 - Reglas de Negocio

Contiene la especificación funcional de cada módulo del sistema.

Cada documento sigue la misma estructura para mantener uniformidad y facilitar su mantenimiento.

## Estructura estándar

Todos los módulos deberán documentar, como mínimo:

- Objetivo
- Alcance
- Actores
- Permisos
- Funcionalidades
- Campos
- Validaciones
- Reglas de negocio
- Casos de uso
- Casos de error
- Flujo funcional
- Integraciones
- Mejoras futuras
- Roadmap
- Decisiones de Arquitectura

---

## Core

Módulos principales sobre los cuales se construye el sistema.

Ejemplos:

- Empresa
- Sucursal
- Categoría
- Producto

---

## Comercial

Módulos relacionados con la operación comercial.

Ejemplos:

- Cliente
- Proveedor
- Venta
- Detalle Venta
- Compra
- Detalle Compra

---

## Inventario

Módulos relacionados con el control de stock.

Ejemplos:

- Stock
- Movimiento de Stock
- Ajustes

---

## Caja

Documentación relacionada con la administración de caja y movimientos financieros.

---

## Seguridad

Documentación relacionada con autenticación, autorización y control de acceso.

Ejemplos:

- Usuario
- Rol
- Permiso
- Auditoría

---

## Configuración

Documentación de todas las configuraciones disponibles para cada empresa.

Aquí se documentan las opciones que modifican el comportamiento del sistema sin alterar las reglas generales del negocio.

Ejemplos:

- Política de stock
- Configuración de ventas
- Configuración de compras
- Parámetros generales
- Configuración fiscal

---

## Reportes

Documentación correspondiente al módulo Dashboard y Reportes.

Incluye indicadores, consultas y herramientas de análisis de información.

---

# 02 - Decisiones

Esta carpeta contiene los **Architecture Decision Records (ADR)** del proyecto.

Cada documento registra una decisión importante tomada durante el desarrollo.

Ejemplos:

- ADR-001 - Soft Delete
- ADR-002 - Arquitectura Multiempresa
- ADR-003 - Precio Histórico
- ADR-004 - Política de Stock
- ADR-005 - Venta Inmutable

El objetivo es documentar las decisiones de arquitectura y negocio para facilitar el mantenimiento y la evolución del sistema.

---

# 📋 Filosofía de Desarrollo

VELTIKA se desarrolla siguiendo los siguientes principios:

- Arquitectura limpia.
- Código mantenible.
- Seguridad primero.
- Sin deuda técnica innecesaria.
- Reglas de negocio antes que implementación.
- Consistencia entre módulos.
- Escalabilidad desde el diseño.
- Documentación viva junto al código.

---

# ✅ Criterio de Finalización de un Issue

Un Issue se considera finalizado únicamente cuando se cumplen todas las siguientes etapas:

- Desarrollo completo de la funcionalidad.
- Pruebas funcionales realizadas.
- Validación de reglas de negocio.
- Actualización de la documentación correspondiente.
- Registro de decisiones de arquitectura cuando aplique.

---

# 🎯 Objetivo

La documentación forma parte de VELTIKA y deberá mantenerse actualizada en cada Sprint.

Toda modificación funcional o arquitectónica deberá reflejarse en estos documentos para conservar una única fuente de información sobre el funcionamiento del sistema.