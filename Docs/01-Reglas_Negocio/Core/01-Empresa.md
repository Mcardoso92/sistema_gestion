# Módulo Empresa

---

# 1. Objetivo

El módulo Empresa representa a cada cliente que utiliza Veltika.

Todo el sistema gira alrededor de una empresa. Cada empresa posee sus propios usuarios, productos, categorías, ventas y demás información, completamente aislada del resto de las empresas.

Este módulo constituye el núcleo de la arquitectura SaaS del sistema.

---

# 2. Alcance

El módulo permite administrar la información principal de una empresa y establecer la relación con todos los módulos del sistema.

Desde aquí comienza la segmentación de datos que garantiza que cada empresa acceda únicamente a su propia información.

---

# 3. Actores

- Super Administrador
- Administrador de Empresa

---

# 4. Permisos

## Super Administrador

✅ Crear empresa

✅ Editar empresa

✅ Desactivar empresa

✅ Reactivar empresa

✅ Visualizar todas las empresas

✅ Administrar suscripciones (futuro)

## Administrador de Empresa

✅ Visualizar los datos de su empresa

✅ Modificar datos básicos autorizados

❌ Crear nuevas empresas

❌ Eliminar empresas

---

# 5. Funcionalidades

Actualmente

- Registrar empresa
- Editar empresa
- Activar empresa
- Desactivar empresa
- Consultar información

Versiones futuras

- Gestión de sucursales
- Gestión de suscripciones
- Cambio de plan
- Gestión de logo
- Configuración regional
- Configuración fiscal
- Preferencias del sistema

---

# 6. Campos

| Campo | Descripción |
|---------|-------------|
| Id | Identificador único |
| Nombre | Nombre comercial |
| Estado | Empresa activa o inactiva |
| FechaAlta | Fecha de registro |

Campos futuros

- CUIT
- Razón Social
- Dirección
- Localidad
- Provincia
- País
- Código Postal
- Teléfono
- Email
- Sitio Web
- Logo
- Moneda
- Zona Horaria
- Idioma
- Plan contratado

---

# 7. Validaciones

- El nombre es obligatorio.
- No puede existir otra empresa con el mismo nombre.
- La fecha de alta se genera automáticamente.
- El estado inicial será Activo.

---

# 8. Reglas de negocio

- Una empresa puede tener múltiples usuarios.
- Una empresa puede tener múltiples categorías.
- Una empresa puede tener múltiples productos.
- Una empresa puede tener múltiples ventas.
- Todos los datos pertenecen exclusivamente a una empresa.
- Ningún usuario puede acceder a la información de otra empresa.

---

# 9. Casos de uso

## Crear empresa

El Super Administrador registra una nueva empresa en el sistema.

Resultado esperado:

- Empresa creada correctamente.
- Se habilita el alta de usuarios administradores.

---

## Editar empresa

Permite modificar los datos generales de la empresa.

---

## Desactivar empresa

La empresa deja de poder operar dentro del sistema.

No se elimina información.

---

# 10. Casos de error

- Nombre vacío.
- Empresa duplicada.
- Intento de eliminar empresa con información asociada.
- Usuario sin permisos.

---

# 11. Flujo funcional

1. El Super Administrador ingresa al módulo.
2. Selecciona "Nueva Empresa".
3. Completa los datos.
4. El sistema valida la información.
5. Se registra la empresa.
6. Se crea el espacio de trabajo para dicha empresa.
7. Se habilita la creación del usuario administrador.

---

# 12. Integraciones

Este módulo se relaciona con:

- Usuarios
- Sucursales
- Productos
- Categorías
- Ventas
- Clientes
- Compras
- Caja
- Reportes

---

# 13. Mejoras futuras

- Multi sucursal.
- Multi moneda.
- Multi idioma.
- Facturación electrónica.
- Integración con AFIP/ARCA.
- Gestión de planes.
- Facturación automática.
- Portal de administración para clientes.
- Personalización visual.
- API pública.

---

# 14. Roadmap

Versión 1.0

- Alta
- Edición
- Activación
- Desactivación

Versión 2.0

- Suscripciones
- Facturación
- Multi sucursal

Versión 3.0

- Personalización completa
- API
- Marketplace de módulos