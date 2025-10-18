# Sistema de Registro de Usuarios - InmobiliariaGarciaJesus

## 📋 Descripción General

Sistema mejorado de registro de usuarios (Propietarios/Inquilinos) con validación de administrador y reutilización inteligente de datos de personas.

## 🎯 Objetivos

1. **Prevenir duplicación de datos**: Reutilizar personas existentes sin usuario asociado
2. **Validación administrativa**: Todos los registros requieren aprobación de administrador
3. **Seguridad mejorada**: Estado inactivo por defecto hasta validación
4. **UX clara**: Mensajes específicos sobre el estado de validación

---

## 🔄 Flujo de Registro

### **Usuario Nuevo se Registra**

```
1. Usuario completa formulario de registro
   ├─ Tipo de Usuario (Propietario/Inquilino)
   ├─ Datos de Usuario (Email, Usuario, Contraseña)
   └─ Datos Personales (Nombre, Apellido, DNI, Teléfono)

2. Sistema verifica DNI en base de datos
   │
   ├─ DNI NO existe
   │  ├─ Crear nueva persona (Estado = INACTIVO)
   │  └─ Crear nuevo usuario (Estado = INACTIVO)
   │
   └─ DNI existe
      ├─ Tiene usuario asociado → RECHAZAR registro
      │  └─ Mensaje: "Ya existe un usuario con este DNI. Use recuperación de contraseña."
      │
      └─ NO tiene usuario → REUTILIZAR persona
         └─ Crear usuario vinculado al ID existente (Estado = INACTIVO)

3. Redirección a Login con mensaje:
   "Registro exitoso. Su cuenta será activada por un administrador"
```

### **Usuario Intenta Iniciar Sesión**

```
1. Usuario ingresa credenciales

2. Sistema valida:
   ├─ Email existe? NO → "Credenciales inválidas"
   ├─ Contraseña correcta? NO → "Credenciales inválidas"
   ├─ Usuario.Estado = INACTIVO? SÍ → "Cuenta pendiente de validación"
   ├─ Persona.Estado = INACTIVO? SÍ → "Credenciales inválidas"
   └─ Todo OK → Login exitoso
```

### **Administrador Valida Usuario**

```
1. Ver usuarios pendientes:
   - Consultar tabla Usuarios WHERE Estado = 0
   - Ver datos completos de la persona asociada

2. Verificar identidad:
   - Validar DNI y documentación
   - Confirmar datos personales correctos
   - Verificar que no sea duplicado

3. Activar usuario:
   CALL sp_ActivarUsuario(usuario_id);
   
   Esto activa:
   - Usuario.Estado = 1 (Activo)
   - Persona.Estado = 1 (Activo)

4. Usuario puede iniciar sesión normalmente
```

---

## 🛠️ Implementación Técnica

### **Backend - UsuarioService.cs**

#### **Métodos Clave**

1. **`RegisterUsuarioAsync(RegisterViewModel model)`**
   - Valida email y username únicos
   - Llama a `GetOrCreatePropietarioAsync()` o `GetOrCreateInquilinoAsync()`
   - Crea usuario con `Estado = false`
   - Retorna mensaje de validación pendiente

2. **`GetOrCreatePropietarioAsync(RegisterViewModel model)`**
   ```csharp
   - Busca propietario por DNI usando PropietarioRepository.GetByDniAsync()
   - Si existe:
     - Verifica si tiene usuario con UsuarioRepository.GetUsuariosByPersonaAsync()
     - Con usuario → Lanza excepción "DNI ya tiene usuario registrado"
     - Sin usuario → Retorna ID existente (REUTILIZA)
   - Si NO existe:
     - Crea nuevo propietario con Estado = false
     - Retorna nuevo ID
   ```

3. **`GetOrCreateInquilinoAsync(RegisterViewModel model)`**
   - Misma lógica que Propietarios

4. **`AuthenticateAsync(string email, string password)`**
   ```csharp
   - Valida email existe
   - Valida contraseña correcta
   - Valida Usuario.Estado = true (mensaje específico si inactivo)
   - Valida Persona.Estado = true
   - Retorna usuario autenticado
   ```

### **Repositorios - Nuevos Métodos**

#### **PropietarioRepository.cs**
```csharp
public async Task<Propietario?> GetByDniAsync(string dni)
```
- Busca propietario por DNI exacto
- Retorna `null` si no existe

#### **InquilinoRepository.cs**
```csharp
public async Task<Inquilino?> GetByDniAsync(string dni)
```
- Busca inquilino por DNI exacto
- Retorna `null` si no existe

#### **UsuarioRepository.cs**
```csharp
public async Task<List<Usuario>> GetUsuariosByPersonaAsync(int personaId, RolUsuario tipoPersona)
```
- Busca todos los usuarios vinculados a una persona específica
- Filtra por tipo de rol (Propietario/Inquilino)
- Usado para detectar si persona ya tiene usuario

### **Frontend - Register.cshtml**

#### **Cambios Realizados**

1. **Alert Informativo**
   ```html
   <div class="alert alert-info">
     Su cuenta será creada con estado Inactivo y deberá ser validada 
     por un administrador antes de poder iniciar sesión.
   </div>
   ```

2. **Validaciones Simplificadas**
   - Mantiene validación de Email/Username disponible
   - NO valida DNI (backend maneja lógica compleja)
   - Mensajes de error claros del backend

### **Frontend - register.js**

#### **Validaciones**

1. **Email**: Verifica disponibilidad en tiempo real
2. **Username**: Verifica disponibilidad en tiempo real
3. **DNI**: NO se valida en frontend (lógica compleja en backend)

---

## 📊 Base de Datos

### **Tablas Afectadas**

#### **Usuarios**
```sql
Estado TINYINT(1) DEFAULT 0  -- 0 = Inactivo (pendiente), 1 = Activo
```

#### **Propietarios**
```sql
Estado TINYINT(1) DEFAULT 0  -- 0 = Inactivo, 1 = Activo
Dni VARCHAR(20) UNIQUE       -- Índice para búsqueda rápida
```

#### **Inquilinos**
```sql
Estado TINYINT(1) DEFAULT 0  -- 0 = Inactivo, 1 = Activo
Dni VARCHAR(20) UNIQUE       -- Índice para búsqueda rápida
```

### **Procedimiento Almacenado**

```sql
CALL sp_ActivarUsuario(usuario_id);
```

**Funcionalidad:**
- Activa `Usuarios.Estado = 1`
- Activa `Propietarios/Inquilinos.Estado = 1` según corresponda
- Transacción atómica (todo o nada)
- Manejo de errores robusto

### **Consultas Útiles**

#### Ver usuarios pendientes
```sql
SELECT 
    u.Id, u.NombreUsuario, u.Email, u.Rol,
    CONCAT(p.Nombre, ' ', p.Apellido) AS NombreCompleto,
    p.Dni, u.FechaCreacion
FROM Usuarios u
LEFT JOIN Propietarios p ON u.PropietarioId = p.Id
LEFT JOIN Inquilinos i ON u.InquilinoId = i.Id
WHERE u.Estado = 0
ORDER BY u.FechaCreacion DESC;
```

#### Activar usuario manualmente (alternativa a SP)
```sql
UPDATE Usuarios SET Estado = 1 WHERE Id = {ID};
UPDATE Propietarios SET Estado = 1 WHERE Id = (SELECT PropietarioId FROM Usuarios WHERE Id = {ID});
```

---

## 🔐 Casos de Uso

### **Caso 1: Usuario Completamente Nuevo**

**Contexto:** Juan Pérez nunca se registró en el sistema

```
1. Juan completa registro como Propietario
   DNI: 12345678
   Email: juan@example.com

2. Sistema verifica:
   ✓ Email disponible
   ✓ DNI no existe en Propietarios

3. Sistema crea:
   - Propietario (Id: 100, DNI: 12345678, Estado: 0)
   - Usuario (Id: 50, PropietarioId: 100, Estado: 0)

4. Resultado:
   ✅ Registro exitoso
   ⏳ Pendiente de validación
```

### **Caso 2: Persona Existe, Sin Usuario**

**Contexto:** María García fue registrada por un empleado pero nunca tuvo usuario

```
1. Base de datos actual:
   Propietarios: (Id: 200, DNI: 87654321, Estado: 1)
   Usuarios: Ninguno con PropietarioId = 200

2. María completa registro como Propietario
   DNI: 87654321
   Email: maria@example.com

3. Sistema verifica:
   ✓ Email disponible
   ✓ DNI existe en Propietarios (Id: 200)
   ✓ Propietario NO tiene usuario asociado

4. Sistema crea:
   - Usuario (Id: 51, PropietarioId: 200, Estado: 0)
   - NO crea nuevo propietario (REUTILIZA Id: 200)

5. Resultado:
   ✅ Registro exitoso
   ♻️ Datos existentes reutilizados
   ⏳ Pendiente de validación
```

### **Caso 3: DNI Ya Tiene Usuario**

**Contexto:** Carlos López ya tiene usuario activo

```
1. Base de datos actual:
   Propietarios: (Id: 300, DNI: 11223344, Estado: 1)
   Usuarios: (Id: 60, PropietarioId: 300, Estado: 1)

2. Alguien intenta registrarse con DNI: 11223344

3. Sistema verifica:
   ✓ DNI existe en Propietarios (Id: 300)
   ✗ Propietario YA tiene usuario (Id: 60)

4. Resultado:
   ❌ Registro rechazado
   💬 Mensaje: "Ya existe un usuario registrado como Propietario con el DNI 11223344"
```

### **Caso 4: Persona Puede Tener Múltiples Roles**

**Contexto:** Ana Martínez es Propietaria y también quiere ser Inquilina

```
1. Base de datos actual:
   Propietarios: (Id: 400, DNI: 99887766, Estado: 1)
   Usuarios: (Id: 70, PropietarioId: 400, Rol: Propietario, Estado: 1)

2. Ana intenta registrarse como Inquilina con mismo DNI

3. Sistema verifica:
   ✓ Email disponible (diferente al anterior)
   ✓ DNI no existe en Inquilinos (diferente tabla)
   ✓ DNI existe en Propietarios pero con rol diferente

4. Sistema crea:
   - Inquilino (Id: 500, DNI: 99887766, Estado: 0)
   - Usuario (Id: 71, InquilinoId: 500, Rol: Inquilino, Estado: 0)

5. Resultado:
   ✅ Registro exitoso
   👥 Ana ahora tiene 2 usuarios con roles diferentes
   ⏳ Nuevo rol pendiente de validación
```

---

## ✅ Testing Manual

### **Test 1: Registro Nuevo Usuario**
1. Ir a `/Auth/Register`
2. Seleccionar tipo "Propietario"
3. Completar todos los campos con datos únicos
4. Enviar formulario
5. **Esperado:** Redirect a `/Auth/Login` con mensaje "Registro exitoso. Su cuenta será activada por un administrador"

### **Test 2: Intento de Login con Usuario Inactivo**
1. Registrar usuario (estado inactivo)
2. Ir a `/Auth/Login`
3. Ingresar credenciales correctas
4. **Esperado:** Error "Su cuenta está pendiente de validación por un administrador"

### **Test 3: Activación de Usuario**
1. Como administrador, ejecutar:
   ```sql
   CALL sp_ActivarUsuario(usuario_id);
   ```
2. Verificar en tabla Usuarios: `Estado = 1`
3. Verificar en tabla Propietarios/Inquilinos: `Estado = 1`
4. Intentar login nuevamente
5. **Esperado:** Login exitoso

### **Test 4: Reutilización de Persona**
1. Crear propietario manualmente en BD sin usuario
2. Registrarse con el mismo DNI
3. **Esperado:** No error de duplicación, usa ID existente

### **Test 5: DNI Duplicado con Usuario**
1. Registrar usuario completo (con DNI X)
2. Intentar registrar otro usuario con mismo DNI
3. **Esperado:** Error "Ya existe un usuario registrado con el DNI X"

---

## 🚀 Deployment

### **Pasos de Migración**

1. **Backup de Base de Datos**
   ```bash
   mysqldump -u root -p inmobiliaria > backup_before_migration.sql
   ```

2. **Ejecutar Migración SQL**
   ```bash
   mysql -u root -p inmobiliaria < update_registro_usuarios_validacion.sql
   ```

3. **Verificar Procedimiento Almacenado**
   ```sql
   SHOW PROCEDURE STATUS WHERE Name = 'sp_ActivarUsuario';
   ```

4. **Compilar y Publicar Aplicación**
   ```bash
   dotnet build --configuration Release
   dotnet publish --configuration Release
   ```

5. **Verificar Funcionalidad**
   - Crear usuario de prueba
   - Verificar estado inactivo
   - Activar con procedimiento
   - Verificar login exitoso

---

## 📝 Notas para Administradores

### **Validación de Usuarios Pendientes**

#### **Proceso Recomendado:**

1. **Consultar pendientes**
   ```sql
   SELECT * FROM vw_usuarios_pendientes;  -- Vista creada en migración
   ```

2. **Verificar identidad**
   - Solicitar documentación (DNI escaneado)
   - Verificar datos coinciden con registro
   - Confirmar legitimidad (llamada telefónica, etc.)

3. **Activar usuario**
   ```sql
   CALL sp_ActivarUsuario(123);  -- Reemplazar 123 con ID real
   ```

4. **Notificar usuario** (opcional - futuro)
   - Email de bienvenida
   - Instrucciones de acceso
   - Link directo a login

### **Rechazar Registros Sospechosos**

Si detecta registro fraudulento:

```sql
-- NO activar usuario, simplemente eliminarlo (soft delete)
UPDATE Usuarios SET Estado = 0 WHERE Id = {ID};

-- Opcional: Agregar observación
UPDATE Usuarios SET Observaciones = 'Registro rechazado: datos sospechosos' WHERE Id = {ID};
```

---

## 🔧 Troubleshooting

### **Error: "Ya existe un usuario con este email"**
**Causa:** Email ya registrado en tabla Usuarios  
**Solución:** Usuario debe usar recuperación de contraseña o registrarse con otro email

### **Error: "Ya existe un usuario registrado con el DNI X"**
**Causa:** Persona con ese DNI ya tiene usuario asociado  
**Solución:** Usuario debe usar recuperación de contraseña

### **Usuario no puede iniciar sesión después de activación**
**Verificar:**
```sql
SELECT u.Estado AS UsuarioEstado, p.Estado AS PropietarioEstado
FROM Usuarios u
LEFT JOIN Propietarios p ON u.PropietarioId = p.Id
WHERE u.Email = 'email@example.com';
```
Ambos estados deben ser `1`

### **Procedimiento sp_ActivarUsuario no funciona**
**Verificar existencia:**
```sql
SHOW CREATE PROCEDURE sp_ActivarUsuario;
```
Si no existe, ejecutar nuevamente el script de migración

---

## 📚 Referencias

- **Código Backend:** `Services/UsuarioService.cs`
- **Repositorios:** `Repositories/PropietarioRepository.cs`, `InquilinoRepository.cs`, `UsuarioRepository.cs`
- **Vistas:** `Views/Auth/Register.cshtml`
- **JavaScript:** `wwwroot/js/Auth/register.js`
- **Migración SQL:** `Database/Migrations/update_registro_usuarios_validacion.sql`
- **Controlador:** `Controllers/AuthController.cs`

---

## 📞 Contacto y Soporte

Para dudas o problemas con el sistema de registro:
1. Revisar logs de aplicación: `Logs/app-{date}.log`
2. Verificar estado de base de datos
3. Consultar esta documentación
4. Contactar al equipo de desarrollo

---

**Última actualización:** 2025-01-18  
**Versión:** 1.0  
**Rama:** `feature/registro-usuarios-mejoras`
