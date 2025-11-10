# GUÍA DE PRUEBAS UNITARIAS CON XUNIT.NET
## RESUMEN EJECUTIVO Y PLAN DE ACCIÓN

---

## 📌 RESUMEN GENERAL

Este documento es un **resumen ejecutivo** de la guía completa de implementación de pruebas unitarias/integración para el proyecto **InmobiliariaGarciaJesus**, enfocado específicamente en las funcionalidades de API para propietarios:

- ✅ **Crear Inmueble** (con estado Inactivo por defecto)
- ✅ **Activar/Desactivar Inmueble** (cambio de estado)
- ✅ **Autenticación JWT** (login con usuario de prueba)
- ✅ **Manejo de Imágenes Base64**

---

## 📚 DOCUMENTACIÓN COMPLETA

La guía se divide en 4 partes:

| Documento | Contenido | Tiempo Estimado |
|-----------|-----------|-----------------|
| **[PARTE 1](GUIA_PRUEBAS_UNITARIAS_PARTE1.md)** | Análisis del proyecto, tipos de pruebas, estrategias de BD y autenticación | 1 hora lectura |
| **[PARTE 2](GUIA_PRUEBAS_UNITARIAS_PARTE2.md)** | Configuración del proyecto, paquetes NuGet, infraestructura de helpers | 2-3 horas implementación |
| **[PARTE 3](GUIA_PRUEBAS_UNITARIAS_PARTE3.md)** | Implementación de tests específicos (12 tests principales) | 4-6 horas implementación |
| **[PARTE 4](GUIA_PRUEBAS_UNITARIAS_PARTE4.md)** | Best practices, troubleshooting, plan de ejecución paso a paso | 1-2 horas refinamiento |

**Tiempo total estimado**: 8-12 horas

---

## 🎯 OBJETIVO PRINCIPAL

Implementar una suite de **12 tests de integración** que validen:

### 1. Autenticación (3 tests)
```
✅ Login exitoso con credenciales válidas
   - Email: jose.perez@email.com
   - Password: 123456
   - Response: Token JWT válido

✅ Login con password incorrecta → 401 Unauthorized

✅ Login con email inexistente → 401 Unauthorized
```

### 2. Crear Inmueble (5 tests)
```
✅ Crear inmueble con datos válidos SIN imagen
   - Estado inicial: "Inactivo" ⭐ (Requisito clave)
   - Response: 201 Created

✅ Crear inmueble CON imagen Base64
   - Imagen se guarda correctamente
   - Imagen marcada como portada

✅ Crear SIN autenticación → 401 Unauthorized

✅ Crear con direccion vacía → 400 BadRequest

✅ Crear con precio negativo → 400 BadRequest
```

### 3. Activar/Desactivar Inmueble (4 tests)
```
✅ Activar inmueble inactivo ⭐ (CASO PRINCIPAL)
   - Estado cambia de "Inactivo" a "Activo"
   - Response: 200 OK

✅ Desactivar inmueble activo
   - Estado cambia de "Activo" a "Inactivo"

✅ Intentar desactivar con contrato activo → 400 BadRequest
   - Validación de regla de negocio

✅ Modificar inmueble de otro propietario → 403 Forbidden
   - Validación de permisos
```

---

## 🏗️ ARQUITECTURA DE TESTS PROPUESTA

### Enfoque: **Pruebas de Integración con WebApplicationFactory**

**¿Por qué integración y no unitarias puras?**

1. **Flujo completo real**: HTTP Request → Controller → Service → Repository → BD → Response
2. **Autenticación JWT real**: Valida que los tokens se generan y validan correctamente
3. **Validaciones de DTOs**: Prueba que ModelState funciona
4. **Entity Framework**: Confirma que el mapeo a BD funciona
5. **Más confianza**: Si pasan, el sistema realmente funciona

**Componentes principales**:

```
CustomWebApplicationFactory
├─ BD en memoria (InMemory Database)
├─ Seed de datos comunes (TiposInmueble)
└─ Configuración de logging

AuthHelper
├─ CrearPropietarioPrueba() → Crea usuario con password hasheado
├─ ObtenerTokenAsync() → Hace login y retorna JWT
└─ CrearClienteAutenticado() → HttpClient con token en headers

TestDataBuilder
├─ CrearInmuebleDtoBuilder → DTO con valores válidos por defecto
└─ Métodos fluent para personalizar

ImageHelper
└─ GenerarImagenBase64() → PNG 1x1 pixel válido para tests
```

---

## ⚙️ TECNOLOGÍAS Y PAQUETES

### Paquetes NuGet Requeridos:

```bash
# Core de XUnit
dotnet add package xunit --version 2.6.5
dotnet add package xunit.runner.visualstudio --version 2.5.6
dotnet add package Microsoft.NET.Test.Sdk --version 17.8.0

# Testing de APIs
dotnet add package Microsoft.AspNetCore.Mvc.Testing --version 9.0.0

# Mocking (opcional)
dotnet add package Moq --version 4.20.70

# BD en memoria
dotnet add package Microsoft.EntityFrameworkCore.InMemory --version 9.0.0

# Aserciones fluidas (opcional)
dotnet add package FluentAssertions --version 6.12.0

# JSON en HTTP
dotnet add package System.Net.Http.Json --version 9.0.0
```

### Estructura del Proyecto:

```
InmobiliariaGarciaJesus.Tests/
│
├── Controllers/
│   ├── AuthApiControllerTests.cs          # 3 tests de autenticación
│   └── InmueblesApiControllerTests.cs     # 9 tests de inmuebles
│
├── Helpers/
│   ├── AuthHelper.cs                      # Login y tokens
│   ├── TestDataBuilder.cs                 # DTOs con builder pattern
│   └── ImageHelper.cs                     # Imágenes Base64 de prueba
│
├── Fixtures/
│   └── CustomWebApplicationFactory.cs     # Configuración servidor de tests
│
└── InmobiliariaGarciaJesus.Tests.csproj
```

---

## 📋 PLAN DE EJECUCIÓN PASO A PASO

### FASE 1: Setup (2 horas)
```bash
# Día 1 - Mañana
✅ 1. Crear proyecto de tests
   dotnet new xunit -n InmobiliariaGarciaJesus.Tests

✅ 2. Instalar paquetes NuGet (todos los anteriores)

✅ 3. Crear estructura de carpetas
   mkdir Controllers Helpers Fixtures

✅ 4. Implementar CustomWebApplicationFactory
   - Configurar BD en memoria
   - Seed de TiposInmueble

✅ 5. Verificar que compila
   dotnet build
   dotnet test
```

### FASE 2: Infraestructura (2-3 horas)
```bash
# Día 1 - Tarde
✅ 1. Crear AuthHelper.cs
   - CrearPropietarioPrueba()
   - ObtenerTokenAsync()
   - CrearClienteAutenticado()

✅ 2. Crear TestDataBuilder.cs
   - CrearInmuebleDtoBuilder

✅ 3. Crear ImageHelper.cs
   - GenerarImagenBase64()

✅ 4. Probar helpers con un test simple
```

### FASE 3: Tests de Autenticación (1-2 horas)
```bash
# Día 2 - Mañana
✅ 1. Implementar AuthApiControllerTests.cs
   - 3 tests de login

✅ 2. Ejecutar y verificar
   dotnet test --filter "AuthApiControllerTests"
   
   Resultado esperado: 3/3 pasan ✅
```

### FASE 4: Tests de Creación (3-4 horas)
```bash
# Día 2 - Tarde + Día 3 - Mañana
✅ 1. Implementar InmueblesApiControllerTests.cs (parte 1)
   - Test: Crear sin autenticación (más simple primero)
   - Test: Crear con datos válidos sin imagen
   - Test: Crear con imagen Base64
   - Test: Crear con direccion vacía
   - Test: Crear con precio negativo

✅ 2. Ejecutar y verificar
   dotnet test --filter "CrearInmueble"
   
   Resultado esperado: 5/5 pasan ✅
```

### FASE 5: Tests de Activación ⭐ (2-3 horas)
```bash
# Día 3 - Tarde (OBJETIVO PRINCIPAL DEL CLIENTE)
✅ 1. Implementar InmueblesApiControllerTests.cs (parte 2)
   - Test: Activar inmueble inactivo ⭐⭐⭐
   - Test: Desactivar inmueble activo
   - Test: Desactivar con contrato activo (rechazado)
   - Test: Modificar inmueble de otro propietario

✅ 2. Ejecutar y verificar
   dotnet test --filter "ActualizarEstado"
   
   Resultado esperado: 4/4 pasan ✅
```

### FASE 6: Refinamiento (1-2 horas)
```bash
# Día 4
✅ 1. Code review y limpieza

✅ 2. Crear README.md en proyecto de tests

✅ 3. Validación final
   dotnet test
   
   Resultado esperado: 12/12 pasan en < 30s ✅
```

---

## 🎬 EJEMPLO DE FLUJO COMPLETO (Test Principal)

### Test: Flujo Crear Inactivo + Activar

```csharp
[Fact]
public async Task FlujoCompleto_CrearInactivoYActivar_FuncionaCorrectamente()
{
    // ========== ARRANGE ==========
    // 1. Configurar servidor de pruebas
    await using var factory = new CustomWebApplicationFactory();
    var client = factory.CreateClient();
    
    using var scope = factory.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<InmobiliariaDbContext>();
    
    // 2. Crear propietario de prueba
    var (propietario, _) = AuthHelper.CrearPropietarioPrueba(
        context,
        email: "jose.perez@email.com",
        password: "123456"
    );
    
    // 3. Autenticarse y obtener token
    var token = await AuthHelper.ObtenerTokenAsync(client);
    var authenticatedClient = AuthHelper.CrearClienteAutenticado(factory, token);
    
    // 4. Preparar datos del inmueble con imagen
    var imagenBase64 = ImageHelper.GenerarImagenBase64();
    var crearDto = new CrearInmuebleDtoBuilder()
        .ConDireccion("Av. Test 123")
        .ConPrecio(150000m)
        .ConImagen(imagenBase64, "test.png")
        .Build();

    // ========== ACT - PARTE 1: CREAR ==========
    var crearResponse = await authenticatedClient.PostAsJsonAsync(
        "/api/InmueblesApi", 
        crearDto
    );

    // ========== ASSERT - PARTE 1 ==========
    Assert.Equal(HttpStatusCode.Created, crearResponse.StatusCode);
    
    var crearApiResponse = await crearResponse.Content
        .ReadFromJsonAsync<ApiResponse<InmuebleDto>>();
    
    var inmueble = crearApiResponse.Data;
    Assert.NotNull(inmueble);
    Assert.Equal("Inactivo", inmueble.Estado); // ⭐ Estado inicial
    Assert.NotNull(inmueble.ImagenPortadaUrl); // Imagen guardada

    // ========== ACT - PARTE 2: ACTIVAR ==========
    var activarDto = new ActualizarEstadoInmuebleDto
    {
        Estado = "Activo"
    };
    
    var activarResponse = await authenticatedClient.PatchAsync(
        $"/api/InmueblesApi/{inmueble.Id}/estado",
        JsonContent.Create(activarDto)
    );

    // ========== ASSERT - PARTE 2 ==========
    Assert.Equal(HttpStatusCode.OK, activarResponse.StatusCode);
    
    var activarApiResponse = await activarResponse.Content
        .ReadFromJsonAsync<ApiResponse<InmuebleDto>>();
    
    var inmuebleActivado = activarApiResponse.Data;
    Assert.Equal("Activo", inmuebleActivado.Estado); // ⭐ Estado actualizado
    
    // Verificar en BD
    var inmuebleDb = await context.Inmuebles.FindAsync(inmueble.Id);
    Assert.Equal(EstadoInmueble.Activo, inmuebleDb.Estado);
}
```

---

## ✅ CRITERIOS DE ÉXITO

### Al finalizar, deberías tener:

```
✅ 12 tests implementados y pasando
✅ Cobertura de InmueblesApiController > 70%
✅ Tiempo de ejecución total < 30 segundos
✅ Tests independientes (pasan en cualquier orden)
✅ Documentación completa
```

### Output esperado:

```bash
$ dotnet test

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:    12, Skipped:     0, Total:    12, Duration: 18 s

Test Run Successful.
Total tests: 12
     Passed: 12
 Total time: 18.2345 Seconds
```

---

## 🚨 PUNTOS CRÍTICOS A VALIDAR

### 1. Estado Inicial "Inactivo"
```csharp
// En el test de crear:
Assert.Equal("Inactivo", inmueble.Estado);

// En el controlador (verificar que existe esta lógica):
Estado = EstadoInmueble.Inactivo, // Por defecto hasta que admin apruebe
```

### 2. Cambio de Estado Funciona
```csharp
// En el test de activar:
Assert.Equal("Activo", inmuebleActivado.Estado);

// Verificar en BD también:
var inmuebleDb = await context.Inmuebles.FindAsync(id);
Assert.Equal(EstadoInmueble.Activo, inmuebleDb.Estado);
```

### 3. Validación de Contrato Activo
```csharp
// No debe permitir desactivar si hay contrato activo
Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
Assert.Contains("contrato", apiResponse.Message.ToLower());
```

---

## 🔍 TROUBLESHOOTING RÁPIDO

| Problema | Solución |
|----------|----------|
| **401 Unauthorized en todos los tests** | Verificar que usuario tiene `Estado = true` y que token se incluye en headers |
| **Database is already being used** | Usar `Guid.NewGuid()` en nombre de BD por cada factory |
| **File already exists** | Mock `IWebHostEnvironment` con carpeta temporal única |
| **Tests fallan intermitentemente** | Asegurar que cada test crea su propia BD y no comparte estado |

---

## 📖 REFERENCIAS

### Documentación del Proyecto:
- **API Endpoints**: `docs/ANDROID_API_ENDPOINTS.md`
- **Autenticación JWT**: Implementada en `Controllers/Api/AuthApiController.cs`
- **Controlador de Inmuebles**: `Controllers/Api/InmueblesApiController.cs`

### Ejemplo de Referencia:
- **Repositorio**: https://github.com/marianoluzza/inmobiliariaULP/tree/master/Inmobiliaria%20Tests
- Similar estructura de tests con PropietariosController

### Documentación Oficial:
- XUnit: https://xunit.net/
- ASP.NET Core Testing: https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests
- WebApplicationFactory: https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests#basic-tests-with-the-default-webapplicationfactory

---

## 🎓 CONCLUSIÓN Y PRÓXIMOS PASOS

### Has completado cuando:

1. ✅ Todos los archivos de infraestructura están creados
2. ✅ Los 3 tests de autenticación pasan
3. ✅ Los 5 tests de creación pasan (especialmente con estado Inactivo)
4. ✅ Los 4 tests de actualización pasan (especialmente activar inmueble)
5. ✅ Documentación está actualizada

### Una vez funcionando:

**Opción A**: Expandir con más tests:
- Tests de consulta (GET /api/InmueblesApi)
- Tests de edge cases (imagen corrupta, etc.)
- Tests de performance

**Opción B**: Integrar con CI/CD:
- GitHub Actions para ejecutar tests automáticamente
- Bloquear merges si tests fallan

**Opción C**: Aplicar mismo patrón a otros controladores:
- ContratosApiController
- PagosApiController
- PropietarioApiController

---

## 📞 CONTACTO Y SOPORTE

Si tienes dudas durante la implementación:

1. **Revisar troubleshooting** en Parte 4
2. **Consultar ejemplos** en Parte 3
3. **Verificar configuración** en Parte 2
4. **Re-leer análisis** en Parte 1

**Documentos relacionados**:
- `GUIA_PRUEBAS_UNITARIAS_PARTE1.md` - Análisis y decisiones
- `GUIA_PRUEBAS_UNITARIAS_PARTE2.md` - Setup técnico
- `GUIA_PRUEBAS_UNITARIAS_PARTE3.md` - Código de tests
- `GUIA_PRUEBAS_UNITARIAS_PARTE4.md` - Plan de ejecución

---

**¡Éxito con la implementación! 🚀**
