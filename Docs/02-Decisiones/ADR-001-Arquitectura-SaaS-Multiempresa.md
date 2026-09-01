# ADR-001 - Arquitectura SaaS Multiempresa

Última revisión: 01/09/2026

## Estado

Aceptado

## Contexto

Veltika es un sistema SaaS en el que múltiples Empresas utilizan la misma aplicación y la misma infraestructura de persistencia.

Se necesita mantener aislamiento lógico de datos sin crear una aplicación o una base de datos independiente para cada cliente durante la etapa actual del producto.

## Decisión

Veltika utilizará una arquitectura multiempresa basada en:

```text
una aplicación compartida
+
una base de datos compartida
+
aislamiento lógico mediante EmpresaId
```

Las entidades de negocio que pertenezcan a un comercio deben quedar asociadas a una Empresa.

Las consultas y operaciones deben validar el alcance de Empresa en backend. Un identificador recibido desde una View, formulario, URL o request no se considera suficiente para autorizar acceso al recurso.

`AdminEmpresa` opera dentro de su propia Empresa.

`SuperAdmin` puede realizar operaciones globales únicamente en los flujos donde esa capacidad esté expresamente permitida.

## Motivos

- Menor costo operativo durante la etapa actual de Veltika.
- Administración y despliegue simplificados.
- Una única evolución de esquema y aplicación.
- Escalabilidad suficiente para el MVP y primeras etapas comerciales.
- Permite mantener aislamiento lógico sin multiplicar infraestructura por cliente.

## Consecuencias

### Positivas

- Infraestructura más simple.
- Menor costo por Empresa.
- Mantenimiento centralizado.
- Migraciones centralizadas.
- Evolución funcional uniforme para todos los clientes.

### Riesgos

Un error de filtrado o autorización podría exponer información entre Empresas.

Por ese motivo, la seguridad multiempresa es una regla transversal y debe verificarse en Controllers, Services y consultas según corresponda.

## Relación con seguridad

Este ADR define la estrategia de partición de datos.

Las reglas específicas de autorización y protección contra acceso cruzado se complementan con `ADR-006-Seguridad-Multiempresa.md`.

## Nota de consolidación

Este ADR consolida la decisión que también había sido registrada posteriormente en `ADR-014-MultiEmpresa.md`.

`ADR-014` queda como registro histórico/supersedido para evitar mantener dos decisiones vigentes sobre el mismo punto arquitectónico.