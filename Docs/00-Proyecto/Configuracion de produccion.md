# Configuración de producción

Este documento registra cómo debe configurarse Veltika en el servidor de producción sin guardar contraseñas ni datos sensibles en GitHub.

## Archivos por ambiente

- `appsettings.json`: contiene la configuración general y valores no sensibles.
- `appsettings.Development.json`: contiene la conexión usada únicamente en desarrollo local.
- `appsettings.Production.json`: reduce el detalle de los logs en producción y no contiene secretos.

ASP.NET Core carga primero `appsettings.json` y después el archivo correspondiente al ambiente. Las variables del servidor tienen prioridad sobre ambos archivos.

## Variables necesarias en Windows/IIS

El servidor debe ejecutar la aplicación con:

- `ASPNETCORE_ENVIRONMENT=Production`
- `ConnectionStrings__SaasDbContext`
- `EmailSettings__Host`
- `EmailSettings__Port`
- `EmailSettings__UserName`
- `EmailSettings__Password`
- `EmailSettings__FromEmail`
- `EmailSettings__FromName`
- `EmailSettings__UseSsl`

Los dos guiones bajos representan los niveles de configuración de ASP.NET Core. Por ejemplo, `EmailSettings__Password` reemplaza `EmailSettings:Password`.

## Reglas para el deploy

1. No escribir valores reales de producción en ningún archivo versionado.
2. Antes de reemplazar `C:\inetpub\Veltika`, verificar dónde están configuradas actualmente la conexión y las credenciales de correo.
3. Conservar una copia de la configuración vigente del servidor antes de publicar.
4. Conservar y respaldar `wwwroot\uploads`, porque contiene archivos cargados por los usuarios y no forma parte de Git.
5. Verificar los bindings definitivos de IIS antes de restringir `AllowedHosts` al dominio de Veltika.
6. Después del deploy, comprobar inicio de sesión, conexión a la base de datos, envío de correo y acceso a las imágenes cargadas.

## Estado pendiente de verificar

La aplicación ya tuvo un despliegue funcional en AWS, pero el repositorio no conserva el perfil de publicación ni permite determinar dónde se guardaron los valores sensibles. Esta comprobación debe realizarse en Windows/IIS cuando se encienda nuevamente la instancia, antes del próximo deploy.
