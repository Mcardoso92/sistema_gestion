# Roadmap - Veltika

Versión: 2.1

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
- Registro autoservicio de empresas.
- Web pública con identidad visual integrada con la aplicación privada.
- Infraestructura productiva operativa en AWS.
- Deploy asistido y documentado con backups y recuperación.

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

## Etapa 7 — Web pública, onboarding e identidad

Objetivo:

Preparar la entrada pública a Veltika y reducir la fricción inicial de una empresa nueva.

Principales capacidades actuales:

- Home pública.
- Layout público separado de la aplicación autenticada.
- Identidad visual unificada entre experiencia pública y privada.
- Navegación principal reorganizada.
- Registro autoservicio de nuevas empresas.
- Inicialización automática de datos base al crear una empresa.
- Importación masiva de productos para acelerar la puesta en marcha.

Estado:

✅ Base funcional completada

La evolución de la web pública continuará principalmente en SEO, páginas de funcionalidades, contenidos, analítica y adquisición.

---

## Etapa 8 — Infraestructura y producción

Objetivo:

Disponer de un entorno productivo real, reproducible y recuperable para ejecutar Veltika fuera del ambiente local.

Infraestructura actual documentada:

- AWS EC2 con Windows Server.
- IIS y App Pool propios de Veltika.
- ASP.NET Core en ambiente `Production`.
- SQL Server Express con base `Veltika_DB`.
- Dominio `www.veltika.com.ar`.
- Configuración sensible mediante variables de entorno del servidor.
- Publicación Release.
- Migraciones idempotentes para producción.
- Scripts de creación e instalación de paquetes de deploy.
- Validación SHA256 del paquete.
- Backup SQL previo y posterior al deploy.
- Backups locales.
- Backups externos en Amazon S3.
- Respaldo de publicación e IIS antes de actualizar.
- Conservación de uploads productivos entre despliegues.
- Verificación HTTP posterior al deploy.
- Procedimiento de recuperación y rollback documentado.
- Smoke test post-deploy definido.

Estado:

✅ Infraestructura productiva operativa

El siguiente nivel de automatización —GitHub Actions, almacenamiento privado de artefactos y despliegue mediante AWS Systems Manager— queda como evolución futura y no es requisito para validar el MVP.

---

# 4. Estado hacia el MVP con usuarios reales

Veltika ya dispone de aplicación funcional, web pública e infraestructura productiva. La prioridad inmediata pasa de "construir la plataforma" a **validarla con usuarios reales y cerrar los riesgos operativos restantes**.

## Ya disponible

- [x] Web pública / Home.
- [x] Registro autoservicio de empresas.
- [x] Inicialización de datos base.
- [x] Infraestructura AWS EC2.
- [x] IIS y aplicación desplegada en producción.
- [x] SQL Server productivo.
- [x] Dominio productivo.
- [x] Configuración de producción separada de desarrollo.
- [x] Secretos fuera del repositorio.
- [x] Proceso de deploy documentado y asistido por scripts.
- [x] Backups SQL locales y externos en S3 integrados al proceso de deploy.
- [x] Procedimiento de recuperación documentado.
- [x] Cobertura automatizada de flujos críticos del MVP.
- [x] Optimización inicial de rendimiento, imágenes y paginación.
- [x] Importación masiva para facilitar la carga inicial.

## A validar o continuar antes/durante los pilotos

- [ ] Ejecutar smoke tests completos sobre cada versión desplegada.
- [ ] Validar HTTPS/certificado y bindings definitivos como parte de la operación productiva cuando corresponda.
- [ ] Verificar periódicamente que los backups de S3 puedan restaurarse correctamente; no limitarse a comprobar que el archivo exista.
- [ ] Revisar logs y diagnóstico con uso real y definir si hace falta observabilidad adicional.
- [ ] Probar recuperación de contraseña y correo transaccional en condiciones reales.
- [ ] Realizar pruebas piloto con comercios reales.
- [ ] Registrar problemas de UX y fricción detectados durante los pilotos.
- [ ] Medir tiempo hasta la primera operación útil de una empresa nueva.
- [ ] Recopilar feedback antes de priorizar funcionalidades grandes del post-MVP.

Estado:

🚧 Preparación para validación real

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

La Home pública ya forma parte de Veltika. La evolución de esta área se enfoca en convertir esa presencia pública en un canal de adquisición y explicación del producto.

Objetivos futuros:

- Páginas públicas específicas para funcionalidades.
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