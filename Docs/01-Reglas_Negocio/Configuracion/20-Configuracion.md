# Módulo Configuración

---

# 1. Objetivo

El módulo Configuración permite administrar los parámetros generales de funcionamiento de Veltika para cada empresa.

Su finalidad es adaptar el comportamiento del sistema a las necesidades particulares de cada organización sin necesidad de realizar modificaciones en el código.

Este módulo constituye el centro de personalización del sistema.

---

# 2. Alcance

El módulo permite consultar y modificar las configuraciones generales de una empresa.

Las configuraciones afectan el comportamiento de distintos módulos como Ventas, Compras, Stock, Caja, Usuarios y Reportes.

---

# 3. Actores

- Super Administrador
- Administrador de Empresa

---

# 4. Permisos

## Super Administrador

✅ Consultar configuraciones.

✅ Modificar configuraciones.

## Administrador de Empresa

✅ Consultar configuraciones de su empresa.

✅ Modificar configuraciones autorizadas.

❌ No puede modificar configuraciones globales del sistema.

---

# 5. Funcionalidades

Actualmente

- Consultar configuración.
- Modificar configuración.
- Restaurar valores por defecto.

Versiones futuras

- Configuración por sucursal.
- Configuración por usuario.
- Plantillas de configuración.
- Importación y exportación.
- Configuración avanzada.

---

# 6. Campos

| Campo | Descripción |
|---------|-------------|
| Id | Identificador único |
| EmpresaId | Empresa propietaria |
| PermitirStockNegativo | Habilita o no el stock negativo |
| Moneda | Moneda principal |
| ZonaHoraria | Zona horaria |
| Idioma | Idioma del sistema |
| FechaActualizacion | Última modificación |

Campos futuros

- LogoEmpresa
- ColorCorporativo
- FormatoFecha
- FormatoHora
- CantidadDecimales
- NumeraciónAutomática
- ModoOscuro
- ImpuestosPredeterminados
- ConfiguraciónFiscal

---

# 7. Validaciones

- Debe existir una empresa.
- La moneda es obligatoria.
- El idioma es obligatorio.
- La zona horaria es obligatoria.
- Solo usuarios autorizados podrán modificar configuraciones.

---

# 8. Reglas de negocio

- Cada empresa posee su propia configuración.
- Las configuraciones afectan únicamente a la empresa propietaria.
- Toda modificación será registrada en Auditoría.
- Las configuraciones podrán modificarse en cualquier momento, salvo aquellas definidas como críticas.

---

# 9. Casos de uso

## Consultar configuración

Permite visualizar la configuración actual de la empresa.

Resultado esperado:

- Configuración cargada correctamente.

---

## Modificar configuración

Permite actualizar los parámetros generales del sistema.

Resultado esperado:

- Configuración actualizada.
- Auditoría registrada.

---

## Restaurar configuración

Permite volver a los valores predeterminados del sistema.

---

# 10. Casos de error

- Empresa inexistente.
- Usuario sin permisos.
- Configuración inválida.
- Error al guardar la configuración.

---

# 11. Flujo funcional

1. El usuario ingresa al módulo Configuración.
2. El sistema carga la configuración actual.
3. El usuario modifica los parámetros deseados.
4. El sistema valida la información.
5. Se guardan los cambios.
6. Se registra la auditoría.
7. La nueva configuración entra en vigencia.

---

# 12. Integraciones

Este módulo se relaciona con:

- Empresa
- Usuario
- Venta
- Compra
- Stock
- Caja
- Reportes
- Auditoría

---

# 13. Mejoras futuras

- Configuración por sucursal.
- Configuración por usuario.
- Configuración regional.
- Configuración fiscal.
- Integración con AFIP/ARCA.
- Plantillas reutilizables.
- API de configuración.

---

# 14. Roadmap

Versión 1.0

- Configuración general.
- Idioma.
- Moneda.
- Stock negativo.

Versión 2.0

- Configuración avanzada.
- Configuración por sucursal.
- Configuración fiscal.

Versión 3.0

- Plantillas.
- API.
- Configuración dinámica.

---

# 15. Decisiones de Arquitectura

## Configuración por empresa

Cada empresa administrará su propia configuración.

Los cambios realizados por una empresa no afectarán a las demás.

---

## Configuración centralizada

Todos los parámetros generales del sistema estarán concentrados en este módulo.

No existirán configuraciones distribuidas entre distintos módulos.

---

## Auditoría obligatoria

Toda modificación realizada sobre la configuración generará automáticamente un registro de Auditoría.

---

## Valores predeterminados

Al crear una nueva empresa, el sistema asignará una configuración inicial estándar.

El administrador podrá modificarla posteriormente.

---

## Escalabilidad

El diseño permitirá agregar nuevos parámetros sin modificar la estructura general del módulo.

Las futuras versiones incorporarán nuevas opciones manteniendo la compatibilidad con las configuraciones existentes.

---

## Configuraciones críticas

Determinados parámetros podrán afectar el funcionamiento del sistema.

Ejemplos:

- Permitir stock negativo.
- Numeración automática.
- Configuración fiscal.
- Moneda principal.

Las modificaciones sobre estos parámetros podrán requerir permisos especiales en futuras versiones.

---

## Preparado para SaaS

Todas las configuraciones estarán asociadas a una empresa.

Esto permitirá que distintas empresas utilicen Veltika con comportamientos personalizados sin interferir entre sí.