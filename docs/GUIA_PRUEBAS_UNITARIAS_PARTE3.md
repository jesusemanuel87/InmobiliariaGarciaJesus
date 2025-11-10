# GUÍA DE IMPLEMENTACIÓN: PRUEBAS UNITARIAS CON XUNIT.NET
## PARTE 3: IMPLEMENTACIÓN DE TESTS ESPECÍFICOS

---

## 12. TESTS DE AUTENTICACIÓN

### 12.1 Archivo: `Controllers/AuthApiControllerTests.cs`

**Objetivo**: Validar que el sistema de autenticación funciona correctamente

#### Test 1: Login exitoso con credenciales válidas

```csharp
[Fact]
public async Task Login_ConCredencialesValidas_RetornaTokenYDatosPropietario()
{
    // Arrange
    await using var factory = new CustomWebApplicationFactory();
    var client = factory.CreateClient();
    
    using var scope = factory.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<InmobiliariaDbContext>();
    
    // Crear propietario de prueba
    AuthHelper.CrearPropietarioPrueba(context, 
        email: "jose.perez@email.com", 
        password: "123456");
    
    var loginDto = new LoginRequestDto
    {
        Email = "jose.perez@email.com",
        Password = "123456"
    };

    // Act
    var response = await client.PostAsJsonAsync("/api/AuthApi/login", loginDto);

    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    
    var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponseDto>>();
    Assert.NotNull(apiResponse);
    Assert.True(apiResponse.Success);
    
    var loginResponse = apiResponse.Data;
    Assert.NotNull(loginResponse);
    Assert.NotNull(loginResponse.Token);
    Assert.NotEmpty(loginResponse.Token);
    Assert.NotNull(loginResponse.Propietario);
    Assert.Equal("José", loginResponse.Propietario.Nombre);
    Assert.Equal("Pérez", loginResponse.Propietario.Apellido);
    Assert.True(loginResponse.Expiracion > DateTime.UtcNow);
}
```

#### Test 2: Login con contraseña incorrecta

```csharp
[Fact]
public async Task Login_ConPasswordIncorrecta_RetornaUnauthorized()
{
    // Arrange
    await using var factory = new CustomWebApplicationFactory();
    var client = factory.CreateClient();
    
    using var scope = factory.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<InmobiliariaDbContext>();
    
    AuthHelper.CrearPropietarioPrueba(context, password: "123456");
    
    var loginDto = new LoginRequestDto
    {
        Email = "jose.perez@email.com",
        Password = "incorrecta"
    };

    // Act
    var response = await client.PostAsJsonAsync("/api/AuthApi/login", loginDto);

    // Assert
    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    
    var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
    Assert.NotNull(apiResponse);
    Assert.False(apiResponse.Success);
    Assert.Contains("Credenciales inválidas", apiResponse.Message);
}
```

#### Test 3: Login con email inexistente

```csharp
[Fact]
public async Task Login_ConEmailInexistente_RetornaUnauthorized()
{
    // Arrange
    await using var factory = new CustomWebApplicationFactory();
    var client = factory.CreateClient();
    
    var loginDto = new LoginRequestDto
    {
        Email = "noexiste@email.com",
        Password = "123456"
    };

    // Act
    var response = await client.PostAsJsonAsync("/api/AuthApi/login", loginDto);

    // Assert
    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
}
```

---

## 13. TESTS DE CREACIÓN DE INMUEBLE

### 13.1 Archivo: `Controllers/InmueblesApiControllerTests.cs`

#### Test 4: Crear inmueble con datos válidos SIN imagen

```csharp
[Fact]
public async Task CrearInmueble_ConDatosValidosSinImagen_RetornaCreatedConEstadoInactivo()
{
    // Arrange
    await using var factory = new CustomWebApplicationFactory();
    var client = factory.CreateClient();
    
    using var scope = factory.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<InmobiliariaDbContext>();
    
    var (propietario, _) = AuthHelper.CrearPropietarioPrueba(context);
    var token = await AuthHelper.ObtenerTokenAsync(client);
    var authenticatedClient = AuthHelper.CrearClienteAutenticado(factory, token);
    
    var dto = new CrearInmuebleDtoBuilder()
        .ConDireccion("Av. Illia 456")
        .ConPrecio(180000m)
        .Build();

    // Act
    var response = await authenticatedClient.PostAsJsonAsync("/api/InmueblesApi", dto);

    // Assert
    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    
    var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<InmuebleDto>>();
    Assert.NotNull(apiResponse);
    Assert.True(apiResponse.Success);
    
    var inmueble = apiResponse.Data;
    Assert.NotNull(inmueble);
    Assert.True(inmueble.Id > 0);
    Assert.Equal("Inactivo", inmueble.Estado); // ⭐ VERIFICAR ESTADO INICIAL
    Assert.Equal("Disponible", inmueble.Disponibilidad);
    Assert.Equal(dto.Direccion, inmueble.Direccion);
    Assert.Equal(dto.Precio, inmueble.Precio);
    Assert.Equal(dto.Ambientes, inmueble.Ambientes);
    
    // Verificar en BD
    var inmuebleDb = await context.Inmuebles.FindAsync(inmueble.Id);
    Assert.NotNull(inmuebleDb);
    Assert.Equal(propietario.Id, inmuebleDb.PropietarioId);
    Assert.Equal(EstadoInmueble.Inactivo, inmuebleDb.Estado);
    
    // Verificar header Location
    Assert.NotNull(response.Headers.Location);
    Assert.Contains($"/api/InmueblesApi/{inmueble.Id}", response.Headers.Location.ToString());
}
```

#### Test 5: Crear inmueble con imagen Base64

```csharp
[Fact]
public async Task CrearInmueble_ConImagenBase64_GuardaImagenCorrectamente()
{
    // Arrange
    await using var factory = new CustomWebApplicationFactory();
    var client = factory.CreateClient();
    
    using var scope = factory.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<InmobiliariaDbContext>();
    
    AuthHelper.CrearPropietarioPrueba(context);
    var token = await AuthHelper.ObtenerTokenAsync(client);
    var authenticatedClient = AuthHelper.CrearClienteAutenticado(factory, token);
    
    var imagenBase64 = ImageHelper.GenerarImagenBase64();
    var dto = new CrearInmuebleDtoBuilder()
        .ConImagen(imagenBase64, "test-image.png")
        .Build();

    // Act
    var response = await authenticatedClient.PostAsJsonAsync("/api/InmueblesApi", dto);

    // Assert
    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    
    var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<InmuebleDto>>();
    var inmueble = apiResponse.Data;
    
    // Verificar que la imagen se guardó
    Assert.NotNull(inmueble.Imagenes);
    Assert.Single(inmueble.Imagenes); // Una imagen
    Assert.True(inmueble.Imagenes[0].EsPortada); // Marcada como portada
    Assert.NotNull(inmueble.ImagenPortadaUrl); // URL no nula
    Assert.Contains("/uploads/inmuebles/", inmueble.ImagenPortadaUrl);
    
    // Verificar en BD
    var imagenDb = await context.InmuebleImagenes
        .FirstOrDefaultAsync(i => i.InmuebleId == inmueble.Id);
    Assert.NotNull(imagenDb);
    Assert.True(imagenDb.EsPortada);
    Assert.NotNull(imagenDb.NombreArchivo);
}
```

#### Test 6: Crear inmueble SIN autenticación

```csharp
[Fact]
public async Task CrearInmueble_SinToken_RetornaUnauthorized()
{
    // Arrange
    await using var factory = new CustomWebApplicationFactory();
    var client = factory.CreateClient(); // Cliente SIN token
    
    var dto = new CrearInmuebleDtoBuilder().Build();

    // Act
    var response = await client.PostAsJsonAsync("/api/InmueblesApi", dto);

    // Assert
    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
}
```

#### Test 7: Crear inmueble con datos inválidos (Direccion vacía)

```csharp
[Fact]
public async Task CrearInmueble_ConDireccionVacia_RetornaBadRequest()
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
        .ConDireccion("") // ⚠️ Inválido
        .Build();

    // Act
    var response = await authenticatedClient.PostAsJsonAsync("/api/InmueblesApi", dto);

    // Assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    
    var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
    Assert.NotNull(apiResponse);
    Assert.False(apiResponse.Success);
    Assert.Contains("dirección", apiResponse.Message.ToLower());
}
```

#### Test 8: Crear inmueble con precio negativo

```csharp
[Fact]
public async Task CrearInmueble_ConPrecioNegativo_RetornaBadRequest()
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
        .ConPrecio(-1000m) // ⚠️ Inválido
        .Build();

    // Act
    var response = await authenticatedClient.PostAsJsonAsync("/api/InmueblesApi", dto);

    // Assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
}
```

---

## 14. TESTS DE ACTUALIZACIÓN DE ESTADO

#### Test 9: Activar inmueble que está Inactivo

```csharp
[Fact]
public async Task ActualizarEstado_ActivarInmuebleInactivo_CambiaEstadoAActivo()
{
    // Arrange
    await using var factory = new CustomWebApplicationFactory();
    var client = factory.CreateClient();
    
    using var scope = factory.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<InmobiliariaDbContext>();
    
    var (propietario, _) = AuthHelper.CrearPropietarioPrueba(context);
    var token = await AuthHelper.ObtenerTokenAsync(client);
    var authenticatedClient = AuthHelper.CrearClienteAutenticado(factory, token);
    
    // Crear inmueble inactivo
    var inmueble = new Inmueble
    {
        Direccion = "Test 123",
        TipoId = 1,
        Ambientes = 2,
        Precio = 100000m,
        Uso = UsoInmueble.Residencial,
        PropietarioId = propietario.Id,
        Estado = EstadoInmueble.Inactivo, // ⭐ Inactivo inicialmente
        FechaCreacion = DateTime.Now
    };
    context.Inmuebles.Add(inmueble);
    await context.SaveChangesAsync();
    
    var updateDto = new ActualizarEstadoInmuebleDto
    {
        Estado = "Activo"
    };

    // Act
    var response = await authenticatedClient.PatchAsync(
        $"/api/InmueblesApi/{inmueble.Id}/estado",
        JsonContent.Create(updateDto));

    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    
    var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<InmuebleDto>>();
    Assert.NotNull(apiResponse);
    Assert.True(apiResponse.Success);
    
    var inmuebleActualizado = apiResponse.Data;
    Assert.Equal("Activo", inmuebleActualizado.Estado); // ⭐ Verificar cambio
    
    // Verificar en BD
    var inmuebleDb = await context.Inmuebles.FindAsync(inmueble.Id);
    Assert.Equal(EstadoInmueble.Activo, inmuebleDb.Estado);
}
```

#### Test 10: Desactivar inmueble que está Activo

```csharp
[Fact]
public async Task ActualizarEstado_DesactivarInmuebleActivo_CambiaEstadoAInactivo()
{
    // Arrange
    await using var factory = new CustomWebApplicationFactory();
    var client = factory.CreateClient();
    
    using var scope = factory.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<InmobiliariaDbContext>();
    
    var (propietario, _) = AuthHelper.CrearPropietarioPrueba(context);
    var token = await AuthHelper.ObtenerTokenAsync(client);
    var authenticatedClient = AuthHelper.CrearClienteAutenticado(factory, token);
    
    // Crear inmueble ACTIVO
    var inmueble = new Inmueble
    {
        Direccion = "Test 456",
        TipoId = 1,
        Ambientes = 3,
        Precio = 150000m,
        Uso = UsoInmueble.Residencial,
        PropietarioId = propietario.Id,
        Estado = EstadoInmueble.Activo, // ⭐ Activo inicialmente
        FechaCreacion = DateTime.Now
    };
    context.Inmuebles.Add(inmueble);
    await context.SaveChangesAsync();
    
    var updateDto = new ActualizarEstadoInmuebleDto
    {
        Estado = "Inactivo"
    };

    // Act
    var response = await authenticatedClient.PatchAsync(
        $"/api/InmueblesApi/{inmueble.Id}/estado",
        JsonContent.Create(updateDto));

    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    
    var inmuebleDb = await context.Inmuebles.FindAsync(inmueble.Id);
    Assert.Equal(EstadoInmueble.Inactivo, inmuebleDb.Estado);
}
```

#### Test 11: Intentar desactivar inmueble con contrato activo

```csharp
[Fact]
public async Task ActualizarEstado_DesactivarConContratoActivo_RetornaBadRequest()
{
    // Arrange
    await using var factory = new CustomWebApplicationFactory();
    var client = factory.CreateClient();
    
    using var scope = factory.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<InmobiliariaDbContext>();
    
    var (propietario, _) = AuthHelper.CrearPropietarioPrueba(context);
    var token = await AuthHelper.ObtenerTokenAsync(client);
    var authenticatedClient = AuthHelper.CrearClienteAutenticado(factory, token);
    
    // Crear inmueble activo
    var inmueble = new Inmueble
    {
        Direccion = "Test 789",
        TipoId = 1,
        Ambientes = 2,
        Precio = 120000m,
        Uso = UsoInmueble.Residencial,
        PropietarioId = propietario.Id,
        Estado = EstadoInmueble.Activo,
        FechaCreacion = DateTime.Now
    };
    context.Inmuebles.Add(inmueble);
    await context.SaveChangesAsync();
    
    // Crear contrato ACTIVO asociado
    var inquilino = new Inquilino
    {
        Nombre = "Juan",
        Apellido = "Test",
        Dni = "99887766",
        Email = "juan@test.com",
        Estado = true,
        FechaCreacion = DateTime.Now
    };
    context.Inquilinos.Add(inquilino);
    await context.SaveChangesAsync();
    
    var contrato = new Contrato
    {
        InmuebleId = inmueble.Id,
        InquilinoId = inquilino.Id,
        FechaInicio = DateTime.Now,
        FechaFin = DateTime.Now.AddYears(1),
        Monto = 120000m,
        Estado = EstadoContrato.Activo, // ⭐ Contrato ACTIVO
        FechaCreacion = DateTime.Now
    };
    context.Contratos.Add(contrato);
    await context.SaveChangesAsync();
    
    var updateDto = new ActualizarEstadoInmuebleDto
    {
        Estado = "Inactivo"
    };

    // Act
    var response = await authenticatedClient.PatchAsync(
        $"/api/InmueblesApi/{inmueble.Id}/estado",
        JsonContent.Create(updateDto));

    // Assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    
    var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
    Assert.NotNull(apiResponse);
    Assert.False(apiResponse.Success);
    Assert.Contains("contrato", apiResponse.Message.ToLower());
    
    // Verificar que el estado NO cambió
    var inmuebleDb = await context.Inmuebles.FindAsync(inmueble.Id);
    Assert.Equal(EstadoInmueble.Activo, inmuebleDb.Estado); // Sigue activo
}
```

#### Test 12: Intentar modificar inmueble de otro propietario

```csharp
[Fact]
public async Task ActualizarEstado_InmuebleDeOtroPropietario_RetornaForbidden()
{
    // Arrange
    await using var factory = new CustomWebApplicationFactory();
    var client = factory.CreateClient();
    
    using var scope = factory.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<InmobiliariaDbContext>();
    
    // Crear propietario A (autenticado)
    var (propietarioA, _) = AuthHelper.CrearPropietarioPrueba(context, 
        email: "propietarioA@email.com", 
        dni: "11111111");
    
    // Crear propietario B (dueño del inmueble)
    var propietarioB = new Propietario
    {
        Nombre = "Propietario",
        Apellido = "B",
        Dni = "22222222",
        Email = "propietarioB@email.com",
        Estado = true,
        FechaCreacion = DateTime.Now
    };
    context.Propietarios.Add(propietarioB);
    await context.SaveChangesAsync();
    
    // Crear inmueble del propietario B
    var inmueble = new Inmueble
    {
        Direccion = "Inmueble de B",
        TipoId = 1,
        Ambientes = 2,
        Precio = 100000m,
        Uso = UsoInmueble.Residencial,
        PropietarioId = propietarioB.Id, // ⭐ Pertenece a B
        Estado = EstadoInmueble.Inactivo,
        FechaCreacion = DateTime.Now
    };
    context.Inmuebles.Add(inmueble);
    await context.SaveChangesAsync();
    
    // Autenticar como propietario A
    var token = await AuthHelper.ObtenerTokenAsync(client, "propietarioA@email.com");
    var authenticatedClient = AuthHelper.CrearClienteAutenticado(factory, token);
    
    var updateDto = new ActualizarEstadoInmuebleDto
    {
        Estado = "Activo"
    };

    // Act
    var response = await authenticatedClient.PatchAsync(
        $"/api/InmueblesApi/{inmueble.Id}/estado",
        JsonContent.Create(updateDto));

    // Assert
    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    
    var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
    Assert.Contains("permiso", apiResponse.Message.ToLower());
}
```

---

## 15. RESUMEN DE TESTS IMPLEMENTADOS

```
AuthApiControllerTests (3 tests)
├─ ✅ Login exitoso con credenciales válidas
├─ ✅ Login con contraseña incorrecta
└─ ✅ Login con email inexistente

InmueblesApiControllerTests (9 tests)
├─ ✅ Crear inmueble válido sin imagen
├─ ✅ Crear inmueble con imagen Base64
├─ ✅ Crear sin autenticación
├─ ✅ Crear con datos inválidos
├─ ✅ Crear con precio negativo
├─ ✅ Activar inmueble inactivo
├─ ✅ Desactivar inmueble activo
├─ ✅ Desactivar con contrato activo (rechazado)
└─ ✅ Modificar inmueble ajeno (Forbidden)

TOTAL: 12 tests principales
```

---

**Continúa en**: GUIA_PRUEBAS_UNITARIAS_PARTE4.md (Best Practices y Casos Avanzados)
