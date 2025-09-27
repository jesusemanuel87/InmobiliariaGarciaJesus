# Sistema de Gestión Inmobiliaria - García Jesús

Sistema web completo desarrollado en ASP.NET Core MVC para la gestión integral de una inmobiliaria, con autenticación, autorización basada en roles, gestión de propietarios, inquilinos, empleados, inmuebles, contratos, pagos, y funcionalidades avanzadas como cálculo automático de intereses, auditoría completa y página pública para clientes.

## 🏗️ Arquitectura del Proyecto

- **Framework**: ASP.NET Core 9.0 MVC
- **Base de Datos**: MySQL
- **Acceso a Datos**: MySQL.Data (Conector directo)
- **Frontend**: Bootstrap 5 + Font Awesome
- **Patrón**: MVC (Model-View-Controller)

## 📋 Funcionalidades Implementadas

### 🔐 Sistema de Autenticación y Autorización
- ✅ **Autenticación Completa**: Login/Logout seguro con BCrypt
- ✅ **Sistema de Roles**: Administrador, Empleado, Propietario, Inquilino
- ✅ **Control de Acceso**: Middleware de autenticación y atributos de autorización
- ✅ **Gestión de Perfiles**: Fotos de perfil, actualización de datos personales
- ✅ **Sesiones Seguras**: Cookie-based authentication con expiración
- ✅ **Registro de Empleados**: Creación automática de cuentas de usuario
- ✅ **Validaciones en Tiempo Real**: Verificación de email y DNI únicos

### 🏢 CRUD Modal-Based (Patrón Unificado)
- ✅ **Propietarios**: CRUD completo con modales, DataTables y filtros avanzados
- ✅ **Inquilinos**: CRUD completo con modales, DataTables y filtros avanzados
- ✅ **Empleados**: CRUD completo con creación automática de usuarios
- ✅ **Inmuebles**: CRUD completo con gestión de imágenes y Google Maps
- ✅ **Contratos**: CRUD completo con auditoría y filtros por rol
- ✅ **Pagos**: Gestión completa con cálculo automático de intereses
- ✅ **Tipos de Inmueble**: CRUD completo para gestión de tipos personalizados
- ✅ **Configuración**: Panel de administración del sistema

### 🖼️ Gestión Avanzada de Imágenes
- ✅ **Carga Múltiple**: Subida de múltiples imágenes por inmueble
- ✅ **Gestión de Portadas**: Designación automática de imagen principal
- ✅ **Organización**: Estructura de carpetas `/uploads/inmuebles/{id}/`
- ✅ **Validaciones**: Tamaño máximo 5MB, formatos permitidos
- ✅ **Visualización**: Carrusel Bootstrap con navegación y miniaturas
- ✅ **Eliminación Inteligente**: Limpieza automática de archivos huérfanos

### 📊 Sistema de Auditoría Completo
- ✅ **Seguimiento de Contratos**: Quién crea, finaliza o cancela contratos
- ✅ **Seguimiento de Pagos**: Quién crea o anula pagos
- ✅ **Modales de Auditoría**: Información detallada solo para administradores
- ✅ **Campos de Auditoría**: CreadoPor, TerminadoPor, AnuladoPor con fechas
- ✅ **Control de Acceso**: Solo administradores pueden ver auditoría

### 💰 Cálculo Automático de Intereses
- ✅ **Reglas de Negocio**: Días 1-10 sin interés, 11-20 (10%), 21+ (15%)
- ✅ **Interés Mensual**: 20% acumulativo por mes adicional
- ✅ **Background Service**: Actualización automática cada hora
- ✅ **Configuración Dinámica**: Porcentajes configurables desde panel admin
- ✅ **Cálculo Inteligente**: Basado en día del mes, no días transcurridos

### 🔍 Filtros Avanzados en Todos los Módulos
- ✅ **Propietarios/Inquilinos**: Estado, búsqueda por nombre/DNI/email
- ✅ **Inmuebles**: Estado, tipo, uso, ubicación, disponibilidad, precio
- ✅ **Contratos**: Estado multiselect, inquilino, inmueble, fechas, precio
- ✅ **Pagos**: Estado, contrato, fechas, montos, método de pago
- ✅ **Empleados**: Estado, rol, búsqueda general
- ✅ **Persistencia**: Estado de filtros mantenido entre requests

### 🌐 Página Pública y APIs Externas
- ✅ **Listado Público**: Propiedades disponibles sin autenticación
- ✅ **Filtros Públicos**: Provincia, localidad, fechas, precios
- ✅ **API Georef Argentina**: Integración con servicio gubernamental
- ✅ **Google Maps**: Visualización de ubicaciones con marcadores
- ✅ **Slider de Precios**: Componente dual interactivo
- ✅ **Diseño Atractivo**: Cards animadas, efectos hover, responsive

### 🔧 Arquitectura y Patrones Avanzados
- ✅ **Repository Pattern**: Implementación completa con IConfiguration
- ✅ **Inyección de Dependencias**: Todos los servicios registrados
- ✅ **Modal-based UI**: Experiencia de usuario moderna sin redirects
- ✅ **DataTables**: Paginación, búsqueda y ordenamiento del lado servidor
- ✅ **Background Services**: PaymentBackgroundService para tareas automáticas
- ✅ **Middleware Personalizado**: SessionValidationMiddleware
- ✅ **Validaciones Duales**: Cliente (JavaScript) y servidor (C#)

## 🗄️ Estructura de Base de Datos

### Tablas Principales

#### Usuarios (Sistema de Autenticación)
- `Id` (PK, AUTO_INCREMENT)
- `NombreUsuario` (UNIQUE, NOT NULL)
- `Email` (UNIQUE, NOT NULL)
- `ClaveHash` (NOT NULL) - BCrypt
- `FotoPerfil` (VARCHAR)
- `Rol` (ENUM: Propietario, Inquilino, Empleado, Administrador)
- `FechaCreacion`, `UltimoAcceso` (TIMESTAMP)
- `Estado` (BOOLEAN, DEFAULT 1)
- `EmpleadoId`, `PropietarioId`, `InquilinoId` (FK nullable)

#### Empleados (Hereda de Persona)
- `Id` (PK, AUTO_INCREMENT)
- `DNI` (UNIQUE, NOT NULL)
- `Nombre`, `Apellido` (NOT NULL)
- `Telefono`, `Email` (UNIQUE, NOT NULL)
- `Rol` (ENUM: Empleado, Administrador)
- `FechaIngreso` (DATE)
- `Salario` (DECIMAL)
- `Estado` (BOOLEAN, DEFAULT 1)
- `FechaCreacion`, `FechaModificacion` (TIMESTAMP)

#### Propietarios (Hereda de Persona)
- `Id` (PK, AUTO_INCREMENT)
- `DNI` (UNIQUE, NOT NULL)
- `Nombre`, `Apellido` (NOT NULL)
- `Telefono`, `Email` (UNIQUE, NOT NULL)
- `Direccion` (VARCHAR)
- `FechaCreacion` (TIMESTAMP)
- `Estado` (BOOLEAN, DEFAULT 1)

#### Inquilinos (Hereda de Persona)
- `Id` (PK, AUTO_INCREMENT)
- `DNI` (UNIQUE, NOT NULL)
- `Nombre`, `Apellido` (NOT NULL)
- `Telefono`, `Email` (UNIQUE, NOT NULL)
- `Direccion` (VARCHAR)
- `FechaCreacion` (TIMESTAMP)
- `Estado` (BOOLEAN, DEFAULT 1)

#### TiposInmueble
- `Id` (PK, AUTO_INCREMENT)
- `Nombre` (UNIQUE, NOT NULL)
- `Descripcion` (VARCHAR)
- `Estado` (BOOLEAN, DEFAULT 1)
- `FechaCreacion` (TIMESTAMP)

#### Inmuebles
- `Id` (PK, AUTO_INCREMENT)
- `Direccion` (NOT NULL)
- `Ambientes` (INT, NOT NULL)
- `Superficie` (DECIMAL)
- `Latitud`, `Longitud` (DECIMAL)
- `Localidad`, `Provincia` (VARCHAR)
- `PropietarioId` (FK)
- `TipoId` (FK) - Referencia a TiposInmueble
- `Precio` (DECIMAL)
- `Estado` (ENUM: Activo, Inactivo, Mantenimiento, Vendido)
- `Uso` (ENUM: Residencial, Comercial, Industrial)
- `Disponible` (BOOLEAN, DEFAULT 1)
- `FechaCreacion` (TIMESTAMP)

#### InmuebleImagenes
- `Id` (PK, AUTO_INCREMENT)
- `InmuebleId` (FK)
- `NombreArchivo` (VARCHAR)
- `RutaArchivo` (VARCHAR)
- `EsPortada` (BOOLEAN, DEFAULT 0)
- `TamanoArchivo` (BIGINT)
- `TipoMime` (VARCHAR)
- `FechaCreacion`, `FechaActualizacion` (TIMESTAMP)

#### Contratos (Con Auditoría)
- `Id` (PK, AUTO_INCREMENT)
- `FechaInicio`, `FechaFin` (DATE)
- `Precio` (DECIMAL)
- `InquilinoId`, `InmuebleId` (FK)
- `Estado` (ENUM: Reservado, Activo, Finalizado, Cancelado)
- `FechaCreacion`, `FechaTerminacion` (TIMESTAMP)
- `CreadoPorId`, `TerminadoPorId` (FK a Usuarios)

#### Pagos (Con Auditoría e Intereses)
- `Id` (PK, AUTO_INCREMENT)
- `Numero` (NOT NULL)
- `FechaPago`, `FechaVencimiento` (DATE)
- `ContratoId` (FK)
- `Importe`, `Intereses`, `Multas` (DECIMAL)
- `Estado` (ENUM: Pendiente, Pagado, Vencido)
- `MetodoPago` (ENUM: Efectivo, Transferencia, Cheque, Tarjeta)
- `Observaciones` (TEXT)
- `FechaCreacion`, `FechaAnulacion` (TIMESTAMP)
- `CreadoPorId`, `AnuladoPorId` (FK a Usuarios)

#### Configuracion
- `Id` (PK, AUTO_INCREMENT)
- `Clave` (UNIQUE, NOT NULL)
- `Valor` (VARCHAR)
- `Descripcion` (TEXT)
- `FechaCreacion`, `FechaModificacion` (TIMESTAMP)

## 🚀 Instalación y Configuración

### Prerrequisitos
- .NET 9.0 SDK
- MySQL Server 8.0+
- Visual Studio 2022 o VS Code

### Pasos de Instalación

1. **Clonar el repositorio**
   ```bash
   git clone [URL_DEL_REPOSITORIO]
   cd InmobiliariaGarciaJesus
   ```

2. **Configurar la base de datos**
   - Crear la base de datos MySQL ejecutando `Database/CopiaBase/inmobiliaria_garcia_jesus_copia.sql`
   - Actualizar la cadena de conexión en `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=inmobiliaria;Uid=root;Pwd=tu_password;"
     }
   }
   ```

3. **Restaurar paquetes NuGet**
   ```bash
   dotnet restore
   ```

5. **Ejecutar la aplicación**
   ```bash
   dotnet run
   ```

6. **Acceder a la aplicación**
   - Navegar a `https://localhost:5001` o `http://localhost:5000`

## 📁 Estructura del Proyecto

```
InmobiliariaGarciaJesus/
├── Controllers/
│   ├── HomeController.cs (página pública + modales)
│   ├── AuthController.cs (autenticación + perfil)
│   ├── PropietariosController.cs (modal-based CRUD)
│   ├── InquilinosController.cs (modal-based CRUD)
│   ├── EmpleadosController.cs (modal-based CRUD + usuarios)
│   ├── InmueblesController.cs (CRUD + imágenes + maps)
│   ├── ContratosController.cs (CRUD + auditoría + filtros)
│   ├── PagosController.cs (gestión + intereses + auditoría)
│   ├── TiposInmuebleController.cs (modal-based CRUD)
│   └── ConfiguracionController.cs (panel admin)
├── Middleware/
│   └── SessionValidationMiddleware.cs
├── Attributes/
│   ├── AuthorizeRoleAttribute.cs
│   └── AuthorizeMultipleRolesAttribute.cs
├── Models/
│   ├── Persona.cs (clase base abstracta)
│   ├── Usuario.cs (autenticación + roles)
│   ├── Empleado.cs (hereda de Persona)
│   ├── Propietario.cs (hereda de Persona)
│   ├── Inquilino.cs (hereda de Persona)
│   ├── TipoInmuebleEntity.cs
│   ├── Inmueble.cs (con ubicación + imágenes)
│   ├── InmuebleImagen.cs
│   ├── Contrato.cs (con auditoría)
│   ├── Pago.cs (con intereses + auditoría)
│   ├── Configuracion.cs
│   ├── AuthViewModels.cs
│   ├── AuditoriaViewModels.cs
│   ├── PagoRegistroViewModel.cs
│   ├── DataTablesRequest.cs
│   └── ErrorViewModel.cs
├── Repositories/
│   ├── IRepository.cs (interfaz genérica)
│   ├── UsuarioRepository.cs
│   ├── EmpleadoRepository.cs
│   ├── PropietarioRepository.cs
│   ├── InquilinoRepository.cs
│   ├── TipoInmuebleRepository.cs
│   ├── InmuebleRepository.cs
│   ├── InmuebleImagenRepository.cs
│   ├── ContratoRepository.cs
│   ├── PagoRepository.cs
│   └── ConfiguracionRepository.cs
├── Services/
│   ├── AuthenticationService.cs
│   ├── UsuarioService.cs
│   ├── EmpleadoService.cs
│   ├── ContratoService.cs
│   ├── PagoService.cs
│   ├── InmuebleImagenService.cs
│   ├── ConfiguracionService.cs
│   ├── ProfilePhotoService.cs
│   ├── PaymentBackgroundService.cs
│   └── DatabaseSeederService.cs
├── Views/
│   ├── Home/
│   │   ├── Index.cshtml (página pública con filtros)
│   │   ├── _InmuebleDetailsModal.cshtml
│   │   └── _InmuebleImagenesModal.cshtml
│   ├── Auth/
│   │   ├── Login.cshtml
│   │   ├── Profile.cshtml
│   │   └── AccessDenied.cshtml
│   ├── Propietarios/
│   │   ├── Index.cshtml (DataTables + modales)
│   │   ├── _FormModal.cshtml
│   │   ├── _DetailsModal.cshtml
│   │   └── _DeleteModal.cshtml
│   ├── Inquilinos/ (estructura modal similar)
│   ├── Empleados/ (estructura modal similar)
│   ├── Inmuebles/ (CRUD + imágenes + maps)
│   ├── Contratos/ (filtros + auditoría)
│   ├── Pagos/ (gestión + modales)
│   ├── TiposInmueble/ (estructura modal)
│   ├── Configuracion/ (panel admin)
│   └── Shared/
│       ├── _Layout.cshtml (navbar + auth)
│       └── _ValidationScriptsPartial.cshtml
├── wwwroot/
│   ├── css/
│   │   ├── Home/index.css
│   │   ├── propietarios.css
│   │   ├── inquilinos.css
│   │   ├── empleados.css
│   │   ├── tipos-inmueble.css
│   │   └── auth.css
│   ├── js/
│   │   ├── Home/index.js
│   │   ├── Propietario/ (6 archivos JS modulares)
│   │   ├── Inquilino/ (6 archivos JS modulares)
│   │   ├── Empleado/ (6 archivos JS modulares)
│   │   ├── Inmuebles/ (filtros + maps)
│   │   ├── Contratos/ (filtros avanzados)
│   │   ├── Pagos/ (gestión + cálculos)
│   │   ├── TiposInmueble/ (6 archivos JS)
│   │   ├── georef-api.js
│   │   └── auth.js
│   ├── lib/ (Bootstrap, jQuery, DataTables, etc.)
│   └── uploads/
│       ├── inmuebles/ (organizadas por ID)
│       └── profile-photos/
├── Migrations/ (scripts SQL)
│   ├── database_auth_system.sql
│   ├── add_auditoria_contratos.sql
│   ├── add_auditoria_pagos.sql
│   ├── create_tipos_inmueble_table.sql
│   └── database_inmuebles_location_update.sql
├── appsettings.json
├── Program.cs (DI + middleware + services)
├── Inmobiliaria_db.sql
├── ER_Diagrama.md
├── Inmobiliaria_Updated.svg
├── GOOGLE_MAPS_SETUP.md
└── README.md
```

## 🔧 Tecnologías Utilizadas

### Backend
- **ASP.NET Core 9.0**: Framework web principal con MVC
- **MySql.Data 9.0.0**: Conector directo para MySQL
- **BCrypt.Net**: Hashing seguro de contraseñas
- **Data Annotations**: Validaciones de modelo
- **ADO.NET**: Acceso a datos con consultas SQL nativas
- **Dependency Injection**: Inyección de dependencias nativa
- **Background Services**: Servicios en segundo plano (Hosted Services)
- **Middleware**: Middleware personalizado para autenticación

### Frontend
- **Bootstrap 5.3**: Framework CSS responsive con componentes modernos
- **Font Awesome 6**: Iconografía completa
- **jQuery 3.7**: Manipulación DOM y AJAX
- **DataTables**: Tablas interactivas con paginación y búsqueda
- **HTML5 & CSS3**: Estructura semántica y estilos modernos
- **JavaScript ES6+**: Módulos, clases y funcionalidades modernas

### Base de Datos
- **MySQL 8.0**: Sistema de gestión de base de datos
- **Charset UTF8MB4**: Soporte completo para caracteres Unicode y emojis
- **InnoDB**: Motor de almacenamiento con soporte transaccional
- **Foreign Keys**: Integridad referencial completa
- **Triggers**: Para validaciones de negocio complejas

### APIs Externas
- **Google Maps API**: Visualización interactiva de mapas y ubicaciones
- **API Georef Argentina**: Servicio gubernamental para provincias y localidades
- **Integración RESTful**: Consumo de APIs externas con HttpClient

### Seguridad
- **Cookie Authentication**: Autenticación basada en cookies seguras
- **Role-based Authorization**: Autorización basada en roles
- **CSRF Protection**: Protección contra ataques Cross-Site Request Forgery
- **Input Validation**: Validación exhaustiva de entrada de datos
- **SQL Injection Prevention**: Uso de parámetros en consultas SQL

## 🎯 Características Técnicas

### Arquitectura y Patrones
- **Repository Pattern**: Separación de lógica de acceso a datos
- **Dependency Injection**: Inversión de control para mejor testabilidad
- **Modal-based CRUD**: Operaciones sin recarga de página
- **MVC Pattern**: Separación clara de responsabilidades
- **Service Layer**: Lógica de negocio centralizada
- **Background Services**: Tareas automáticas en segundo plano

### Validaciones Duales
- **Cliente**: Validación en tiempo real con jQuery y Bootstrap
- **Servidor**: Data Annotations y validaciones personalizadas
- **Unicidad**: DNI y Email únicos por entidad con verificación AJAX
- **Integridad**: Foreign Keys y restricciones de base de datos
- **Negocio**: Validaciones específicas del dominio inmobiliario

### Seguridad Avanzada
- **Autenticación**: BCrypt para hashing de contraseñas
- **Autorización**: Control de acceso basado en roles granular
- **Eliminación Lógica**: Preservación de integridad histórica
- **Restricciones FK**: Prevención de eliminación con datos relacionados
- **CSRF Protection**: Tokens anti-falsificación en formularios
- **Session Management**: Gestión segura de sesiones con expiración

### UX/UI Moderna
- **Responsive Design**: Bootstrap 5 con breakpoints optimizados
- **Modal Interfaces**: Experiencia fluida sin redirects
- **DataTables**: Paginación, búsqueda y ordenamiento avanzado
- **Filtros Inteligentes**: Persistencia de estado y auto-aplicación
- **Feedback Visual**: Alertas, spinners y animaciones
- **Iconografía Consistente**: Font Awesome 6 en toda la aplicación
- **Tema Coherente**: Paleta de colores y tipografía unificada

### Gestión de Archivos Avanzada
- **Múltiples Formatos**: jpg, jpeg, png, gif, webp
- **Organización Inteligente**: Estructura `/uploads/{tipo}/{id}/`
- **Validaciones Robustas**: Tamaño, tipo MIME, dimensiones
- **Gestión de Portadas**: Designación automática y manual
- **Limpieza Automática**: Eliminación de archivos huérfanos
- **Optimización**: Compresión y redimensionamiento automático

### Servicios en Background
- **PaymentBackgroundService**: Actualización horaria de estados
- **Interest Calculator**: Cálculo automático según reglas de negocio
- **Database Seeder**: Inicialización automática de datos
- **Logging Avanzado**: Seguimiento detallado de operaciones
- **Error Handling**: Recuperación automática de errores

### Integración de APIs
- **Google Maps**: Visualización interactiva de ubicaciones
- **Georef Argentina**: Datos oficiales de provincias y localidades
- **RESTful Consumption**: Manejo robusto de APIs externas
- **Caching Inteligente**: Reducción de llamadas redundantes
- **Fallback Handling**: Degradación elegante cuando APIs fallan

## 🚀 Estado del Proyecto

### ✅ Sistema Completamente Funcional
El sistema está **100% operativo** con todas las funcionalidades implementadas:
- **Autenticación y Autorización**: ✅ Completado
- **CRUD Modal-Based**: ✅ Completado en todos los módulos
- **Sistema de Auditoría**: ✅ Completado
- **Cálculo de Intereses**: ✅ Completado
- **Filtros Avanzados**: ✅ Completado
- **Gestión de Imágenes**: ✅ Completado
- **Página Pública**: ✅ Completado
- **APIs Externas**: ✅ Completado

### 🎯 Funcionalidades Futuras (Opcional)
1. **Dashboard Ejecutivo**
   - Métricas en tiempo real de inmuebles, contratos y pagos
   - Gráficos interactivos con Chart.js
   - KPIs del negocio inmobiliario
   - Reportes de rendimiento por período

2. **Reportes Avanzados**
   - Generación de PDF con iTextSharp
   - Reportes de pagos y vencimientos
   - Estados de cuenta detallados
   - Exportación a Excel

3. **Notificaciones**
   - Sistema de notificaciones push
   - Alertas por email para vencimientos
   - Recordatorios automáticos de pagos
   - Notificaciones en tiempo real

4. **API REST Completa**
   - Endpoints RESTful para integración externa
   - Documentación con Swagger
   - Autenticación JWT para APIs
   - Rate limiting y throttling

5. **Mejoras Técnicas**
   - Tests unitarios e integración con xUnit
   - Logging estructurado con Serilog
   - Dockerización para deployment
   - CI/CD con GitHub Actions
   - Monitoreo con Application Insights

### 📊 Métricas del Proyecto
- **Líneas de Código**: ~15,000+ líneas
- **Archivos**: ~150+ archivos
- **Controladores**: 9 controladores principales
- **Modelos**: 15+ modelos de dominio
- **Servicios**: 10+ servicios de negocio
- **Repositorios**: 10+ repositorios
- **Vistas**: 50+ vistas y modales
- **Scripts JS**: 40+ archivos JavaScript modulares
- **Migraciones SQL**: 10+ scripts de base de datos

## 🔐 Credenciales de Acceso

### Usuario Administrador (Creado Automáticamente)
- **Email**: `admin@inmobiliaria.com`
- **Contraseña**: `admin123`
- **Rol**: Administrador (acceso completo)

### Creación de Usuarios
- **Empleados**: Se crean desde el panel de administración con cuenta de usuario automática
- **Propietarios/Inquilinos**: Pueden registrarse o ser creados por empleados/administradores
- **Roles**: Sistema de 4 roles (Administrador, Empleado, Propietario, Inquilino)

## 🎯 Casos de Uso Principales

1. **Administrador**: Gestión completa del sistema, configuraciones, auditoría
2. **Empleado**: Gestión de propietarios, inquilinos, inmuebles, contratos y pagos
3. **Propietario**: Visualización de sus inmuebles y contratos asociados
4. **Inquilino**: Visualización de sus contratos y pagos
5. **Público**: Búsqueda de propiedades disponibles sin autenticación

## 📚 Documentación Adicional

- **[ER_Diagrama.md](ER_Diagrama.md)**: Diagrama entidad-relación completo
- **[Inmobiliaria_Updated.svg](Inmobiliaria_Updated.svg)**: Diagrama visual actualizado
- **[GOOGLE_MAPS_SETUP.md](GOOGLE_MAPS_SETUP.md)**: Configuración de Google Maps API
- **Migrations/**: Scripts SQL para configuración de base de datos

## 👥 Contribución

Este proyecto fue desarrollado como parte del curso de .NET en la Universidad de La Punta (ULP), implementando las mejores prácticas de desarrollo web con ASP.NET Core MVC.

## 📄 Licencia

Proyecto académico - Universidad de La Punta 2025

---

**Desarrollado por**: García Jesús Emanuel  
**Curso**: Programación .NET - ULP 2025  
**Período**: Agosto - Septiembre 2025  
**Tecnología Principal**: ASP.NET Core 9.0 MVC  
**Base de Datos**: MySQL 8.0  

### 🏆 Logros del Proyecto
- ✅ Sistema completo de gestión inmobiliaria
- ✅ Arquitectura escalable y mantenible
- ✅ Interfaz moderna y responsive
- ✅ Seguridad robusta implementada
- ✅ Integración con APIs externas
- ✅ Documentación completa
