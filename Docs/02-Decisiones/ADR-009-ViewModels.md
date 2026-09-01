# ADR-009 - Uso de ViewModels en la capa MVC

## Estado
Aceptado

## Contexto

Exponer directamente entidades persistentes en formularios complejos aumenta acoplamiento entre UI y persistencia y puede facilitar overposting o permitir que el cliente envíe propiedades que no debería controlar.

## Decisión

Las operaciones MVC que reciben o presentan datos específicos utilizarán ViewModels cuando sea necesario para representar el contrato real de la pantalla o de la operación.

Los ViewModels deben contener únicamente los datos que la interfaz necesita enviar o recibir. Las propiedades sensibles, derivadas o controladas por el servidor no deben confiarse al modelo enviado por el cliente.

No se exige crear un ViewModel artificial para cada lectura trivial si no aporta separación, seguridad o claridad.

## Consecuencias

- Menor riesgo de overposting.
- Contratos de entrada más explícitos.
- Las entidades pueden evolucionar sin obligar a exponer todos sus campos en las Views.
- Puede existir código de mapeo adicional, aceptado a cambio de claridad y seguridad.

## Regla relacionada

Aunque un dato exista dentro del ViewModel, las reglas críticas siguen validándose server-side según ADR-007.
