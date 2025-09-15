# Sistema de Gestión Inmobiliaria - García Jesús

Sistema web desarrollado en ASP.NET Core MVC para la gestión de una inmobiliaria, permitiendo administrar propietarios, inquilinos, inmuebles, contratos y pagos.

## 🏗️ Arquitectura del Proyecto

- **Framework**: ASP.NET Core 9.0 MVC
- **Base de Datos**: MySQL
- **Acceso a Datos**: MySQL.Data (Conector directo)
- **Frontend**: Bootstrap 5 + Font Awesome
- **Patrón**: MVC (Model-View-Controller)

## 📋 Funcionalidades Implementadas

### Primera Entrega - CRUD Básico
- ✅ **Gestión de Propietarios**: Crear, leer, actualizar y eliminar propietarios
- ✅ **Gestión de Inquilinos**: Crear, leer, actualizar y eliminar inquilinos
- ✅ **Validaciones**: Validación de datos en cliente y servidor
- ✅ **Interfaz Responsive**: Diseño adaptable con Bootstrap

### Segunda Entrega - CRUD Inmuebles y Contratos
- ✅ **Gestión de Inmuebles**: CRUD completo con estados (Activo/Inactivo)
- ✅ **Gestión de Contratos**: CRUD completo con estados (Activo/Finalizado/Cancelado)
- ✅ **Arquitectura Repository**: Implementación del patrón Repository con inyección de dependencias
- ✅ **Herencia de Modelos**: Clase base Persona para Propietario e Inquilino
- ✅ **Vista Expandible**: Inmuebles del propietario con carga AJAX inline
- ✅ **Estados Dinámicos**: Badges coloridos para estados de inmuebles y contratos
- ✅ **Servicio de Estados**: ContratoStateService para actualización automática de contratos vencidos

### Tercera Entrega - Página Pública y Sistema de Filtros
- ✅ **Página Pública de Inmuebles**: Listado público de propiedades disponibles para alquiler
- ✅ **Sistema de Filtros Avanzados**:
  - Filtro por ubicación (Provincia/Localidad) con integración API Georef Argentina
  - Filtro por rango de fechas de disponibilidad
  - Filtro por rango de precios con slider dual interactivo
- ✅ **Slider de Precios Dual**: Implementación completa con thumbs arrastrables y fill visual
- ✅ **Integración API Georef**: Dropdowns dinámicos de provincias y localidades argentinas
- ✅ **Gestión de Imágenes**: Sistema completo de carga y visualización de imágenes de inmuebles
- ✅ **Layout Responsivo**: Diseño con sidebar de filtros y tarjetas de propiedades
- ✅ **UX Mejorada**: Hero section compacto, animaciones, efectos hover y feedback visual
- ✅ **Funcionalidades Interactivas**: Limpiar filtros, cambio de vista (cards/lista), sincronización de inputs

### Funcionalidades Futuras
- 🔄 Sistema de Usuarios y Autenticación
- 🔄 Gestión de Pagos
- 🔄 Reportes y Dashboard

## 🗄️ Estructura de Base de Datos

### Tablas Principales

#### Propietarios
- `Id` (PK, AUTO_INCREMENT)
- `DNI` (UNIQUE, NOT NULL)
- `Nombre` (NOT NULL)
- `Apellido` (NOT NULL)
- `Telefono`
- `Email` (UNIQUE, NOT NULL)
- `Direccion`
- `FechaCreacion` (DEFAULT CURRENT_TIMESTAMP)
- `Estado` (DEFAULT 1)

#### Inquilinos
- `Id` (PK, AUTO_INCREMENT)
- `DNI` (UNIQUE, NOT NULL)
- `Nombre` (NOT NULL)
- `Apellido` (NOT NULL)
- `Telefono`
- `Email` (UNIQUE, NOT NULL)
- `Direccion`
- `FechaCreacion` (DEFAULT CURRENT_TIMESTAMP)
- `Estado` (DEFAULT 1)

#### Inmuebles
- `Id` (PK, AUTO_INCREMENT)
- `Direccion` (NOT NULL)
- `Ambientes` (NOT NULL)
- `Superficie` (DECIMAL)
- `Latitud`, `Longitud` (DECIMAL)
- `Localidad`, `Provincia` (VARCHAR)
- `PropietarioId` (FK)
- `Tipo` (ENUM: Casa, Departamento, Local, Oficina, Terreno)
- `Precio` (DECIMAL)
- `Estado` (DEFAULT 1)
- `Uso` (ENUM: Residencial, Comercial, Industrial)
- `Disponible` (BOOLEAN, DEFAULT 1)

#### InmuebleImagenes
- `Id` (PK, AUTO_INCREMENT)
- `InmuebleId` (FK)
- `NombreArchivo` (VARCHAR)
- `RutaArchivo` (VARCHAR)
- `EsPortada` (BOOLEAN, DEFAULT 0)
- `TamanoArchivo` (BIGINT)
- `TipoMime` (VARCHAR)
- `FechaCreacion`, `FechaActualizacion` (TIMESTAMP)

#### Contratos
- `Id` (PK, AUTO_INCREMENT)
- `FechaInicio`, `FechaFin` (DATE)
- `Precio` (DECIMAL)
- `InquilinoId` (FK)
- `InmuebleId` (FK)
- `Estado` (ENUM: Activo, Finalizado, Cancelado)

#### Pagos
- `Id` (PK, AUTO_INCREMENT)
- `Numero` (NOT NULL)
- `FechaPago` (DATE)
- `ContratoId` (FK)
- `Importe` (DECIMAL)
- `Estado` (ENUM: Pendiente, Pagado, Vencido)

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
   - Crear la base de datos MySQL ejecutando `Inmobiliaria_db.sql`
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
│   ├── HomeController.cs (con página pública)
│   ├── PropietariosController.cs
│   ├── InquilinosController.cs
│   ├── InmueblesController.cs
│   ├── ContratosController.cs
│   ├── ContratoApiController.cs
│   ├── PagosController.cs
│   └── ConfiguracionController.cs
├── Data/
│   └── InmobiliariaContext.cs
├── Models/
│   ├── Persona.cs (clase base)
│   ├── Propietario.cs
│   ├── Inquilino.cs
│   ├── Inmueble.cs
│   ├── InmuebleImagen.cs
│   ├── Contrato.cs
│   ├── Pago.cs
│   ├── Configuracion.cs
│   ├── ViewModels/ (múltiples ViewModels)
│   └── ErrorViewModel.cs
├── Repositories/
│   ├── IRepository.cs (interfaz genérica)
│   ├── PropietarioRepository.cs
│   ├── InquilinoRepository.cs
│   ├── InmuebleRepository.cs
│   ├── InmuebleImagenRepository.cs
│   ├── ContratoRepository.cs
│   ├── PagoRepository.cs
│   └── ConfiguracionRepository.cs
├── Services/
│   ├── ContratoService.cs
│   ├── ContratoStateService.cs
│   ├── InmuebleImagenService.cs
│   ├── PagoService.cs
│   ├── ConfiguracionService.cs
│   └── PaymentBackgroundService.cs
├── Views/
│   ├── Home/
│   │   ├── Index.cshtml (página pública con filtros)
│   │   └── Privacy.cshtml
│   ├── Propietarios/
│   │   ├── Index.cshtml (con vista expandible)
│   │   ├── Inmuebles.cshtml
│   │   └── _InmueblesPartial.cshtml
│   ├── Inquilinos/
│   ├── Inmuebles/
│   ├── Contratos/
│   ├── Pagos/
│   ├── Configuracion/
│   └── Shared/
├── wwwroot/
│   ├── css/
│   │   └── Home/
│   │       └── index.css (estilos página pública)
│   ├── js/
│   │   ├── Home/
│   │   │   └── index.js (funcionalidad filtros)
│   │   └── georef-api.js (API Argentina)
│   ├── lib/
│   └── uploads/
│       └── inmuebles/ (imágenes por propiedad)
├── appsettings.json
├── Program.cs
├── Inmobiliaria_db.sql
├── GOOGLE_MAPS_SETUP.md
└── README.md
```

## 🔧 Tecnologías Utilizadas

### Backend
- **ASP.NET Core 9.0**: Framework web principal
- **MySql.Data 9.0.0**: Conector directo para MySQL
- **Data Annotations**: Validaciones de modelo
- **ADO.NET**: Acceso a datos con consultas SQL nativas

### Frontend
- **Bootstrap 5**: Framework CSS para diseño responsive
- **Font Awesome 6**: Iconografía
- **jQuery**: Manipulación DOM y AJAX
- **HTML5 & CSS3**: Estructura y estilos
- **API Georef Argentina**: Integración con servicio gubernamental para ubicaciones

### Base de Datos
- **MySQL 8.0**: Sistema de gestión de base de datos
- **Charset UTF8MB4**: Soporte completo para caracteres Unicode

### APIs Externas
- **Google Maps API**: Visualización de mapas y ubicaciones
- **API Georef**: Servicio del gobierno argentino para provincias y localidades

## 🎯 Características Técnicas

### Validaciones
- **Cliente**: Validación en tiempo real con jQuery
- **Servidor**: Data Annotations y validaciones personalizadas
- **Unicidad**: DNI y Email únicos por entidad

### Seguridad
- **Eliminación Lógica**: Los registros se marcan como inactivos
- **Restricciones FK**: Prevención de eliminación con datos relacionados
- **Validación CSRF**: Protección contra ataques Cross-Site Request Forgery

### UX/UI
- **Diseño Responsive**: Adaptable a dispositivos móviles
- **Iconografía Consistente**: Font Awesome en toda la aplicación
- **Mensajes de Estado**: Feedback visual para operaciones
- **Navegación Intuitiva**: Menú claro y accesible
- **Página Pública Atractiva**: Hero section, tarjetas de propiedades con animaciones
- **Filtros Interactivos**: Slider dual de precios, dropdowns dinámicos
- **Experiencia de Usuario Optimizada**: Auto-filtrado, limpiar filtros, cambio de vista

### Gestión de Archivos
- **Sistema de Imágenes**: Carga múltiple por inmueble con imagen de portada
- **Organización por Carpetas**: Estructura `/uploads/inmuebles/{id}/`
- **Validaciones**: Tamaño máximo, tipos de archivo permitidos
- **Gestión Automática**: Asignación de portada, eliminación en cascada

### Servicios en Background
- **PaymentBackgroundService**: Actualización automática de estados de pago cada hora
- **Cálculo de Intereses**: Aplicación automática de penalizaciones por mora
- **Logging Estructurado**: Seguimiento de operaciones automáticas

## 🔄 Próximas Mejoras

### **Cuarta Entrega - Autenticación y Autorización** 🎯
- **Sistema de Login/Logout**: Implementación de Identity Framework
- **Roles de Usuario**: Admin, Empleado, Usuario Público
- **Protección de Rutas**: Middleware de autorización
- **Gestión de Sesiones**: Control de acceso y permisos
- **Área de Administración**: Panel protegido para gestión

### Funcionalidades Futuras
1. **Dashboard con Estadísticas**
   - Métricas de inmuebles, contratos y pagos
   - Gráficos interactivos
   - Reportes de rendimiento

2. **Reportes Avanzados**
   - Generación de PDF
   - Reportes de pagos y vencimientos
   - Estadísticas por período

3. **Mejoras Técnicas**
   - API REST completa
   - Tests unitarios e integración
   - Logging estructurado con Serilog
   - Dockerización para deployment

## 👥 Contribución

Este proyecto fue desarrollado como parte del curso de .NET en la Universidad de La Punta (ULP).

## 📄 Licencia

Proyecto académico - Universidad de La Punta 2025

---

**Desarrollado por**: García Jesús  
**Curso**: .NET - ULP 2025  
**Fecha Inicio**: Agosto 2025
**Fecha Fin**: Septiembre 2025
