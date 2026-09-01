# Roadmap - Veltika

Versión: 2.0

Última actualización: 01/09/2026

---

# 1. Objetivo

Este documento resume la evolución general de Veltika y el estado actual de sus principales etapas de desarrollo.

El roadmap no reemplaza los issues de GitHub. Los issues contienen el alcance detallado, reglas de negocio, criterios de aceptación y tareas concretas de cada funcionalidad.

Para mejoras posteriores al MVP, la fuente principal de planificación es el issue **#27 - Mejoras y evolución post-MVP**.

---

# 2. Estado actual del proyecto

Veltika ya superó la etapa de CRUDs administrativos básicos y cuenta con una base funcional de gestión comercial considerablemente más amplia que la contemplada en la primera versión de este documento.

Actualmente el sistema incluye, entre otras capacidades:

- Arquitectura SaaS multiempresa.
- ASP.NET Core MVC con Entity Framework Core e Identity.
- Seguridad por empresa y roles.
- Baja lógica y reactivación en módulos administrativos.
- Gestión de empresas, usuarios, categorías y productos.
- Gestión de clientes y proveedores.
- Ventas y punto de venta (POS).
- Múltiples medios de pago por venta.
- Compras y pagos.
- Cobros de ventas y seguimiento de saldos.
- Movimientos y control de stock.
- Devoluciones de compras.
- Reintegros/devoluciones de ventas.
- Cajas, medios de pago y movimientos de caja.
- Transferencias entre cajas.
- Turnos de caja.
- Apertura, cierre y arqueo con efectivo esperado, contado y diferencia.
- Gastos y categorías de gasto.
- Dashboard operativo/comercial.
- Reportes de ventas y stock.
- Valorización de inventario.
- Exportaciones a Excel.
- Importación masiva de productos y stock.
- Flujo POS optimizado para búsqueda, teclado y escaneo de códigos de barra.

---

# 3. Evolución completada

## Etapa 1 — Base SaaS y módulos administrativos

Objetivo:

Construir la estructura multiempresa, seguridad y administración básica sobre la cual funciona Veltika.

Principales capacidades:

- Empresa.
- Categoría.
- Producto.
- Usuario.
- Roles.
- Seguridad multiempresa.
- Baja lógica y reactivación.

Estado:

✅ Completada

---

## Etapa 2 — Gestión comercial

Objetivo:

Incorporar las entidades necesarias para operar comercialmente con terceros.

Principales capacidades:

- Clientes.
- Proveedores.
- Integración de clientes con ventas.
- Integración de proveedores con compras.

Estado:

✅ Completada

---

## Etapa 3 — Ventas y POS

Objetivo:

Construir el circuito de venta y una interfaz rápida para operación diaria.

Principales capacidades:

- Venta y detalle de venta.
- Control de stock durante la venta.
- Selección de cliente.
- Cobros.
- Saldos pendientes.
- Múltiples medios de pago.
- Selección de caja según medio de pago.
- Cálculo de vuelto para efectivo.
- Búsqueda rápida de productos.
- Navegación por teclado.
- Flujo compatible con código de barras.
- Reintegros/devoluciones de venta.

Estado:

✅ Base funcional completada

El POS continuará evolucionando post-MVP según el uso real de los comercios.

---

## Etapa 4 — Compras, proveedores e inventario

Objetivo:

Completar el circuito de abastecimiento y mantener trazabilidad de stock.

Principales capacidades:

- Compra y detalle de compra.
- Pagos de compras.
- Saldos con proveedores.
- Ingreso de stock por compras.
- Movimientos de stock.
- Ajustes de stock.
- Devoluciones de compras.
- Punto de reposición.
- Importación masiva de productos y stock.

Estado:

✅ Base funcional completada

---

## Etapa 5 — Caja y operación financiera

Objetivo:

Registrar y controlar los movimientos financieros operativos de cada empresa.

Principales capacidades:

- Cajas.
- Medios de pago.
- Asociación entre cajas y medios de pago.
- Movimientos de caja.
- Transferencias entre cajas.
- Gastos.
- Turnos de caja.
- Apertura y cierre de turno.
- Cierre forzado cuando corresponda.
- Fondo fijo.
- Efectivo esperado.
- Efectivo contado.
- Diferencia de caja.
- Importe rendido.

Estado:

✅ Base funcional completada

---

## Etapa 6 — Reportes y dashboard

Objetivo:

Transformar los datos operativos en información útil para administrar el comercio.

Principales capacidades:

- Dashboard.
- Ventas del día y del mes.
- Productos con stock bajo.
- Productos más vendidos.
- Clientes frecuentes.
- Evolución reciente de ventas.
- Reporte de ventas.
- Reporte de stock.
- Valorización del inventario al costo y precio de venta.
- Exportaciones a Excel.

Estado:

✅ Base funcional completada

La evolución post-MVP buscará pasar de indicadores descriptivos a análisis comparativos y recomendaciones accionables.

---

# 4. Camino hacia el MVP público

La prioridad inmediata no es agregar indefinidamente nuevos módulos de negocio, sino estabilizar, validar y preparar Veltika para usuarios reales.

Los objetivos de esta etapa incluyen:

- Completar y unificar la experiencia visual de la aplicación.
- Finalizar la web pública de Veltika.
- Mejorar onboarding y configuración inicial.
- Revisar seguridad y aislamiento multiempresa.
- Mejorar validaciones y manejo de errores.
- Completar pruebas de los flujos críticos.
- Revisar rendimiento y consultas principales.
- Preparar despliegue productivo.
- Configurar dominio, HTTPS, base de datos y almacenamiento.
- Definir backups y recuperación.
- Incorporar observabilidad y logging adecuados para producción.
- Realizar pruebas piloto con comercios reales.
- Recopilar feedback antes de priorizar nuevas funcionalidades grandes.

Estado:

🚧 En evolución

---

# 5. Evolución post-MVP

Las funcionalidades futuras se mantienen en detalle en el issue **#27 - Mejoras y evolución post-MVP**.

Las principales líneas de evolución son:

## Productos e inventario

- Historial de costos y precios.
- Actualización masiva de precios.
- Listas de precios.
- Reposición inteligente.
- Inventario físico y conteo cíclico.
- Unidades de medida.
- Variantes.
- Combos y kits.
- Lotes y vencimientos cuando el rubro lo requiera.
- Números de serie cuando el rubro lo requiera.
- Catálogo maestro Veltika por código de barras/EAN.

## Ventas

- Productos favoritos.
- Ventas suspendidas.
- Descuentos y promociones.
- Ticket digital e impresión.
- Presupuestos/cotizaciones.
- Pedidos de clientes.

## Compras

- Historial y comparación de costos por proveedor.
- Órdenes de compra.
- Recepción total o parcial de mercadería.
- Sugerencias automáticas de compra.
- Costos adicionales de compra.

## Clientes y proveedores

- Historial comercial consolidado.
- Cuenta corriente avanzada.
- Vencimientos y alertas.
- Límites de crédito.
- Segmentación.
- Fidelización.

## Sucursales y depósitos

- Sucursales por empresa.
- Multi-depósito.
- Stock por ubicación.
- Transferencias de stock.
- Reportes consolidados y por sucursal.

## Caja y finanzas

- Análisis histórico de diferencias de caja.
- Indicadores por cajero.
- Reportes por medio de pago.
- Conciliación de movimientos.
- Conciliación bancaria.
- Proyección simple de flujo de caja.

## Inteligencia de negocio

- Días estimados de stock.
- Cantidades sugeridas de reposición.
- Detección de productos sin rotación.
- Rentabilidad por producto, categoría, cliente y período.
- Ranking por ganancia aportada.
- Comparativas automáticas entre períodos.
- Tendencias de ventas.
- Metas comerciales.
- Dashboard personalizable.
- Recomendaciones accionables.

## Automatizaciones

- Centro de notificaciones avanzado.
- Alertas configurables.
- Resúmenes periódicos.
- Automatizaciones basadas en eventos.

## Integraciones

- Mercado Pago.
- Facturación electrónica ARCA.
- Tickets y comprobantes digitales.
- API pública.
- Webhooks.
- PWA / experiencia móvil.

---

# 6. Evolución de Veltika como producto SaaS

Además de evolucionar el sistema de gestión, Veltika deberá desarrollar la infraestructura necesaria para operar como producto comercial SaaS.

Objetivos futuros:

- Definir planes comerciales.
- Suscripciones por empresa.
- Período de prueba.
- Límites y capacidades por plan cuando sean necesarios.
- Feature flags para módulos opcionales.
- Renovación, suspensión y reactivación de suscripciones.
- Integración con cobro recurrente.
- Panel SuperAdmin orientado a gestión comercial del SaaS.
- Métricas de adopción y uso.
- Onboarding guiado.
- Medición de activación, retención y uso por módulo.

---

# 7. Web pública y adquisición

Veltika contará con una web pública independiente de la experiencia privada de la aplicación.

Objetivos:

- Home pública.
- Páginas de funcionalidades.
- Arquitectura SEO.
- URLs amigables.
- Sitemap y robots.txt.
- Metadatos y contenido indexable.
- Search Console.
- Analítica respetando privacidad y consentimiento aplicable.
- Recursos/blog.
- Estrategia de contenidos.
- Conversión desde la web pública hacia prueba, registro o contacto.

---

# 8. Criterios de priorización

Toda nueva funcionalidad deberá evaluarse antes de entrar en desarrollo.

Criterios:

1. Resolver una necesidad real de los comercios objetivo.
2. Priorizar problemas frecuentes y de alto impacto.
3. Reducir tiempo operativo, errores o trabajo manual.
4. Mejorar adopción, retención o capacidad de monetización de Veltika.
5. Evitar complejidad que no haya sido validada.
6. Mantener seguridad multiempresa y trazabilidad.
7. Evitar incorporar al núcleo funcionalidades específicas de un rubro si pueden resolverse como capacidades opcionales.
8. Crear un issue específico cuando una funcionalidad pase del backlog a desarrollo.

---

# 9. Visión del proyecto

Veltika busca evolucionar desde un sistema de gestión comercial sólido hacia una plataforma SaaS capaz de ayudar a pequeños y medianos comercios a operar y tomar mejores decisiones.

La evolución se plantea en tres niveles:

1. **Registrar correctamente la operación:** ventas, compras, stock, caja, clientes y proveedores.
2. **Explicar qué está ocurriendo:** reportes, indicadores, comparativas y rentabilidad.
3. **Ayudar a decidir qué hacer:** alertas inteligentes, recomendaciones, automatizaciones y asistencia basada en datos.

El crecimiento del producto deberá ser incremental y guiado por el uso real, evitando transformar Veltika en un ERP complejo antes de que exista una necesidad comprobada.