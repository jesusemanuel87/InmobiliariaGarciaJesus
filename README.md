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

### Funcionalidades Futuras
- 🔄 Gestión de Pagos
- 🔄 Sistema de Usuarios y Autenticación
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
- `PropietarioId` (FK)
- `Tipo` (ENUM: Casa, Departamento, Local, Oficina, Terreno)
- `Precio` (DECIMAL)
- `Estado` (DEFAULT 1)
- `Uso` (ENUM: Residencial, Comercial, Industrial)

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
│   ├── HomeController.cs
│   ├── PropietariosController.cs
│   ├── InquilinosController.cs
│   ├── InmueblesController.cs
│   ├── ContratosController.cs
│   └── ContratoApiController.cs
├── Data/
│   └── InmobiliariaContext.cs
├── Models/
│   ├── Persona.cs (clase base)
│   ├── Propietario.cs
│   ├── Inquilino.cs
│   ├── Inmueble.cs
│   ├── Contrato.cs
│   ├── Pago.cs
│   ├── InmuebleConContrato.cs (DTO)
│   └── ErrorViewModel.cs
├── Repositories/
│   ├── IRepository.cs (interfaz genérica)
│   ├── PropietarioRepository.cs
│   ├── InquilinoRepository.cs
│   ├── InmuebleRepository.cs
│   └── ContratoRepository.cs
├── Services/
│   ├── ContratoService.cs
│   └── ContratoStateService.cs
├── Views/
│   ├── Home/
│   ├── Propietarios/
│   │   ├── Index.cshtml (con vista expandible)
│   │   ├── Inmuebles.cshtml
│   │   └── _InmueblesPartial.cshtml
│   ├── Inquilinos/
│   ├── Inmuebles/
│   ├── Contratos/
│   └── Shared/
├── wwwroot/
│   ├── css/
│   ├── js/
│   └── lib/
├── appsettings.json
├── Program.cs
├── Inmobiliaria_db.sql
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

### Base de Datos
- **MySQL 8.0**: Sistema de gestión de base de datos
- **Charset UTF8MB4**: Soporte completo para caracteres Unicode

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

## 🔄 Próximas Mejoras

1. **Implementación de ORM (Recomendado)**
   - Migrar a Entity Framework Core para mayor productividad
   - Implementar Code First con migraciones automáticas
   - Aprovechar LINQ para consultas complejas
   - Lazy loading para relaciones entre entidades

2. **Autenticación y Autorización**
   - Sistema de login/logout
   - Roles de usuario (Admin, Empleado)
   - Protección de rutas

3. **Gestión Completa**
   - CRUD de Inmuebles
   - CRUD de Contratos
   - Gestión de Pagos

4. **Funcionalidades Avanzadas**
   - Dashboard con estadísticas
   - Reportes en PDF
   - Búsqueda y filtros avanzados
   - Notificaciones de vencimientos

5. **Mejoras Técnicas**
   - API REST
   - Logging estructurado
   - Tests unitarios
   - Dockerización

## 👥 Contribución

Este proyecto fue desarrollado como parte del curso de .NET en la Universidad de La Punta (ULP).

## 📄 Licencia

Proyecto académico - Universidad de La Punta 2025

---

**Desarrollado por**: García Jesús  
**Curso**: .NET - ULP 2025  
**Fecha**: Agosto 2025
