# Roadmap - Veltika

Versión: 1.0

Última actualización: 22/07/2026

---

# 1. Objetivo

Este documento establece la planificación general del desarrollo de Veltika.

Su propósito es definir el orden de implementación de los módulos del sistema y servir como guía durante el desarrollo del proyecto.

El roadmap podrá evolucionar con nuevas funcionalidades, manteniendo siempre la arquitectura principal del sistema.

---

# 2. Estado actual del proyecto

## Documentación

- ✅ Arquitectura General
- ✅ 22 módulos funcionales documentados

## Desarrollo

Estado actual:

🚧 Inicio del desarrollo.

Módulo en desarrollo:

- Empresa

---

# 3. Roadmap de implementación

## Sprint 1 — Base del sistema

Objetivo

Implementar la estructura principal sobre la cual funcionará el resto del sistema.

Módulos

- Empresa
- Categoría
- Producto

Estado

⬜ Pendiente

---

## Sprint 2 — Seguridad

Objetivo

Implementar el sistema de autenticación y autorización de usuarios.

Módulos

- Usuario
- Rol
- Permiso

Estado

⬜ Pendiente

---

## Sprint 3 — Gestión comercial

Objetivo

Implementar la administración de clientes y proveedores.

Módulos

- Cliente
- Proveedor

Estado

⬜ Pendiente

---

## Sprint 4 — Ventas

Objetivo

Implementar el circuito completo de ventas.

Módulos

- Venta
- DetalleVenta

Estado

⬜ Pendiente

---

## Sprint 5 — Compras

Objetivo

Implementar el circuito completo de compras.

Módulos

- Compra
- DetalleCompra

Estado

⬜ Pendiente

---

## Sprint 6 — Inventario

Objetivo

Implementar el control de stock y sus movimientos.

Módulos

- MovimientoStock
- Stock
- AjusteStock

Estado

⬜ Pendiente

---

## Sprint 7 — Caja

Objetivo

Implementar la gestión financiera diaria.

Módulos

- Caja
- MovimientoCaja

Estado

⬜ Pendiente

---

## Sprint 8 — Inteligencia del negocio

Objetivo

Implementar herramientas para el análisis de información.

Módulos

- Reportes
- Dashboard

Estado

⬜ Pendiente

---

# 4. Objetivos de la versión 1.0

Al finalizar la primera versión, Veltika permitirá:

- Administrar empresas.
- Administrar categorías.
- Administrar productos.
- Administrar usuarios.
- Administrar roles y permisos.
- Administrar clientes.
- Administrar proveedores.
- Registrar ventas.
- Registrar compras.
- Controlar stock.
- Gestionar caja.
- Consultar reportes.
- Visualizar indicadores mediante Dashboard.

---

# 5. Funcionalidades previstas para versiones futuras

Las siguientes funcionalidades no forman parte de la versión 1.0 y serán evaluadas para futuras versiones del sistema.

## Comercial

- Presupuestos
- Pedidos
- Devoluciones de venta
- Notas de crédito

## Compras

- Órdenes de compra
- Recepción de mercadería
- Devoluciones a proveedores

## Inventario

- Inventario físico
- Conteo cíclico
- Transferencias entre sucursales
- Gestión de lotes
- Gestión de números de serie

## Caja

- Arqueo de caja
- Turnos de caja
- Conciliación bancaria

## Configuración

- Multi moneda
- Multi idioma
- Configuración fiscal

## Integraciones

- Facturación electrónica
- Integración con AFIP / ARCA
- Mercado Pago
- Terminales POS
- API pública

## Inteligencia del negocio

- Dashboard personalizable
- Indicadores avanzados
- Automatizaciones
- Inteligencia Artificial

---

# 6. Criterios de desarrollo

Durante el desarrollo de Veltika se seguirán las siguientes reglas:

- Cada sprint deberá finalizar completamente antes de comenzar el siguiente.
- No se implementarán funcionalidades pertenecientes a versiones futuras.
- Cada módulo deberá contar con su documentación funcional antes de comenzar su desarrollo.
- Toda nueva funcionalidad deberá respetar la arquitectura definida en el documento "Arquitectura General".
- La calidad y estabilidad del sistema tendrán prioridad sobre la velocidad de desarrollo.

---

# 7. Visión del proyecto

Veltika tiene como objetivo convertirse en una plataforma de gestión moderna, escalable y modular.

Cada versión del sistema deberá ampliar sus capacidades sin comprometer la arquitectura existente, permitiendo una evolución ordenada y sostenible a largo plazo.