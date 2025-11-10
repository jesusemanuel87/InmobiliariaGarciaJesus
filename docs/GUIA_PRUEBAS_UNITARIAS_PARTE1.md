# GUÍA DE IMPLEMENTACIÓN: PRUEBAS UNITARIAS CON XUNIT.NET
## PARTE 1: ANÁLISIS Y PLANIFICACIÓN

---

## 📋 CONTENIDO
- Parte 1: Análisis y Planificación (ESTE DOCUMENTO)
- Parte 2: Configuración e Infraestructura
- Parte 3: Implementación de Tests
- Parte 4: Casos Avanzados y Best Practices

---

## 1. ANÁLISIS DEL PROYECTO ACTUAL

### 1.1 Tecnologías Identificadas
- **Framework**: .NET 9.0
- **Base de datos**: MySQL con Entity Framework Core (Pomelo)
- **Autenticación**: JWT Bearer Token
- **Hash de contraseñas**: BCrypt.Net
- **API**: ASP.NET Core Web API con DTOs

### 1.2 Controladores API a Probar

**InmueblesApiController** - Endpoints principales:
```
POST   /api/InmueblesApi                → Crear inmueble (estado: Inactivo por defecto)
PATCH  /api/InmueblesApi/{id}/estado    → Activar/Desactivar inmueble  
GET    /api/InmueblesApi/{id}           → Obtener detalle de inmueble
GET    /api/InmueblesApi                → Listar inmuebles del propietario
```

**AuthApiController** - Para autenticación en tests:
```
POST   /api/AuthApi/login               → Obtener token JWT
```

### 1.3 Flujo de Negocio Actual
```
┌─────────────────────────────────────────────────────────────┐
│  FLUJO COMPLETO: Crear y Activar Inmueble                   │
└─────────────────────────────────────────────────────────────┘

1. Propietario se autentica
   ├─ POST /api/AuthApi/login
   ├─ Body: { "email": "jose.perez@email.com", "password": "123456" }
   └─ Response: { "token": "eyJhbGc...", "propietario": {...} }

2. Propietario crea inmueble
   ├─ POST /api/InmueblesApi
   ├─ Header: Authorization: Bearer {token}
   ├─ Body: CrearInmuebleDto con datos e imagen opcional en Base64
   └─ Response: Inmueble creado con Estado="Inactivo"

3. Admin revisa y aprueba (proceso manual actual)

4. Propietario activa inmueble
   ├─ PATCH /api/InmueblesApi/{id}/estado
   ├─ Header: Authorization: Bearer {token}
   ├─ Body: { "estado": "Activo" }
   └─ Response: Inmueble con Estado="Activo"
```

### 1.4 DTOs Identificados

**CrearInmuebleDto**:
```csharp
{
    Direccion: string (requerido, max 200 chars)
    Localidad: string (opcional, max 100 chars)
    Provincia: string (opcional, max 100 chars)
    TipoId: int (requerido)
    Ambientes: int (requerido, rango 1-100)
    Superficie: decimal? (opcional, > 0)
    Latitud: decimal? (opcional)
    Longitud: decimal? (opcional)
    Precio: decimal (requerido, > 0)
    Uso: int (requerido, 0=Residencial, 1=Comercial, 2=Industrial)
    ImagenBase64: string? (opcional)
    ImagenNombre: string? (opcional)
}
```

**ActualizarEstadoInmuebleDto**:
```csharp
{
    Estado: string? ("Activo" o "Inactivo")
    Activo: bool? (true o false)
    // Acepta ambos formatos por compatibilidad
}
```

---

## 2. ESTRUCTURA DE PRUEBAS A IMPLEMENTAR

### 2.1 Organización del Proyecto
```
InmobiliariaGarciaJesus.Tests/
│
├── Controllers/
│   ├── InmueblesApiControllerTests.cs      # Tests del controlador principal
│   └── AuthApiControllerTests.cs           # Tests de autenticación
│
├── Helpers/
│   ├── TestDataBuilder.cs                  # Builder pattern para DTOs
│   ├── AuthHelper.cs                       # Helper para login y tokens
│   ├── ImageHelper.cs                      # Helper para imágenes de prueba
│   └── AssertHelper.cs                     # Aserciones personalizadas
│
├── Fixtures/
│   ├── WebApplicationFactoryFixture.cs     # Configuración del servidor
│   └── DatabaseFixture.cs                  # Configuración de BD en memoria
│
├── TestData/
│   └── SeedData.cs                         # Datos iniciales para tests
│
├── appsettings.Test.json                   # Configuración para tests
│
└── InmobiliariaGarciaJesus.Tests.csproj
```

### 2.2 Nomenclatura de Tests (Patrón Given-When-Then)
```
[Método]_[Escenario]_[ResultadoEsperado]
```

**Ejemplos**:
```csharp
✅ CrearInmueble_ConDatosValidos_RetornaCreated()
✅ CrearInmueble_ConImagenBase64_GuardaImagenCorrectamente()
✅ ActualizarEstado_ActivarInmueble_CambiaEstadoAActivo()
✅ CrearInmueble_SinToken_RetornaUnauthorized()
✅ ActualizarEstado_InmuebleDeOtroPropietario_RetornaForbidden()
```

---

## 3. TIPOS DE PRUEBAS: INTEGRACIÓN vs UNITARIAS

### 3.1 Pruebas de Integración (⭐ Recomendado para APIs)

**¿Qué son?**
- Prueban el flujo completo: HTTP Request → Controller → Service → Repository → DB → Response
- Usan servidor de pruebas real (WebApplicationFactory)
- BD en memoria que simula MySQL
- Autenticación JWT real

**Ventajas**:
```
✅ Prueban el comportamiento real del sistema
✅ Detectan problemas de integración entre capas
✅ Validan que la autenticación JWT funciona correctamente
✅ Verifican que las validaciones de DTOs funcionan
✅ Confirman que Entity Framework mapea correctamente
✅ Detectan problemas de serialización JSON
```

**Desventajas**:
```
⚠️ Más lentas (100-500ms por test)
⚠️ Requieren más configuración inicial
⚠️ Más difíciles de debuggear (más componentes involucrados)
```

### 3.2 Pruebas Unitarias (Complementarias)

**¿Qué son?**
- Prueban componentes aislados (un método, una clase)
- Usan mocks para dependencias
- No requieren BD ni servidor

**Ventajas**:
```
✅ Muy rápidas (< 10ms por test)
✅ Fáciles de escribir y debuggear
✅ Aíslan exactamente qué está fallando
```

**Desventajas**:
```
⚠️ No detectan problemas de integración
⚠️ Requieren muchos mocks (pueden quedar desactualizados)
⚠️ No prueban el flujo completo real
```

### 3.3 Decisión: Enfoque Híbrido

**Para este proyecto se recomienda**:

| Tipo de Test | Uso | Cantidad |
|--------------|-----|----------|
| **Integración** | Flujos principales de API (Crear, Editar, Consultar) | 70% |
| **Unitarias** | Validaciones complejas, helpers, builders | 30% |

**Justificación**:
- Las APIs con JWT requieren probar el flujo completo de autenticación
- La creación con imágenes Base64 involucra múltiples capas
- Los tests de integración dan más confianza de que el sistema funciona

---

## 4. ESTRATEGIA DE BASE DE DATOS PARA TESTS

### 4.1 Opción 1: InMemory Database (⭐ Recomendado)

**Configuración**:
```csharp
services.AddDbContext<InmobiliariaDbContext>(options =>
{
    options.UseInMemoryDatabase("InmobiliariaTestDb");
});
```

**Ventajas**:
```
✅ Sin dependencias externas (no requiere MySQL instalado)
✅ Muy rápido (datos en RAM)
✅ Fácil de configurar
✅ Limpieza automática entre tests
✅ Ideal para CI/CD
```

**Limitaciones**:
```
⚠️ No soporta stored procedures ni triggers
⚠️ No valida constraints de BD (unique, foreign keys) igual que MySQL
⚠️ Algunas queries pueden comportarse diferente
```

**¿Cuándo usar?**
- ✅ 95% de los casos
- ✅ Tests de controladores API
- ✅ Tests de servicios con Entity Framework

### 4.2 Opción 2: MySQL en Docker (Casos específicos)

**Configuración**:
```yaml
# docker-compose.test.yml
services:
  mysql-test:
    image: mysql:8.0
    environment:
      MYSQL_ROOT_PASSWORD: test
      MYSQL_DATABASE: inmobiliaria_test
    ports:
      - "3307:3306"
```

**¿Cuándo usar?**
- ⚠️ Si necesitas probar stored procedures específicos
- ⚠️ Si hay queries complejas que se comportan diferente en InMemory
- ⚠️ Para tests de carga/performance

**Recomendación**: Comenzar con InMemory. Migrar a Docker solo si encuentras problemas específicos.

---

## 5. ESTRATEGIA DE AUTENTICACIÓN EN TESTS

### 5.1 Opción 1: Autenticación Real (⭐ Recomendado)

**Flujo**:
```
1. Test crea usuario de prueba en BD
   └─ Email: "jose.perez@email.com"
   └─ Password: "123456" (hasheado con BCrypt)

2. Test hace login real
   └─ POST /api/AuthApi/login
   └─ Recibe token JWT válido

3. Test usa token en requests subsiguientes
   └─ Header: Authorization: Bearer {token}

4. Middleware JWT valida el token normalmente
```

**Ventajas**:
```
✅ Prueba el flujo completo de autenticación
✅ Detecta problemas en generación/validación de tokens
✅ Test más realista (igual que producción)
✅ Valida claims y roles correctamente
```

**Desventajas**:
```
⚠️ Cada test debe hacer login (agrega ~100ms)
