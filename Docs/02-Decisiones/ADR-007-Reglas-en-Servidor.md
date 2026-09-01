# ADR-007 - Reglas críticas en servidor

## Estado
Aceptado

## Contexto

La interfaz necesita realizar cálculos y validaciones para ofrecer una buena experiencia de usuario, pero cualquier dato enviado por el navegador puede ser manipulado.

## Decisión

Las reglas de negocio críticas serán validadas y, cuando corresponda, recalculadas en el servidor antes de persistir una operación.

Esto incluye especialmente:

- precios y valores económicos;
- totales y subtotales;
- stock disponible;
- cantidades máximas permitidas;
- saldos de Caja;
- saldos pendientes de cobro, pago o reintegro;
- Empresa y pertenencia de recursos;
- estado de entidades relacionadas;
- Turnos de Caja requeridos;
- asociaciones válidas entre Caja y MedioPago.

JavaScript puede anticipar estas reglas para mejorar UX, pero nunca será la fuente de verdad.

## Consecuencias

- Se evita confiar en valores manipulables desde el cliente.
- Puede existir cierta duplicación entre validaciones de UX y validaciones server-side.
- Los Controllers/Services deben volver a consultar el estado actual cuando una operación depende de datos que pueden cambiar concurrentemente.
- Los errores de negocio deben resolverse antes de confirmar la transacción.
