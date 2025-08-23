# Diagrama Entidad-Relación - Sistema Inmobiliaria

## Entidades y Relaciones

```
┌─────────────────┐         ┌─────────────────┐         ┌─────────────────┐
│   PROPIETARIOS  │         │    INMUEBLES    │         │    CONTRATOS    │
├─────────────────┤         ├─────────────────┤         ├─────────────────┤
│ Id (PK)         │◄────────┤ Id (PK)         │◄────────┤ Id (PK)         │
│ DNI (UNIQUE)    │   1:N   │ Direccion       │   1:N   │ FechaInicio     │
│ Nombre          │         │ Ambientes       │         │ FechaFin        │
│ Apellido        │         │ Superficie      │         │ Precio          │
│ Telefono        │         │ Latitud         │         │ InquilinoId (FK)│
│ Email (UNIQUE)  │         │ Longitud        │         │ InmuebleId (FK) │
│ Direccion       │         │ PropietarioId   │         │ Estado          │
│ FechaCreacion   │         │ Tipo            │         │ FechaCreacion   │
│ Estado          │         │ Precio          │         └─────────────────┘
└─────────────────┘         │ Estado          │                   │
                            │ Uso             │                   │
                            │ FechaCreacion   │                   │
                            └─────────────────┘                   │
                                                                  │
┌─────────────────┐                                               │
│   INQUILINOS    │                                               │
├─────────────────┤                                               │
│ Id (PK)         │◄──────────────────────────────────────────────┘
│ DNI (UNIQUE)    │                                        1:N
│ Nombre          │
│ Apellido        │
│ Telefono        │
│ Email (UNIQUE)  │
│ Direccion       │
│ FechaCreacion   │
│ Estado          │
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
```

## Descripción de Relaciones

### 1. PROPIETARIOS → INMUEBLES (1:N)
- **Relación**: Un propietario puede tener múltiples inmuebles
- **Clave Foránea**: `PropietarioId` en tabla `INMUEBLES`
- **Restricción**: `ON DELETE RESTRICT` - No se puede eliminar un propietario con inmuebles asociados

### 2. INMUEBLES → CONTRATOS (1:N)
- **Relación**: Un inmueble puede tener múltiples contratos (histórico)
- **Clave Foránea**: `InmuebleId` en tabla `CONTRATOS`
- **Restricción**: `ON DELETE RESTRICT` - No se puede eliminar un inmueble con contratos asociados

### 3. INQUILINOS → CONTRATOS (1:N)
- **Relación**: Un inquilino puede tener múltiples contratos
- **Clave Foránea**: `InquilinoId` en tabla `CONTRATOS`
- **Restricción**: `ON DELETE RESTRICT` - No se puede eliminar un inquilino con contratos activos

### 4. CONTRATOS → PAGOS (1:N)
- **Relación**: Un contrato tiene múltiples pagos mensuales
- **Clave Foránea**: `ContratoId` en tabla `PAGOS`
- **Restricción**: `ON DELETE RESTRICT` - No se puede eliminar un contrato con pagos asociados

## Índices y Restricciones

### Claves Primarias
- Todas las tablas tienen `Id` como clave primaria auto-incremental

### Índices Únicos
- `PROPIETARIOS.DNI` - UNIQUE
- `PROPIETARIOS.Email` - UNIQUE
- `INQUILINOS.DNI` - UNIQUE
- `INQUILINOS.Email` - UNIQUE

### Tipos de Datos Especiales
- **ENUM Tipo**: Casa, Departamento, Local, Oficina, Terreno
- **ENUM Uso**: Residencial, Comercial, Industrial
- **ENUM EstadoContrato**: Activo, Finalizado, Cancelado
- **ENUM EstadoPago**: Pendiente, Pagado, Vencido

### Campos de Auditoría
- `FechaCreacion` - Timestamp de creación del registro
- `Estado` - Para eliminación lógica (1=Activo, 0=Inactivo)

## Reglas de Negocio

1. **Unicidad de DNI y Email** por entidad
2. **Eliminación lógica** para mantener integridad histórica
3. **Restricciones de eliminación** para preservar relaciones
4. **Validaciones de fechas** en contratos (FechaFin > FechaInicio)
5. **Estados controlados** mediante ENUMs
6. **Precios y superficies** como decimales para precisión

## Cardinalidades

- **Propietario:Inmueble** = 1:N (Un propietario, muchos inmuebles)
- **Inmueble:Contrato** = 1:N (Un inmueble, muchos contratos históricos)
- **Inquilino:Contrato** = 1:N (Un inquilino, muchos contratos)
- **Contrato:Pago** = 1:N (Un contrato, muchos pagos mensuales)
