# ADR-005 - Cliente opcional en Venta

## Estado
Aceptado

## Contexto

Veltika debe servir tanto para operaciones identificadas con un Cliente como para ventas rápidas a consumidores ocasionales.

Obligar a registrar un Cliente en cada Venta introduciría fricción operativa y fomentaría la creación de registros ficticios.

## Decisión

`Venta.ClienteId` será opcional (`int?`).

Una Venta puede existir sin Cliente asociado y continuará perteneciendo obligatoriamente a una Empresa y a un Usuario.

Cuando se seleccione un Cliente, el servidor debe validar que corresponda a la Empresa autorizada para la operación.

## Consecuencias

- No es necesario crear un Cliente ficticio llamado "Consumidor Final".
- El POS permite ventas rápidas sin alta previa de Cliente.
- Los reportes de clientes deben contemplar ventas sin Cliente asociado.
- Las funcionalidades futuras dependientes de identidad del Cliente sólo podrán aplicarse cuando exista asociación real.
