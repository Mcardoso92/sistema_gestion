# Pruebas automatizadas de Veltika

Este proyecto verifica reglas críticas de Veltika sin conectarse a `Saas_DB` ni modificar información real.

## Ejecutar las pruebas

Desde la carpeta raíz del repositorio:

```bash
dotnet test
```

## Cobertura actual

- Saldos, cobros y reintegros de ventas.
- Saldos, pagos, devoluciones y reintegros de compras.
- Saldos de cajas con y sin turnos.
- Validación y optimización de imágenes.
- Validaciones de productos y operaciones financieras.
- Aislamiento multiempresa en el listado y detalle de productos.

## Funcionamiento

Cada prueba crea sus propios datos temporales en memoria. Las pruebas de imágenes utilizan una carpeta temporal y la eliminan al finalizar.

## Casos que continúan siendo manuales

- Recorridos visuales completos desde el navegador.
- Manipulación de requests POST desde las herramientas del navegador.
- Operaciones simultáneas y bloqueos reales de SQL Server.
- Integración con IIS, SMTP y servicios de producción.

Cada error detectado debe convertirse en una nueva prueba automática cuando sea posible, para evitar que vuelva a aparecer.
