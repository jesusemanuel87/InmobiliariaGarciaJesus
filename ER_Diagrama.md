# Diagrama Entidad-Relación - Sistema Inmobiliaria

## Entidades y Relaciones

```
                    ┌─────────────────┐
                    │     PERSONA     │ (Clase Base Abstracta)
                    ├─────────────────┤
                    │ Id (PK)         │
                    │ Nombre          │
                    │ Apellido        │
                    │ Dni (UNIQUE)    │
                    │ Telefono        │
                    │ Email (UNIQUE)  │
                    └─────────────────┘
                             △
                    ┌────────┴────────┐
                    │                 │
         ┌─────────────────┐ ┌─────────────────┐
         │   PROPIETARIOS  │ │   INQUILINOS    │
         ├─────────────────┤ ├─────────────────┤
         │ Id (PK)         │ │ Id (PK)         │
         │ Dni (UNIQUE)    │ │ Dni (UNIQUE)    │
         │ Nombre          │ │ Nombre          │
         │ Apellido        │ │ Apellido        │
         │ Telefono        │ │ Telefono        │
         │ Email (UNIQUE)  │ │ Email (UNIQUE)  │
         │ Direccion       │ │ Direccion       │
         │ FechaCreacion   │ │ FechaCreacion   │
         │ Estado          │ │ Estado          │
         └─────────────────┘ └─────────────────┘
                  │                    │
                  │ 1:N                │ 1:N
                  ▼                    │
         ┌─────────────────┐           │
         │    INMUEBLES    │           │
         ├─────────────────┤           │
         │ Id (PK)         │           │
         │ Direccion       │           │
         │ Ambientes       │           │
         │ Superficie      │           │
         │ Latitud         │           │
         │ Longitud        │           │
         │ PropietarioId   │           │
         │ Tipo            │           │
         │ Precio          │           │
         │ Estado          │           │
         │ Uso             │           │
         │ FechaCreacion   │           │
         └─────────────────┘           │
                  │                    │
                  │ 1:N                │
                  ▼                    │
         ┌─────────────────┐           │
         │    CONTRATOS    │◄──────────┘
         ├─────────────────┤
         │ Id (PK)         │
         │ FechaInicio     │
         │ FechaFin        │
         │ Precio          │
         │ InquilinoId (FK)│
         │ InmuebleId (FK) │
         │ Estado          │
         │ FechaCreacion   │
         └─────────────────┘
                  │
                  │ 1:N
                  ▼
         ┌─────────────────┐
         │     PAGOS       │
         ├─────────────────┤
         │ Id (PK)         │
         │ Numero          │
         │ FechaPago       │
         │ ContratoId (FK) │
         │ Importe         │
         │ Estado          │
         │ FechaCreacion   │
         └─────────────────┘

         ┌─────────────────┐
         │    USUARIOS     │ (Sistema de Autenticación)
         ├─────────────────┤
         │ Id (PK)         │
         │ Rol             │
         │ Nombre          │
         │ Apellido        │
         │ Email (UNIQUE)  │
         │ Clave           │
         │ Avatar          │
         │ Estado          │
         │ FechaCreacion   │
         └─────────────────┘
```

## Descripción de Relaciones

### 0. PERSONA (Clase Base Abstracta)
- **Herencia**: Propietarios e Inquilinos heredan de Persona
- **Campos Comunes**: Id, Nombre, Apellido, Dni, Telefono, Email
- **Patrón**: Implementado con Repository Pattern e inyección de dependencias

### 1. PROPIETARIOS → INMUEBLES (1:N)
- **Relación**: Un propietario puede tener múltiples inmuebles
- **Clave Foránea**: `PropietarioId` en tabla `INMUEBLES`
- **Restricción**: `ON DELETE RESTRICT` - No se puede eliminar un propietario con inmuebles asociados
- **Vista Expandible**: Los inmuebles se pueden visualizar inline con carga AJAX

### 2. INMUEBLES → CONTRATOS (1:N)
- **Relación**: Un inmueble puede tener múltiples contratos (histórico)
- **Clave Foránea**: `InmuebleId` en tabla `CONTRATOS`
- **Restricción**: `ON DELETE RESTRICT` - No se puede eliminar un inmueble con contratos asociados
- **Estados**: Activo/Inactivo con badges coloridos

### 3. INQUILINOS → CONTRATOS (1:N)
- **Relación**: Un inquilino puede tener múltiples contratos
- **Clave Foránea**: `InquilinoId` en tabla `CONTRATOS`
- **Restricción**: `ON DELETE RESTRICT` - No se puede eliminar un inquilino con contratos activos

### 4. CONTRATOS → PAGOS (1:N)
- **Relación**: Un contrato tiene múltiples pagos mensuales
- **Clave Foránea**: `ContratoId` en tabla `PAGOS`
- **Restricción**: `ON DELETE RESTRICT` - No se puede eliminar un contrato con pagos asociados
- **Estados**: Activo/Finalizado/Cancelado con servicio automático de vencimientos

### 5. USUARIOS (Sistema Independiente)
- **Propósito**: Autenticación y autorización del sistema
- **Roles**: Administrador/Empleado
- **Sin relaciones**: Tabla independiente para futuras implementaciones

## Índices y Restricciones

### Claves Primarias
- Todas las tablas tienen `Id` como clave primaria auto-incremental

### Índices Únicos
- `PROPIETARIOS.Dni` - UNIQUE (campo heredado de Persona)
- `PROPIETARIOS.Email` - UNIQUE (campo heredado de Persona)
- `INQUILINOS.Dni` - UNIQUE (campo heredado de Persona)
- `INQUILINOS.Email` - UNIQUE (campo heredado de Persona)
- `USUARIOS.Email` - UNIQUE

### Tipos de Datos Especiales
- **ENUM Tipo**: Casa, Departamento, Local, Oficina, Terreno
- **ENUM Uso**: Residencial, Comercial, Industrial
- **ENUM EstadoContrato**: Reservado, Activo, Finalizado, Cancelado
- **ENUM EstadoPago**: Pendiente, Pagado, Vencido
- **ENUM RolUsuario**: Administrador, Empleado

### Campos de Auditoría
- `FechaCreacion` - Timestamp de creación del registro
- `Estado` - Para eliminación lógica (1=Activo, 0=Inactivo)

## Arquitectura Implementada

### Patrón Repository
- **IRepository<T>**: Interfaz genérica para operaciones CRUD
- **Repositories específicos**: PropietarioRepository, InquilinoRepository, InmuebleRepository, ContratoRepository
- **Inyección de dependencias**: Registrados en Program.cs

### Servicios de Negocio
- **ContratoService**: Lógica de negocio para contratos
- **ContratoStateService**: Servicio automático para actualizar contratos vencidos

### Modelos y DTOs
- **Persona**: Clase base abstracta con herencia
- **InmuebleConContrato**: DTO para vistas con datos combinados

## Reglas de Negocio

1. **Herencia de Persona** - Propietarios e Inquilinos comparten campos base
2. **Unicidad de Dni y Email** por entidad
3. **Eliminación lógica** para mantener integridad histórica
4. **Restricciones de eliminación** para preservar relaciones
5. **Validaciones de fechas** en contratos (FechaFin > FechaInicio)
6. **Estados controlados** mediante ENUMs
7. **Actualización automática** de contratos vencidos mediante servicio
8. **Vista expandible** de inmuebles con carga AJAX

## Cardinalidades

- **Propietario:Inmueble** = 1:N (Un propietario, muchos inmuebles)
- **Inmueble:Contrato** = 1:N (Un inmueble, muchos contratos históricos)
- **Inquilino:Contrato** = 1:N (Un inquilino, muchos contratos)
- **Contrato:Pago** = 1:N (Un contrato, muchos pagos mensuales)

## Funcionalidades Implementadas

### Segunda Entrega
- ✅ **CRUD Completo**: Inmuebles y Contratos
- ✅ **Estados Dinámicos**: Badges coloridos para estados
- ✅ **Vista Expandible**: Inmuebles por propietario con AJAX
- ✅ **Arquitectura Limpia**: Repository Pattern + Servicios
- ✅ **Herencia de Modelos**: Clase base Persona
- ✅ **Servicio Automático**: Actualización de contratos vencidos
