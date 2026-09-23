# Guía operativa de staging de Veltika

Esta guía establece el recorrido `Desarrollo local → QA → Producción` y el
orden seguro para preparar `qa.veltika.com.ar` en la misma EC2 de producción.

## Recursos reservados para QA

- Sitio IIS: `Veltika-QA`
- App Pool: `VeltikaQAPool`
- Publicación: `C:\inetpub\Veltika-QA`
- Base de datos: `Veltika_QA_DB`
- Backups del deploy: `C:\VeltikaBackups\QA`
- Backups SQL: `C:\Backups\Veltika-QA`
- Host: `qa.veltika.com.ar`
- Ambiente ASP.NET Core: `Staging`

QA nunca debe utilizar `Veltika_DB`, `C:\inetpub\Veltika` ni los uploads de
producción. Los scripts detienen la ejecución si detectan esos recursos.

## 1. Preparar los archivos

En la computadora de desarrollo, confirmar que el repositorio esté limpio y
generar un único paquete candidato:

```powershell
Set-Location "C:\Users\Usuario\Desktop\Repo GitHub\sistema_gestion"
git status
powershell.exe -NoProfile -ExecutionPolicy Bypass `
    -File ".\Scripts\Deploy\Crear-PaqueteDeploy.ps1"
```

Copiar a la EC2:

- El ZIP generado, dentro de `C:\Deploy`.
- Los scripts de `Scripts\Deploy`, dentro de `C:\Scripts\Veltika`.

No copiar archivos de configuración local ni escribir credenciales dentro de
los scripts.

## 2. Diagnóstico inicial

Abrir PowerShell como administrador en la EC2:

```powershell
Set-Location "C:\Scripts\Veltika"
powershell.exe -NoProfile -ExecutionPolicy Bypass `
    -File ".\Verificar-VeltikaQA.ps1"
```

En la primera ejecución es normal encontrar elementos `PENDIENTE`. Esta
verificación no modifica IIS, SQL, DNS ni archivos.

## 3. Preparar IIS y SQL

Ejecutar en este orden:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass `
    -File ".\Preparar-IisVeltikaQA.ps1"

powershell.exe -NoProfile -ExecutionPolicy Bypass `
    -File ".\Preparar-BaseDatosVeltikaQA.ps1"

powershell.exe -NoProfile -ExecutionPolicy Bypass `
    -File ".\Configurar-AccesoSqlVeltikaQA.ps1"
```

El sitio y el App Pool permanecen detenidos hasta que la base, las variables y
la aplicación estén listas. La identidad de QA recibe únicamente lectura y
escritura sobre `Veltika_QA_DB`.

## 4. Configurar variables y correo seguro

QA debe usar una cuenta o destino de correo de prueba. Nunca utilizar una lista
de clientes reales ni una contraseña versionada.

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass `
    -File ".\Configurar-VariablesVeltikaQA.ps1" `
    -EmailHost "HOST_SMTP_QA" `
    -EmailPort 587 `
    -EmailUsuario "USUARIO_SMTP_QA" `
    -EmailRemitente "CORREO_QA" `
    -EmailNombre "Veltika QA"
```

El script solicita de forma interactiva la conexión a `Veltika_QA_DB` y la
contraseña del correo. Los valores sensibles no se imprimen.

## 5. Configurar DNS y HTTPS

Crear en Route 53 el registro `qa.veltika.com.ar` apuntando a la misma IP
pública utilizada por `www.veltika.com.ar`. Esperar a que resuelva antes de
continuar.

El certificado instalado en `LocalMachine\My` debe incluir exactamente
`qa.veltika.com.ar` o el comodín `*.veltika.com.ar`. Obtener su thumbprint sin
mostrar ni exportar su clave privada:

```powershell
Get-ChildItem Cert:\LocalMachine\My |
    Select-Object Subject, Thumbprint, NotAfter
```

Después configurar el binding exclusivo de QA:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass `
    -File ".\Configurar-HttpsVeltikaQA.ps1" `
    -ThumbprintCertificado "THUMBPRINT_DEL_CERTIFICADO"
```

## 6. Verificar el ZIP en la EC2

```powershell
Get-FileHash "C:\Deploy\VeltikaDeploy-FECHA.zip" -Algorithm SHA256
```

El hash debe coincidir exactamente con el informado al generar el paquete.
Si no coincide, eliminar esa copia y transferir nuevamente el archivo.

## 7. Realizar el primer deploy en QA

```powershell
Set-Location "C:\Scripts\Veltika"
powershell.exe -NoProfile -ExecutionPolicy Bypass `
    -File ".\Instalar-VeltikaQA.ps1" `
    -PaqueteZip "C:\Deploy\VeltikaDeploy-FECHA.zip" `
    -HashEsperado "SHA256_GENERADO"
```

El instalador aplica el script idempotente de migraciones únicamente sobre
`Veltika_QA_DB`, conserva uploads de QA y reinicia solamente `Veltika-QA`.
Producción debe continuar disponible durante todo el proceso.

## 8. Verificación obligatoria

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass `
    -File ".\Verificar-VeltikaQA.ps1" `
    -ExigirCompleto
```

Con `-ExigirCompleto`, cualquier elemento pendiente finaliza con error. No se
continúa con la validación funcional hasta obtener cero pendientes.

## 9. Smoke test de la release

Utilizar únicamente datos ficticios y comprobar:

1. `https://qa.veltika.com.ar` inicia sin advertencias de certificado.
2. Login y logout.
3. Dashboard y módulos principales.
4. Una operación completa de prueba.
5. Persistencia en `Veltika_QA_DB`.
6. Un upload almacenado bajo `C:\inetpub\Veltika-QA\wwwroot\uploads`.
7. Migraciones aplicadas correctamente.
8. Ausencia de errores críticos en los logs.
9. Producción sigue operativa y conserva sus propios datos y uploads.

Registrar como issue independiente cualquier error funcional encontrado. No se
modifica manualmente el paquete candidato para hacerlo funcionar solo en QA.

## 10. Promover a producción

Solo después de aprobar el smoke test:

1. Utilizar exactamente el mismo ZIP y SHA256 validados en QA.
2. Ejecutar el instalador productivo habitual.
3. Completar el smoke test de producción.

Si el paquete cambia después de probarlo en QA, se considera una release nueva
y debe volver a comenzar desde el paso 1.

