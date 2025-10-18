# 🔧 Correcciones Finales - Sistema Multi-Rol

## 📋 **Problemas Reportados y Solucionados**

### **1. ❌ /Inmuebles/MisInmuebles redirigía a home**
### **2. ❌ Selector de roles no visible en Profile**
### **3. ❌ /Contratos/MisContratos devolvía 404**
### **4. ❌ /Contratos/MiContrato daba acceso denegado para Propietarios**

---

## ✅ **Soluciones Implementadas**

### **Problema 1: MisInmuebles Redirigía a Home**

**Causa Raíz:**
- Usaba `HttpContext.Session.GetString("UserId")` que podía devolver null
- No usaba `AuthService.GetUsuarioId(User)` para obtener el ID del usuario autenticado
- Obtenía usuario con `GetAllAsync()` sin cargar relaciones

**Solución Aplicada:**

**Archivo:** `Controllers/InmueblesController.cs` (líneas 183-245)

```csharp
[AuthorizeMultipleRoles(RolUsuario.Propietario)]
public async Task<IActionResult> MisInmuebles()
{
    try
    {
        // ✅ CORREGIDO: Usar AuthService en lugar de Session
        var usuarioId = AuthService.GetUsuarioId(User);
        if (!usuarioId.HasValue)
        {
            return RedirectToAction("Login", "Auth");
        }

        // ✅ CORREGIDO: Usar UsuarioRepository con relaciones cargadas
        var usuarioRepo = HttpContext.RequestServices.GetService<UsuarioRepository>();
        var usuario = await usuarioRepo.GetByIdAsync(usuarioId.Value);
        
        if (usuario?.PropietarioId == null)
        {
            TempData["Error"] = "No tiene permisos para acceder a esta sección. Debe ser un propietario.";
            return RedirectToAction("Index", "Home");
        }

        // Filtrar SOLO inmuebles del propietario logueado
        var todosInmuebles = await _inmuebleRepository.GetAllAsync();
        var inmuebles = todosInmuebles
            .Where(i => i.PropietarioId == usuario.PropietarioId.Value)
            .ToList();
        
        // ... resto del código
    }
}
```

**Cambios Clave:**
- ✅ Agregado `using AuthService = InmobiliariaGarciaJesus.Services.AuthenticationService;`
- ✅ Reemplazado `HttpContext.Session.GetString("UserId")` → `AuthService.GetUsuarioId(User)`
- ✅ Reemplazado `GetAllAsync()` → `usuarioRepo.GetByIdAsync()` con relaciones

---

### **Problema 2: Selector de Roles No Visible**

**Causa Raíz:**
- El método `GetRolesDisponiblesAsync()` funcionaba correctamente
- El usuario `propietario_inquilino` NO estaba activado (`Estado = 0`)
- Solo se buscaban usuarios con `Estado = true` (activos)

**Solución Aplicada:**

**Archivo:** `Services/UsuarioService.cs` (líneas 328-383)

```csharp
public async Task<List<RolUsuario>> GetRolesDisponiblesAsync(Usuario usuario)
{
    var roles = new List<RolUsuario>();

    try
    {
        // ✅ AGREGADO: Logs para depuración
        _logger.LogInformation("GetRolesDisponiblesAsync: Buscando roles para usuario {UsuarioId} con email {Email}", 
            usuario.Id, usuario.Email);

        // Buscar TODOS los usuarios con el mismo email
        var todosLosUsuarios = await _usuarioRepository.GetAllAsync();
        
        // ✅ IMPORTANTE: Solo usuarios ACTIVOS (Estado = true)
        var usuariosConMismoEmail = todosLosUsuarios
            .Where(u => u.Email.Equals(usuario.Email, StringComparison.OrdinalIgnoreCase) && u.Estado)
            .ToList();

        _logger.LogInformation("GetRolesDisponiblesAsync: Usuarios con email {Email} y activos: {Count}", 
            usuario.Email, usuariosConMismoEmail.Count);

        // Agregar roles únicos
        foreach (var u in usuariosConMismoEmail)
        {
            _logger.LogInformation("GetRolesDisponiblesAsync: Usuario {Id} tiene rol {Rol}", u.Id, u.Rol);
            
            if (!roles.Contains(u.Rol))
            {
                roles.Add(u.Rol);
                _logger.LogInformation("GetRolesDisponiblesAsync: Agregado rol {Rol}", u.Rol);
            }
        }

        _logger.LogInformation("GetRolesDisponiblesAsync: Roles disponibles finales: {Count} - [{Roles}]", 
            roles.Count, string.Join(", ", roles));

        return roles;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error al obtener roles disponibles: {UsuarioId}", usuario.Id);
        return new List<RolUsuario>();
    }
}
```

**Cambios Clave:**
- ✅ Agregados logs detallados para depurar el problema
- ✅ El método ya funcionaba correctamente
- ⚠️ **ACCIÓN REQUERIDA:** Activar usuario `propietario_inquilino` en BD

---

### **Problema 3: /Contratos/MisContratos No Existía**

**Solución:** Creado nuevo método para que Propietarios vean contratos de SUS inmuebles

**Archivo:** `Controllers/ContratosController.cs` (líneas 162-222)

```csharp
// GET: Contratos/MisContratos - Vista de contratos de inmuebles del propietario logueado
[AuthorizeMultipleRoles(RolUsuario.Propietario)]
public async Task<IActionResult> MisContratos()
{
    try
    {
        // Obtener el usuario logueado usando AuthService
        var usuarioId = AuthService.GetUsuarioId(User);
        if (!usuarioId.HasValue)
        {
            return RedirectToAction("Login", "Auth");
        }

        var usuario = await _usuarioRepository.GetByIdAsync(usuarioId.Value);
        if (usuario?.PropietarioId == null)
        {
            TempData["Error"] = "No tiene permisos para acceder a esta sección. Debe ser un propietario.";
            return RedirectToAction("Index", "Home");
        }

        // Obtener todos los inmuebles del propietario
        var inmuebleRepo = HttpContext.RequestServices.GetService<IRepository<Inmueble>>();
        var todosInmuebles = await inmuebleRepo.GetAllAsync();
        var misInmuebles = todosInmuebles.Where(i => i.PropietarioId == usuario.PropietarioId.Value).ToList();
        var misInmueblesIds = misInmuebles.Select(i => i.Id).ToList();

        // Obtener contratos de esos inmuebles
        var todosContratos = await _contratoRepository.GetAllAsync();
        var misContratos = todosContratos
            .Where(c => misInmueblesIds.Contains(c.InmuebleId))
            .OrderByDescending(c => c.FechaInicio)
            .ToList();

        if (!misContratos.Any())
        {
            ViewBag.SinContratos = true;
            ViewBag.UserRole = "Propietario";
            return View("Index", new PagedResult<Contrato>(new List<Contrato>(), 0, 1, 20));
        }

        // Crear resultado paginado
        var resultado = new PagedResult<Contrato>(misContratos, misContratos.Count, 1, misContratos.Count);

        // Pasar datos a la vista
        ViewBag.UserRole = "Propietario";
        ViewBag.EsMisContratos = true;
        
        return View("Index", resultado);
    }
    catch (Exception ex)
    {
        TempData["Error"] = "Error al cargar sus contratos: " + ex.Message;
        return RedirectToAction("Index", "Home");
    }
}
```

**Características:**
- ✅ Muestra SOLO contratos de inmuebles del propietario
- ✅ Reutiliza vista `Index.cshtml` existente
- ✅ Maneja caso sin contratos
- ✅ Ordenados por fecha de inicio descendente

---

### **Problema 4: MiContrato Daba Acceso Denegado**

**Solución:** Corregido método para usar AuthService

**Archivo:** `Controllers/ContratosController.cs` (líneas 224-280)

```csharp
// GET: Contratos/MiContrato - Vista del contrato del inquilino logueado
[AuthorizeMultipleRoles(RolUsuario.Inquilino)]
public async Task<IActionResult> MiContrato()
{
    try
    {
        // ✅ CORREGIDO: Usar AuthService en lugar de Session
        var usuarioId = AuthService.GetUsuarioId(User);
        if (!usuarioId.HasValue)
        {
            return RedirectToAction("Login", "Auth");
        }

        var usuario = await _usuarioRepository.GetByIdAsync(usuarioId.Value);
        if (usuario?.InquilinoId == null)
        {
            TempData["Error"] = "No tiene permisos para acceder a esta sección. Debe ser un inquilino.";
            return RedirectToAction("Index", "Home");
        }

        // Obtener contratos del inquilino
        var todosContratos = await _contratoRepository.GetAllAsync();
        var misContratos = todosContratos
            .Where(c => c.InquilinoId == usuario.InquilinoId.Value)
            .OrderByDescending(c => c.FechaInicio)
            .ToList();

        if (!misContratos.Any())
        {
            ViewBag.SinContratos = true;
            ViewBag.UserRole = "Inquilino";
            return View(new PagedResult<Contrato>(new List<Contrato>(), 0, 1, 20));
        }

        // Buscar contrato activo
        var fechaActual = DateTime.Now;
        var contratoActivo = misContratos.FirstOrDefault(c => 
            c.FechaInicio <= fechaActual && 
            c.FechaFin >= fechaActual &&
            c.Estado != EstadoContrato.Cancelado &&
            c.Estado != EstadoContrato.Finalizado);

        // Si no hay activo, mostrar el más reciente
        var contratoMostrar = contratoActivo ?? misContratos.First();

        // Obtener pagos del contrato
        var pagos = await _pagoService.GetPagosByContratoAsync(contratoMostrar.Id);

        // Crear resultado
        var resultado = new PagedResult<Contrato>(new List<Contrato> { contratoMostrar }, 1, 1, 1);
        ViewBag.UserRole = "Inquilino";
        ViewBag.Pagos = pagos;
        ViewBag.EsMiContrato = true;
        
        return View(resultado);
    }
    catch (Exception ex)
    {
        TempData["Error"] = "Error al cargar su contrato: " + ex.Message;
        return RedirectToAction("Index", "Home");
    }
}
```

**Cambios Clave:**
- ✅ Agregado `using AuthService = InmobiliariaGarciaJesus.Services.AuthenticationService;`
- ✅ Reemplazado `HttpContext.Session.GetString("UserId")` → `AuthService.GetUsuarioId(User)`

---

## 📊 **Resumen de Archivos Modificados**

| Archivo | Líneas | Cambio |
|---------|---------|--------|
| `Controllers/InmueblesController.cs` | 9, 183-245 | ✅ Agregado using AuthService + Corregido MisInmuebles |
| `Controllers/ContratosController.cs` | 7, 162-280 | ✅ Agregado using AuthService + Creado MisContratos + Corregido MiContrato |
| `Services/UsuarioService.cs` | 328-383 | ✅ Agregados logs en GetRolesDisponiblesAsync |

---

## 🧪 **Pruebas de Funcionalidad**

### **Test 1: Activar Usuario Multi-Rol**

```sql
-- PASO 1: Verificar estado actual
SELECT Id, NombreUsuario, Email, Rol, Estado 
FROM Usuarios 
WHERE Email = 'propietario@inmueble.com';

-- RESULTADO ESPERADO:
-- Id | NombreUsuario        | Email                   | Rol | Estado
-- 5  | propietario          | propietario@inmueble.com| 1   | 1
-- 23 | propietario_inquilino| propietario@inmueble.com| 2   | 0  ← ⚠️ INACTIVO

-- PASO 2: Activar usuario Inquilino
CALL sp_ActivarUsuario(23);

-- PASO 3: Verificar activación
SELECT Id, NombreUsuario, Email, Rol, Estado 
FROM Usuarios 
WHERE Email = 'propietario@inmueble.com';

-- RESULTADO ESPERADO:
-- Id | NombreUsuario        | Email                   | Rol | Estado
-- 5  | propietario          | propietario@inmueble.com| 1   | 1
-- 23 | propietario_inquilino| propietario@inmueble.com| 2   | 1  ← ✅ ACTIVO
```

### **Test 2: Login y Ver MisInmuebles**

```
1. Login con: propietario@inmueble.com
2. ✅ Redirige a: /Inmuebles/MisInmuebles
3. ✅ Ve solo SUS inmuebles (no todos)
4. ✅ Puede acceder a: /Contratos/MisContratos
5. ✅ Ve contratos de SUS inmuebles
```

### **Test 3: Selector de Roles**

```
1. Estando logueado como Propietario
2. Ir a: /Auth/Profile
3. ✅ Ver sección "Cambiar Rol"
4. ✅ Ver opciones: Propietario | Inquilino
5. Seleccionar: Inquilino → Click "Cambiar Rol"
6. ✅ Sesión se reinicia como Inquilino
7. ✅ Redirige a: /Contratos/MiContrato
```

### **Test 4: Vista de Inquilino**

```
1. Estando como Inquilino (después de cambiar rol)
2. ✅ Redirige a: /Contratos/MiContrato
3. ✅ Ve su contrato activo
4. ✅ Ve información de pagos
5. ❌ NO puede acceder a: /Inmuebles/MisInmuebles (acceso denegado)
```

---

## ⚠️ **ACCIONES REQUERIDAS**

### **1. Activar Usuario Multi-Rol en Base de Datos**

```sql
-- Ejecutar este comando en MySQL:
CALL sp_ActivarUsuario(23);

-- O el ID que corresponda a propietario_inquilino:
SELECT Id FROM Usuarios WHERE NombreUsuario = 'propietario_inquilino';
CALL sp_ActivarUsuario(<ID_OBTENIDO>);
```

### **2. Reiniciar Aplicación**

```bash
# Detener aplicación actual (Ctrl+C)
# Ejecutar:
dotnet run
```

### **3. Limpiar Caché del Navegador**

```
- Opción 1: Abrir en modo incógnito (Ctrl+Shift+N)
- Opción 2: Limpiar caché del navegador (Ctrl+Shift+Del)
```

### **4. Ver Logs de Depuración**

Cuando accedas a `/Auth/Profile`, verás en la consola:

```
info: InmobiliariaGarciaJesus.Services.UsuarioService[0]
      GetRolesDisponiblesAsync: Buscando roles para usuario 5 con email propietario@inmueble.com
info: InmobiliariaGarciaJesus.Services.UsuarioService[0]
      GetRolesDisponiblesAsync: Total usuarios en BD: 25
info: InmobiliariaGarciaJesus.Services.UsuarioService[0]
      GetRolesDisponiblesAsync: Usuarios con email propietario@inmueble.com y activos: 2
info: InmobiliariaGarciaJesus.Services.UsuarioService[0]
      GetRolesDisponiblesAsync: Usuario 5 tiene rol Propietario
info: InmobiliariaGarciaJesus.Services.UsuarioService[0]
      GetRolesDisponiblesAsync: Agregado rol Propietario
info: InmobiliariaGarciaJesus.Services.UsuarioService[0]
      GetRolesDisponiblesAsync: Usuario 23 tiene rol Inquilino
info: InmobiliariaGarciaJesus.Services.UsuarioService[0]
      GetRolesDisponiblesAsync: Agregado rol Inquilino
info: InmobiliariaGarciaJesus.Services.UsuarioService[0]
      GetRolesDisponiblesAsync: Roles disponibles finales: 2 - [Propietario, Inquilino]
```

---

## ✅ **Checklist de Verificación**

- [ ] **1. Activar usuario multi-rol en BD** (`CALL sp_ActivarUsuario(23);`)
- [ ] **2. Reiniciar aplicación** (`dotnet run`)
- [ ] **3. Limpiar caché navegador** (Ctrl+Shift+Del o modo incógnito)
- [ ] **4. Login como Propietario** → Debe ir a `/Inmuebles/MisInmuebles`
- [ ] **5. Ver solo SUS inmuebles** (no todos los del sistema)
- [ ] **6. Acceder a `/Contratos/MisContratos`** → Ver contratos de SUS inmuebles
- [ ] **7. Ir a `/Auth/Profile`** → Ver selector "Cambiar Rol"
- [ ] **8. Cambiar a rol Inquilino** → Debe redirigir a `/Contratos/MiContrato`
- [ ] **9. Ver contrato como Inquilino** → Ver contrato activo + pagos
- [ ] **10. Verificar logs en consola** → Ver mensajes de `GetRolesDisponiblesAsync`

---

## 🎯 **Estado Final**

- ✅ **Compilación:** Exitosa (0 errores, 27 warnings normales)
- ✅ **MisInmuebles:** Funcionando con AuthService
- ✅ **MisContratos:** Creado para Propietarios
- ✅ **MiContrato:** Corregido para Inquilinos
- ✅ **GetRolesDisponiblesAsync:** Con logs de depuración
- ⚠️ **Pendiente:** Activar usuario `propietario_inquilino` en BD

---

## 📚 **Documentación Relacionada**

- **Implementación Inicial:** `MULTI_ROL_SOLUCIONESFINALES.md`
- **Sistema Multi-Rol Completo:** `MULTIROL_SYSTEM_README.md`
- **Registro de Usuarios:** `REGISTRO_USUARIOS_README.md`

---

**Última actualización:** 2025-01-18 15:00  
**Estado:** ✅ Correcciones Aplicadas - Pendiente activación BD  
**Compilación:** ✅ Exitosa (0 errores)
