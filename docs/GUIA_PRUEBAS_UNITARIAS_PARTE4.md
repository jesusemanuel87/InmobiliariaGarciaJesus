# GUÍA DE IMPLEMENTACIÓN: PRUEBAS UNITARIAS CON XUNIT.NET
## PARTE 4: BEST PRACTICES, TROUBLESHOOTING Y PLAN DE EJECUCIÓN

---

## 16. BEST PRACTICES Y PATRONES RECOMENDADOS

### 16.1 Independencia de Tests

**❌ INCORRECTO** (Tests dependientes):
```csharp
private static int _inmuebleIdGlobal;

[Fact]
public async Task Test1_CrearInmueble()
{
    // ...
    _inmuebleIdGlobal = inmueble.Id; // ⚠️ Estado compartido
}

[Fact]
public async Task Test2_ActivarInmueble()
{
    // Depende de Test1
    var response = await client.PatchAsync($"/api/InmueblesApi/{_inmuebleIdGlobal}/estado", ...);
}
```

**✅ CORRECTO** (Tests independientes):
```csharp
[Fact]
public async Task Test1_CrearInmueble()
{
    // Crea TODO lo que necesita
    await using var factory = new CustomWebApplicationFactory();
    // ... setup completo
}

[Fact]
public async Task Test2_ActivarInmueble()
{
    // Crea TODO lo que necesita (incluyendo el inmueble a activar)
    await using var factory = new CustomWebApplicationFactory();
    // ... setup completo + crear inmueble inactivo
}
```

### 16.2 Naming Conventions

**✅ Tests descriptivos**:
```csharp
// Claro y específico
[Fact]
public async Task CrearInmueble_ConPrecioMayorA5Millones_RetornaBadRequest()

// ❌ Evitar nombres genéricos
[Fact]
public async Task Test1() 

[Fact]
public async Task TestCrear()
```

### 16.3 Usar FluentAssertions (Opcional pero recomendado)

**Con FluentAssertions**:
```csharp
inmueble.Estado.Should().Be("Inactivo");
inmueble.Imagenes.Should().NotBeNull().And.HaveCount(1);
inmueble.Precio.Should().BeGreaterThan(0);
response.StatusCode.Should().Be(HttpStatusCode.Created);
```

**Sin FluentAssertions (XUnit tradicional)**:
```csharp
Assert.Equal("Inactivo", inmueble.Estado);
Assert.NotNull(inmueble.Imagenes);
Assert.Single(inmueble.Imagenes);
Assert.True(inmueble.Precio > 0);
Assert.Equal(HttpStatusCode.Created, response.StatusCode);
```

### 16.4 Theory y InlineData para Tests Parametrizados

**Ejemplo**: Validar múltiples precios inválidos

```csharp
[Theory]
[InlineData(0)]      // Precio cero
[InlineData(-100)]   // Precio negativo
[InlineData(-0.01)]  // Precio casi cero negativo
public async Task CrearInmueble_ConPrecioInvalido_RetornaBadRequest(decimal precioInvalido)
{
    // Arrange
    await using var factory = new CustomWebApplicationFactory();
    var client = factory.CreateClient();
    
    using var scope = factory.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<InmobiliariaDbContext>();
    
    AuthHelper.CrearPropietarioPrueba(context);
    var token = await AuthHelper.ObtenerTokenAsync(client);
    var authenticatedClient = AuthHelper.CrearClienteAutenticado(factory, token);
    
    var dto = new CrearInmuebleDtoBuilder()
        .ConPrecio(precioInvalido)
        .Build();

    // Act
    var response = await authenticatedClient.PostAsJsonAsync("/api/InmueblesApi", dto);

    // Assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
}
```

---

## 17. TROUBLESHOOTING COMÚN

### 17.1 Error: "Database is already being used"

**Síntoma**:
```
System.InvalidOperationException: Cannot access a disposed context instance.
```

**Causa**: El contexto de BD se está compartiendo entre tests o se dispone antes de tiempo.

**Solución**:
```csharp
// Cada test debe crear su propio factory con GUID único
await using var factory = new CustomWebApplicationFactory();

// En CustomWebApplicationFactory:
options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}");
```

### 17.2 Error: "Unauthorized" en todos los tests

**Síntoma**: Todos los tests que requieren autenticación fallan con 401.

**Posibles causas y soluciones**:

1. **JWT Secret no configurado para tests**:
```csharp
// En CustomWebApplicationFactory, agregar:
builder.ConfigureAppConfiguration((context, config) =>
{
    config.AddJsonFile("appsettings.Test.json", optional: true);
});
```

2. **Token no se está incluyendo en headers**:
```csharp
// Verificar que se use AuthHelper.CrearClienteAutenticado()
var authenticatedClient = AuthHelper.CrearClienteAutenticado(factory, token);
// NO usar: var client = factory.CreateClient(); // ❌ Sin token
```

3. **Usuario de prueba no tiene Estado = true**:
```csharp
// En AuthHelper.CrearPropietarioPrueba():
usuario.Estado = true; // ⭐ IMPORTANTE
```

### 17.3 Error: "File already exists" o "Access denied"

**Síntoma**: Tests que suben imágenes fallan intermitentemente.

**Causa**: Las imágenes se están guardando en ubicación compartida.

**Solución**: Mock de IWebHostEnvironment con carpeta temporal única:
```csharp
// En CustomWebApplicationFactory:
builder.ConfigureServices(services =>
{
    services.AddSingleton<IWebHostEnvironment>(sp =>
    {
        var env = new Mock<IWebHostEnvironment>();
        var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempPath);
        env.Setup(e => e.WebRootPath).Returns(tempPath);
        env.Setup(e => e.EnvironmentName).Returns("Test");
        return env.Object;
    });
});
```

### 17.4 Tests fallan intermitentemente

**Síntoma**: A veces pasan, a veces fallan (flaky tests).

**Causas comunes**:
1. Estado compartido entre tests
2. Dependencia en timing (usar `Task.Delay()`)
3. Dependencia en orden de ejecución

**Solución**: Asegurar que cada test:
- Crea su propia BD con nombre único
- No depende de otros tests
- Limpia sus recursos al finalizar

---

## 18. PLAN DE IMPLEMENTACIÓN COMPLETO

### FASE 1: Setup Inicial (2 horas)

**Día 1 - Mañana**

✅ **1.1 Crear proyecto y configurar paquetes** (30 min)
```bash
dotnet new xunit -n InmobiliariaGarciaJesus.Tests
# Instalar todos los paquetes NuGet
# Agregar referencia al proyecto principal
```

✅ **1.2 Crear estructura de carpetas** (10 min)
```bash
mkdir Controllers Helpers Fixtures TestData
```

✅ **1.3 Implementar CustomWebApplicationFactory** (1 hora)
- Configurar BD en memoria
- Seed de datos comunes (TiposInmueble)
- Configurar logging

✅ **1.4 Verificar que compila** (20 min)
```bash
dotnet build
dotnet test # Debe ejecutar el test dummy
```

---

### FASE 2: Helpers e Infraestructura (2-3 horas)

**Día 1 - Tarde**

✅ **2.1 Crear AuthHelper** (1 hora)
- Método `CrearPropietarioPrueba()`
- Método `ObtenerTokenAsync()`
- Método `CrearClienteAutenticado()`
- **Probar manualmente** con un test simple

✅ **2.2 Crear TestDataBuilder** (1 hora)
- `CrearInmuebleDtoBuilder` con valores por defecto
- Métodos fluent para personalizar
- **Probar** creando algunos DTOs

✅ **2.3 Crear ImageHelper** (30 min)
- Método `GenerarImagenBase64()`
- **Verificar** que la imagen es válida

---

### FASE 3: Tests de Autenticación (1-2 horas)

**Día 2 - Mañana**

✅ **3.1 AuthApiControllerTests** (1-2 horas)
- Test: Login exitoso
- Test: Login con password incorrecta
- Test: Login con email inexistente

**Objetivo**: Validar que podemos autenticarnos y obtener tokens.

```bash
dotnet test --filter "FullyQualifiedName~AuthApiControllerTests"
```

**Criterio de éxito**: Los 3 tests pasan ✅

---

### FASE 4: Tests de Creación (3-4 horas)

**Día 2 - Tarde + Día 3 - Mañana**

✅ **4.1 InmueblesApiControllerTests - Creación** (3-4 horas)
- Test: Crear sin autenticación (más simple)
- Test: Crear con datos válidos sin imagen
- Test: Crear con imagen Base64
- Test: Crear con direccion vacía
- Test: Crear con precio negativo

**Orden recomendado**:
1. Primero el test SIN autenticación (más simple, para probar estructura)
2. Luego el test CON autenticación pero sin imagen
3. Después agregar imagen
4. Finalmente tests de validación

```bash
dotnet test --filter "CrearInmueble"
```

**Criterio de éxito**: 5 tests pasan ✅

---

### FASE 5: Tests de Actualización (2-3 horas)

**Día 3 - Tarde**

✅ **5.1 InmueblesApiControllerTests - Estado** (2-3 horas)
- Test: Activar inmueble inactivo ⭐ (MÁS IMPORTANTE)
- Test: Desactivar inmueble activo
- Test: Desactivar con contrato activo (rechazado)
- Test: Modificar inmueble de otro propietario (Forbidden)

**Este es el objetivo principal del cliente**: Probar el flujo completo de crear inactivo y luego activar.

```bash
dotnet test --filter "ActualizarEstado"
```

**Criterio de éxito**: 4 tests pasan ✅

---

### FASE 6: Refinamiento (1-2 horas)

**Día 4**

✅ **6.1 Code Review y Mejoras** (1 hora)
- Eliminar código duplicado
- Agregar comentarios a tests complejos
- Verificar nomenclatura consistente
- Asegurar que tests se ejecutan en paralelo sin problemas

✅ **6.2 Documentación** (30 min)
- Crear README.md en proyecto de tests
- Documentar cómo ejecutar tests
- Documentar estructura del proyecto

✅ **6.3 Validación Final** (30 min)
```bash
# Ejecutar TODOS los tests
dotnet test

# Ejecutar con cobertura (opcional)
dotnet test /p:CollectCoverage=true

# Ejecutar en orden aleatorio para verificar independencia
dotnet test --logger "console;verbosity=detailed"
```

**Criterio de éxito**: 
- ✅ 12 tests pasan
- ✅ Tiempo de ejecución < 30 segundos
- ✅ Sin warnings ni errores

---

## 19. COMANDOS ÚTILES DURANTE EL DESARROLLO

### Ejecutar tests

```bash
# Todos los tests
dotnet test

# Tests de un archivo específico
dotnet test --filter "FullyQualifiedName~InmueblesApiControllerTests"

# Tests que contengan una palabra
dotnet test --filter "CrearInmueble"

# Test específico
dotnet test --filter "FullyQualifiedName=InmobiliariaGarciaJesus.Tests.Controllers.InmueblesApiControllerTests.CrearInmueble_ConDatosValidos_RetornaCreated"

# Con logging verbose (para debugging)
dotnet test --logger "console;verbosity=detailed"

# Solo listar tests sin ejecutarlos
dotnet test --list-tests
```

### Cobertura de código (opcional)

```bash
# Instalar herramienta de cobertura
dotnet add package coverlet.collector

# Ejecutar con cobertura
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Ver reporte en consola
dotnet test /p:CollectCoverage=true /p:CoverletOutput=./coverage.json
```

---

## 20. CHECKLIST FINAL DE VALIDACIÓN

### ✅ Antes de considerar completado:

**Configuración**:
- [ ] Proyecto de tests compila sin errores
- [ ] Todos los paquetes NuGet instalados
- [ ] Referencia al proyecto principal configurada

**Infraestructura**:
- [ ] CustomWebApplicationFactory implementado
- [ ] AuthHelper funcional (login y tokens)
- [ ] TestDataBuilder con valores por defecto válidos
- [ ] ImageHelper genera imágenes válidas

**Tests de Autenticación** (3 tests):
- [ ] Login exitoso retorna token válido
- [ ] Login con password incorrecta retorna 401
- [ ] Login con email inexistente retorna 401

**Tests de Creación** (5 tests):
- [ ] Crear inmueble sin autenticación retorna 401
- [ ] Crear inmueble válido sin imagen retorna 201 con Estado=Inactivo
- [ ] Crear inmueble con imagen Base64 guarda imagen correctamente
- [ ] Crear con direccion vacía retorna 400
- [ ] Crear con precio negativo retorna 400

**Tests de Actualización** (4 tests):
- [ ] **Activar inmueble inactivo cambia Estado a Activo** ⭐ (PRINCIPAL)
- [ ] Desactivar inmueble activo cambia Estado a Inactivo
- [ ] Desactivar con contrato activo retorna 400 (rechazado)
- [ ] Modificar inmueble de otro propietario retorna 403

**Calidad**:
- [ ] Tests se ejecutan en < 30 segundos
- [ ] Tests pasan al ejecutarse en orden aleatorio (independientes)
- [ ] No hay warnings de compilación
- [ ] Nomenclatura consistente (Metodo_Escenario_Resultado)
- [ ] README.md documentado

---

## 21. MÉTRICAS DE ÉXITO

**Al finalizar deberías tener**:

```
✅ 12 tests implementados y pasando
✅ Cobertura de controladores API > 70%
✅ Tiempo de ejecución total < 30 segundos
✅ 0 tests flakey (no fallan intermitentemente)
✅ Documentación completa en README.md
```

**Output esperado al ejecutar `dotnet test`**:
```
Starting test execution, please wait...
A total of 1 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:    12, Skipped:     0, Total:    12, Duration: 18 s
```

---

## 22. PRÓXIMOS PASOS (Opcional)

Una vez que tengas los 12 tests principales funcionando, podrías expandir con:

### Tests Adicionales:
1. **Tests de Consulta**:
   - GET /api/InmueblesApi → Listar inmuebles del propietario
   - GET /api/InmueblesApi/{id} → Obtener detalle

2. **Tests de Edge Cases**:
   - Crear con imagen corrupta/inválida
   - Crear con TipoId inexistente
   - Actualizar estado con valores inválidos ("Pendiente", "xyz")

3. **Tests de Integración Complejos**:
   - Flujo completo: Crear → Activar → Crear Contrato → Intentar Desactivar

4. **Tests de Performance**:
   - Crear 100 inmuebles en paralelo
   - Medir tiempo de respuesta de endpoints

### Mejoras de Infraestructura:
1. **CI/CD Integration**:
   - Configurar GitHub Actions para ejecutar tests automáticamente
   - Tests deben pasar antes de hacer merge a main

2. **Cobertura Automática**:
   - Generar reporte HTML de cobertura
   - Establecer umbral mínimo (ej: 70%)

3. **Mutation Testing**:
   - Usar Stryker.NET para validar calidad de tests

---

## 23. RECURSOS Y REFERENCIAS

### Documentación Oficial:
- XUnit.net: https://xunit.net/
- ASP.NET Core Testing: https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests
- Entity Framework InMemory: https://learn.microsoft.com/en-us/ef/core/testing/

### Repositorio de Referencia:
- https://github.com/marianoluzza/inmobiliariaULP/tree/master/Inmobiliaria%20Tests

### Herramientas Útiles:
- FluentAssertions: https://fluentassertions.com/
- Moq: https://github.com/moq/moq4
- Bogus (generador de datos fake): https://github.com/bchavez/Bogus

---

## 24. CONCLUSIÓN

Este documento proporciona una guía completa para implementar pruebas unitarias/integración en el proyecto InmobiliariaGarciaJesus, con enfoque específico en:

✅ **Funcionalidad de Agregar Inmueble**:
   - Creación con estado Inactivo por defecto
   - Validación de datos de entrada
   - Manejo de imágenes en Base64

✅ **Funcionalidad de Activar Inmueble**:
   - Cambio de estado de Inactivo a Activo
   - Validación de permisos (solo propietario dueño)
   - Validación de reglas de negocio (no desactivar si hay contrato)

✅ **Autenticación JWT**:
   - Login con credenciales de prueba (jose.perez@email.com / 123456)
   - Uso de tokens en requests autenticados
   - Validación de permisos basados en propietario

La implementación sigue las mejores prácticas de testing en .NET, usando:
- **WebApplicationFactory** para tests de integración realistas
- **InMemory Database** para evitar dependencias externas
- **Patrón AAA** (Arrange-Act-Assert) para claridad
- **Builder Pattern** para construcción de datos de prueba
- **Helpers** para reducir duplicación de código

**Tiempo estimado total**: 8-12 horas de trabajo enfocado.

**Resultado final**: Suite de tests robusta que valida las funcionalidades críticas de la API para propietarios.
