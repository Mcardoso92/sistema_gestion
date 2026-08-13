# ADR-006 - Seguridad Multiempresa

## Estado
Aceptado

## Contexto
Los datos nunca deben depender de valores enviados por el navegador.

## Decisión
`EmpresaId` se obtiene siempre desde el usuario autenticado.

## Consecuencias
Se evita el acceso cruzado entre empresas.
