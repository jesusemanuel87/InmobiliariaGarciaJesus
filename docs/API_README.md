# 📱 API REST - Inmobiliaria García Jesús

**Versión**: 1.0  
**Tecnología**: ASP.NET Core 9.0 + EF Core + JWT  
**Base de Datos**: MySQL

---

## 📋 Funcionalidades Implementadas (Primera Entrega)

### ✅ Autenticación
- **Login/Logout** de propietarios con JWT
- **Cambiar contraseña** (requiere password actual)
- **Reset de contraseña** (olvidé mi contraseña) - valida Email + DNI

### ✅ Perfil de Propietario
- **Ver perfil** completo
- **Editar perfil** (nombre, apellido, teléfono, dirección)
- **Subir foto de perfil** (jpg, png, gif, webp - max 5MB)

### ✅ Gestión de Inmuebles
- **Listar inmuebles** del propietario autenticado
- **Ver detalle** de un inmueble específico
- **Agregar nuevo inmueble** con foto (por defecto **deshabilitado** según requisitos)
- **Habilitar/Deshabilitar** un inmueble

### ✅ Contratos y Pagos
- **Listar contratos por inmueble** con todos sus pagos
- **Ver detalle de contrato** individual
- **Listar todos los contratos** de todos los inmuebles del propietario

---

## 🔐 Seguridad Implementada

- **Autenticación JWT**: Tokens con expiración de 24 horas
- **Autorización por Claims**: El propietarioId se extrae del token (NO se envía en requests)
- **Validación de permisos**: Solo se puede acceder a inmuebles/contratos propios
- **CORS configurado**: Permite requests desde aplicación móvil
- **BCrypt**: Hashing seguro de contraseñas

---

## 🚀 Endpoints Disponibles

### 📍 Base URL
```
http://localhost:5000/api
https://localhost:7000/api  (HTTPS)
```

### 📍 Swagger Documentation
```
http://localhost:5000/api/docs
```

### **Authentication** (`/api/AuthApi`)
| Método | Endpoint | Descripción | Auth |
|--------|----------|-------------|------|
| POST | `/login` | Login de propietario | ❌ |
| POST | `/cambiar-password` | Cambiar contraseña | ✅ |
| POST | `/reset-password` | Resetear contraseña (olvidé) | ❌ |

### **Propietario** (`/api/PropietarioApi`)
| Método | Endpoint | Descripción | Auth |
|--------|----------|-------------|------|
| GET | `/perfil` | Obtener perfil | ✅ |
| PUT | `/perfil` | Actualizar perfil | ✅ |
| POST | `/perfil/foto` | Subir foto de perfil | ✅ |

### **Inmuebles** (`/api/InmueblesApi`)
| Método | Endpoint | Descripción | Auth |
|--------|----------|-------------|------|
| GET | `/` | Listar mis inmuebles | ✅ |
| GET | `/{id}` | Detalle de inmueble | ✅ |
| POST | `/` | Crear inmueble (inactivo) | ✅ |
| PATCH | `/{id}/estado` | Habilitar/Deshabilitar | ✅ |

### **Contratos** (`/api/ContratosApi`)
| Método | Endpoint | Descripción | Auth |
|--------|----------|-------------|------|
| GET | `/` | Todos mis contratos | ✅ |
| GET | `/{id}` | Detalle de contrato | ✅ |
| GET | `/inmueble/{inmuebleId}` | Contratos de un inmueble | ✅ |

---

## 📦 Estructura del Proyecto

```
InmobiliariaGarciaJesus/
├── Controllers/
│   └── Api/                        # 🆕 Controllers de la API REST
│       ├── AuthApiController.cs
│       ├── PropietarioApiController.cs
│       ├── InmueblesApiController.cs
│       └── ContratosApiController.cs
├── Data/
│   └── InmobiliariaDbContext.cs    # 🆕 DbContext de EF Core
├── Models/
│   ├── DTOs/                       # 🆕 Data Transfer Objects
│   │   ├── ApiResponse.cs
│   │   ├── AuthDto.cs
│   │   ├── PropietarioDto.cs
│   │   ├── InmuebleDto.cs
│   │   ├── ContratoDto.cs
│   │   └── PagoDto.cs
│   └── [Modelos existentes]
├── Services/
│   ├── JwtService.cs               # 🆕 Generación y validación JWT
│   └── [Servicios existentes]
└── docs/
    ├── API_README.md               # 📄 Este archivo
    ├── ANDROID_MODELS.md           # 📄 Modelos Kotlin
    └── ANDROID_RETROFIT_SETUP.md   # 📄 Configuración Retrofit
```

---

## ⚙️ Configuración

### **appsettings.json**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=inmobiliaria;Uid=root;Pwd=;Port=3306;CharSet=utf8mb4;"
  },
  "JwtSettings": {
    "SecretKey": "InmobiliariaGarciaJesus_SecretKey_2025_MuySegura_MinimoDe32Caracteres!",
    "Issuer": "InmobiliariaGarciaJesusAPI",
    "Audience": "InmobiliariaMovilApp",
    "ExpirationMinutes": "1440"
  }
}
```

---

## 🎯 Próximos Pasos para Android

1. **Ver documentación de modelos Kotlin**: `docs/ANDROID_MODELS.md`
2. **Configurar Retrofit**: `docs/ANDROID_RETROFIT_SETUP.md`
3. **Probar endpoints con Swagger**: `http://localhost:5000/api/docs`

---

## 🧪 Ejecutar la API

```bash
# Restaurar paquetes
dotnet restore

# Ejecutar en modo desarrollo
dotnet run

# URL: http://localhost:5000 o https://localhost:7000
```

---

## 📖 Arquitectura Clean

El proyecto sigue principios de **Clean Architecture**:

- **Separación de responsabilidades**: Controllers → Services → Repositories → Data
- **DTOs para API**: Separados de los modelos de dominio
- **Inyección de dependencias**: Todo configurado en Program.cs
- **EF Core**: ORM para acceso a datos (reemplaza ADO.NET)
- **JWT Service**: Lógica de autenticación centralizada
- **CORS**: Configurado para aplicaciones móviles
- **Swagger**: Documentación interactiva automática

---

## 📞 Contacto

**Desarrollador**: García Jesús Emanuel  
**Materia**: Desarrollo Web-API Android - ASP.NET  
**Año**: 2025
