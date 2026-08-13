# ADR-009 - Uso de ViewModels

## Estado
Aceptado

## Decisión
Los Controllers nunca recibirán entidades directamente.
Cada operación utilizará ViewModels específicos.

## Consecuencias
Mayor seguridad y menor riesgo de overposting.
