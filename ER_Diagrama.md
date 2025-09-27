# Diagrama Entidad-Relación - Sistema Inmobiliaria García Jesús

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
                    ┌────────┼────────┐
                    │        │        │
         ┌─────────────────┐ │ ┌─────────────────┐
         │   PROPIETARIOS  │ │ │   INQUILINOS    │
         ├─────────────────┤ │ ├─────────────────┤
         │ Id (PK)         │ │ │ Id (PK)         │
         │ Dni (UNIQUE)    │ │ │ Dni (UNIQUE)    │
         │ Nombre          │ │ │ Nombre          │
         │ Apellido        │ │ │ Apellido        │
         │ Telefono        │ │ │ Telefono        │
         │ Email (UNIQUE)  │ │ │ Email (UNIQUE)  │
         │ Direccion       │ │ │ Direccion       │
         │ FechaCreacion   │ │ │ FechaCreacion   │
         │ Estado          │ │ │ Estado          │
         └─────────────────┘ │ └─────────────────┘
                  │          │          │
                  │ 1:N      │          │ 1:N
                  ▼          │          │
         ┌─────────────────┐ │          │
         │    INMUEBLES    │ │          │
         ├─────────────────┤ │          │
         │ Id (PK)         │ │          │
         │ Direccion       │ │          │
         │ Ambientes       │ │          │
         │ Superficie      │ │          │
         │ Latitud         │ │          │
         │ Longitud        │ │          │
         │ Localidad       │ │          │
         │ Provincia       │ │          │
         │ PropietarioId   │ │          │
         │ TipoId (FK)     │ │          │
         │ Precio          │ │          │
         │ Estado          │ │          │
         │ Uso             │ │          │
         │ Disponible      │ │          │
         │ FechaCreacion   │ │          │
         └─────────────────┘ │          │
                  │          │          │
                  │ 1:N      │          │
                  ▼          │          │
         ┌─────────────────┐ │          │
         │    CONTRATOS    │◄┼──────────┘
         ├─────────────────┤ │
         │ Id (PK)         │ │
         │ FechaInicio     │ │
         │ FechaFin        │ │
         │ Precio          │ │
         │ InquilinoId (FK)│ │
         │ InmuebleId (FK) │ │
         │ Estado          │ │
         │ FechaCreacion   │ │
         │ CreadoPorId (FK)│ │
         │ TerminadoPorId  │ │
         │ FechaTerminacion│ │
         └─────────────────┘ │
                  │          │
                  │ 1:N      │
                  ▼          │
         ┌─────────────────┐ │
         │     PAGOS       │ │
         ├─────────────────┤ │
         │ Id (PK)         │ │
         │ Numero          │ │
         │ FechaPago       │ │
         │ FechaVencimiento│ │
         │ ContratoId (FK) │ │
         │ Importe         │ │
         │ Intereses       │ │
         │ Multas          │ │
         │ Estado          │ │
         │ MetodoPago      │ │
         │ Observaciones   │ │
         │ FechaCreacion   │ │
         │ CreadoPorId (FK)│ │
         │ AnuladoPorId    │ │
         │ FechaAnulacion  │ │
         └─────────────────┘ │
                             │
         ┌─────────────────┐ │
         │   EMPLEADOS     │◄┘
         ├─────────────────┤
         │ Id (PK)         │
         │ Dni (UNIQUE)    │
         │ Nombre          │
         │ Apellido        │
         │ Telefono        │
         │ Email (UNIQUE)  │
         │ Rol             │
         │ FechaIngreso    │
         │ Salario         │
         │ Estado          │
         │ FechaCreacion   │
         │ FechaModificacion│
         └─────────────────┘
                  │
                  │ 1:1
                  ▼
         ┌─────────────────┐
         │    USUARIOS     │ (Sistema de Autenticación)
         ├─────────────────┤
         │ Id (PK)         │
         │ NombreUsuario   │
         │ Email (UNIQUE)  │
         │ ClaveHash       │
         │ FotoPerfil      │
         │ Rol             │
         │ FechaCreacion   │
         │ UltimoAcceso    │
         │ Estado          │
         │ EmpleadoId (FK) │
         │ PropietarioId   │
         │ InquilinoId     │
         └─────────────────┘

         ┌─────────────────┐
         │ TIPOS_INMUEBLE  │
         ├─────────────────┤
         │ Id (PK)         │
         │ Nombre          │
         │ Descripcion     │
         │ Estado          │
         │ FechaCreacion   │
         └─────────────────┘
                  │
                  │ 1:N
                  ▼
         ┌─────────────────┐
         │INMUEBLE_IMAGENES│
         ├─────────────────┤
         │ Id (PK)         │
         │ InmuebleId (FK) │
         │ NombreArchivo   │
         │ RutaArchivo     │
         │ EsPortada       │
         │ TamanoArchivo   │
         │ TipoMime        │
         │ FechaCreacion   │
         │ FechaActualizacion│
         └─────────────────┘

         ┌─────────────────┐
         │ CONFIGURACION   │
         ├─────────────────┤
         │ Id (PK)         │
         │ Clave (UNIQUE)  │
         │ Valor           │
         │ Descripcion     │
         │ FechaCreacion   │
         │ FechaModificacion│
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
- **Funcionalidades**: Modal-based CRUD, filtros avanzados, DataTables con paginación

### 2. TIPOS_INMUEBLE → INMUEBLES (1:N)
- **Relación**: Un tipo de inmueble puede tener múltiples inmuebles
- **Clave Foránea**: `TipoId` en tabla `INMUEBLES`
- **Gestión**: CRUD completo para tipos personalizados (Casa, Departamento, Local, etc.)
- **Restricción**: No se puede eliminar un tipo con inmuebles asociados

### 3. INMUEBLES → INMUEBLE_IMAGENES (1:N)
- **Relación**: Un inmueble puede tener múltiples imágenes
- **Clave Foránea**: `InmuebleId` en tabla `INMUEBLE_IMAGENES`
- **Gestión**: Sistema completo de carga, visualización y gestión de imágenes
- **Portada**: Una imagen designada como portada por inmueble
- **Organización**: Carpetas por inmueble `/uploads/inmuebles/{id}/`

### 4. INMUEBLES → CONTRATOS (1:N)
- **Relación**: Un inmueble puede tener múltiples contratos (histórico)
- **Clave Foránea**: `InmuebleId` en tabla `CONTRATOS`
- **Restricción**: `ON DELETE RESTRICT` - No se puede eliminar un inmueble con contratos asociados
- **Estados**: Activo/Inactivo/Mantenimiento/Vendido con badges coloridos
- **Ubicación**: Integración con API Georef Argentina y Google Maps

### 5. INQUILINOS → CONTRATOS (1:N)
- **Relación**: Un inquilino puede tener múltiples contratos
- **Clave Foránea**: `InquilinoId` en tabla `CONTRATOS`
- **Restricción**: `ON DELETE RESTRICT` - No se puede eliminar un inquilino con contratos activos
- **Funcionalidades**: Modal-based CRUD, filtros por estado y búsqueda

### 6. CONTRATOS → PAGOS (1:N)
- **Relación**: Un contrato tiene múltiples pagos mensuales
- **Clave Foránea**: `ContratoId` en tabla `PAGOS`
- **Restricción**: `ON DELETE RESTRICT` - No se puede eliminar un contrato con pagos asociados
- **Estados**: Reservado/Activo/Finalizado/Cancelado con servicio automático de vencimientos
- **Auditoría**: Campos de auditoría (CreadoPor, TerminadoPor, fechas)

### 7. EMPLEADOS → USUARIOS (1:1)
- **Relación**: Cada empleado tiene una cuenta de usuario asociada
- **Clave Foránea**: `EmpleadoId` en tabla `USUARIOS`
- **Autenticación**: Sistema completo con BCrypt, roles y permisos
- **Roles**: Empleado/Administrador con diferentes niveles de acceso

### 8. USUARIOS → CONTRATOS/PAGOS (Auditoría)
- **Relación**: Auditoría de quién crea/modifica contratos y pagos
- **Claves Foráneas**: `CreadoPorId`, `TerminadoPorId`, `AnuladoPorId`
- **Funcionalidad**: Seguimiento completo de acciones por usuario
- **Acceso**: Solo administradores pueden ver información de auditoría

### 9. CONFIGURACION (Sistema)
- **Propósito**: Configuraciones del sistema (intereses, duraciones de contrato, etc.)
- **Gestión**: Panel de administración para configurar parámetros del negocio
- **Clave Única**: Sistema clave-valor para configuraciones flexibles

## Índices y Restricciones

### Claves Primarias
- Todas las tablas tienen `Id` como clave primaria auto-incremental

### Índices Únicos
- `PROPIETARIOS.Dni` - UNIQUE (campo heredado de Persona)
- `PROPIETARIOS.Email` - UNIQUE (campo heredado de Persona)
- `INQUILINOS.Dni` - UNIQUE (campo heredado de Persona)
- `INQUILINOS.Email` - UNIQUE (campo heredado de Persona)
- `EMPLEADOS.Dni` - UNIQUE (campo heredado de Persona)
- `EMPLEADOS.Email` - UNIQUE (campo heredado de Persona)
- `USUARIOS.Email` - UNIQUE
- `USUARIOS.NombreUsuario` - UNIQUE
- `CONFIGURACION.Clave` - UNIQUE
- `TIPOS_INMUEBLE.Nombre` - UNIQUE

### Tipos de Datos Especiales
- **ENUM Uso**: Residencial, Comercial, Industrial
- **ENUM EstadoInmueble**: Activo, Inactivo, Mantenimiento, Vendido
- **ENUM EstadoContrato**: Reservado, Activo, Finalizado, Cancelado
- **ENUM EstadoPago**: Pendiente, Pagado, Vencido
- **ENUM MetodoPago**: Efectivo, Transferencia, Cheque, Tarjeta
- **ENUM RolUsuario**: Propietario, Inquilino, Empleado, Administrador
- **ENUM RolEmpleado**: Empleado, Administrador

### Campos de Auditoría
- `FechaCreacion` - Timestamp de creación del registro
- `FechaModificacion` - Timestamp de última modificación (donde aplique)
- `Estado` - Para eliminación lógica (1=Activo, 0=Inactivo)
- `CreadoPorId` - Usuario que creó el registro (Contratos/Pagos)
- `TerminadoPorId` - Usuario que terminó el contrato
- `AnuladoPorId` - Usuario que anuló el pago
- `FechaTerminacion` - Fecha de terminación del contrato
- `FechaAnulacion` - Fecha de anulación del pago

## Arquitectura Implementada

### Patrón Repository
- **IRepository<T>**: Interfaz genérica para operaciones CRUD
- **Repositories específicos**: PropietarioRepository, InquilinoRepository, InmuebleRepository, ContratoRepository, PagoRepository, EmpleadoRepository, UsuarioRepository, ConfiguracionRepository, InmuebleImagenRepository, TipoInmuebleRepository
- **Inyección de dependencias**: Todos registrados en Program.cs con IConfiguration pattern
- **Migración**: Todos los repositorios migrados de MySqlConnectionManager a IConfiguration

### Servicios de Negocio
- **ContratoService**: Lógica de negocio para contratos
- **PagoService**: Cálculo de intereses y gestión de pagos
- **PaymentBackgroundService**: Servicio automático para actualizar estados de pago cada hora
- **AuthenticationService**: Autenticación con BCrypt y gestión de sesiones
- **UsuarioService**: Gestión de usuarios y perfiles
- **EmpleadoService**: Gestión de empleados
- **InmuebleImagenService**: Gestión de imágenes de inmuebles
- **ConfiguracionService**: Gestión de configuraciones del sistema
- **ProfilePhotoService**: Gestión de fotos de perfil
- **DatabaseSeederService**: Inicialización automática de datos

### Modelos y DTOs
- **Persona**: Clase base abstracta con herencia
- **InmuebleConContrato**: DTO para vistas con datos combinados
- **ViewModels**: Múltiples ViewModels para formularios y vistas
- **AuditoriaViewModels**: ViewModels específicos para auditoría
- **AuthViewModels**: ViewModels para autenticación
- **DataTablesRequest**: DTO para integración con DataTables

## Reglas de Negocio

1. **Herencia de Persona** - Propietarios, Inquilinos y Empleados comparten campos base
2. **Unicidad de Dni y Email** por entidad (Propietarios, Inquilinos, Empleados)
3. **Eliminación lógica** para mantener integridad histórica
4. **Restricciones de eliminación** para preservar relaciones
5. **Validaciones de fechas** en contratos (FechaFin > FechaInicio)
6. **Estados controlados** mediante ENUMs
7. **Actualización automática** de estados de pago mediante PaymentBackgroundService
8. **Cálculo automático de intereses** por mora según configuración
9. **Sistema de auditoría** completo para contratos y pagos
10. **Control de acceso basado en roles** (Administrador, Empleado, Propietario, Inquilino)
11. **Gestión de imágenes** con portada automática y organización por carpetas
12. **Configuraciones dinámicas** del sistema mediante panel de administración
13. **Integración con APIs externas** (Georef Argentina, Google Maps)
14. **Modal-based CRUD** para mejor experiencia de usuario
15. **Filtros avanzados** con persistencia de estado

## Cardinalidades

- **Propietario:Inmueble** = 1:N (Un propietario, muchos inmuebles)
- **TipoInmueble:Inmueble** = 1:N (Un tipo, muchos inmuebles)
- **Inmueble:InmuebleImagen** = 1:N (Un inmueble, muchas imágenes)
- **Inmueble:Contrato** = 1:N (Un inmueble, muchos contratos históricos)
- **Inquilino:Contrato** = 1:N (Un inquilino, muchos contratos)
- **Contrato:Pago** = 1:N (Un contrato, muchos pagos mensuales)
- **Empleado:Usuario** = 1:1 (Un empleado, una cuenta de usuario)
- **Usuario:Contrato** = 1:N (Un usuario puede crear/terminar muchos contratos)
- **Usuario:Pago** = 1:N (Un usuario puede crear/anular muchos pagos)

## Funcionalidades Implementadas

### Sistema de Autenticación y Autorización
- ✅ **Autenticación Completa**: Login/Logout con BCrypt
- ✅ **Sistema de Roles**: Administrador, Empleado, Propietario, Inquilino
- ✅ **Control de Acceso**: Middleware y atributos de autorización
- ✅ **Gestión de Perfiles**: Fotos de perfil y actualización de datos
- ✅ **Sesiones Seguras**: Cookie-based authentication

### CRUD Modal-Based (Patrón Unificado)
- ✅ **Propietarios**: CRUD completo con modales y DataTables
- ✅ **Inquilinos**: CRUD completo con modales y DataTables
- ✅ **Empleados**: CRUD completo con creación de usuarios
- ✅ **Inmuebles**: CRUD completo con gestión de imágenes
- ✅ **Contratos**: CRUD completo con auditoría
- ✅ **Pagos**: Gestión completa con cálculo de intereses
- ✅ **Tipos de Inmueble**: CRUD completo para gestión de tipos
- ✅ **Configuración**: Panel de administración del sistema

### Sistemas Avanzados
- ✅ **Gestión de Imágenes**: Carga múltiple, portadas, organización
- ✅ **Sistema de Auditoría**: Seguimiento completo de acciones
- ✅ **Cálculo de Intereses**: Automático por mora con reglas de negocio
- ✅ **Filtros Avanzados**: En todos los módulos con persistencia
- ✅ **Integración APIs**: Georef Argentina y Google Maps
- ✅ **Página Pública**: Listado de propiedades con filtros
- ✅ **Background Services**: Actualización automática de estados

### Arquitectura y Patrones
- ✅ **Repository Pattern**: Implementación completa con IConfiguration
- ✅ **Inyección de Dependencias**: Todos los servicios registrados
- ✅ **Modal-based UI**: Experiencia de usuario moderna
- ✅ **DataTables**: Paginación, búsqueda y ordenamiento
- ✅ **Responsive Design**: Bootstrap 5 y diseño adaptable
- ✅ **Validaciones**: Cliente y servidor con feedback visual
