# Sistema Multi-Rol - InmobiliariaGarciaJesus

## 📋 Descripción

Sistema híbrido que permite a usuarios existentes agregar roles adicionales (ej: Propietario puede registrarse también como Inquilino) validando su contraseña actual y reutilizando sus datos personales.

---

## ✅ ¿Qué se implementó?

### **Flujo de Usuario:**

#### **Caso 1: Usuario Nuevo (Sin cuenta existente)**
```
1. Accede a /Auth/Register
2. Selecciona rol (Propietario/Inquilino)
3. Ingresa email
4. Sistema valida: "Email disponible para este rol" ✅
5. Completa formulario normalmente
6. Registra → Estado INACTIVO (validación de admin)
```

#### **Caso 2: Usuario Existente (Agregar nuevo rol)**
```
1. Usuario "propietario@mail.com" ya registrado como Propietario
2. Accede a /Auth/Register
3. Selecciona rol: Inquilino
4. Ingresa email: propietario@mail.com
5. Sistema detecta: "Este email ya está registrado" ⚠️
6. Aparece campo: "Ingrese su contraseña actual"
7. Usuario ingresa contraseña
8. Click en "Validar y Continuar"
9. Sistema valida contraseña ✅
10. Auto-completa: Nombre, Apellido, DNI, Telefono (readonly)
11. Auto-sugiere username: "propietario_inquilino"
12. Oculta campos de contraseña (usará la existente)
13. Usuario confirma registro
14. Sistema crea:
    - Nuevo Inquilino (copiando datos de Propietario)
    - Nuevo Usuario (mismo email, nuevo username, mismo hash de contraseña)
    - Ambos con Estado = INACTIVO
```

---

## 🔧 Cambios Implementados

### **Backend**

#### **UsuarioRepository.cs**
```csharp
// NUEVO: Validar email + rol (permite mismo email con roles diferentes)
public async Task<bool> EmailExistsWithRoleAsync(string email, RolUsuario rol, int? excludeId = null)
```

#### **UsuarioService.cs**
```csharp
// NUEVO: Validar cuenta existente con contraseña
public async Task<(bool Exists, Usuario? Usuario, string? ErrorMessage)> 
    ValidateExistingEmailForMultiRoleAsync(string email, RolUsuario newRole, string password)

// NUEVO: Registrar usuario multi-rol copiando datos
public async Task<(bool Success, string Message, int? UsuarioId)> 
    RegisterMultiRoleUsuarioAsync(RegisterViewModel model, string existingPassword)
```

#### **AuthController.cs**
```csharp
// MODIFICADO: Detecta modo multi-rol
public async Task<IActionResult> Register(RegisterViewModel model, string? existingPassword, bool isMultiRole = false)

// MODIFICADO: Valida email + rol
public async Task<IActionResult> CheckEmailAvailability(string email, int? rol)

// NUEVO: Endpoint AJAX para validar cuenta existente
public async Task<IActionResult> ValidateExistingEmailForMultiRole(string email, int rol, string password)
```

### **Frontend**

#### **Register.cshtml**
- Agregada sección oculta `#existingAccountValidation` que aparece cuando email existe
- Campo para ingresar contraseña de cuenta existente
- Botón "Validar y Continuar"

#### **register.js**
```javascript
// Detecta email existente y muestra validación
bindEmailValidation() // Modificado

// Valida contraseña y obtiene datos
bindExistingAccountValidation() // NUEVO

// Auto-completa formulario con datos existentes
autofillExistingUserData(data) // NUEVO
```

### **Base de Datos**

#### **Migración SQL:** `update_usuarios_multirol_email.sql`
```sql
-- Elimina restricción UNIQUE de Email
DROP INDEX Email ON Usuarios;

-- Agrega restricción UNIQUE compuesta (Email + Rol)
ALTER TABLE Usuarios ADD UNIQUE INDEX UK_Usuarios_Email_Rol (Email, Rol);
```

---

## 🚀 Pasos para Deployment

### **1. Ejecutar Migración SQL** ⚠️ CRÍTICO

```bash
# Conectar a MySQL
mysql -u root -p inmobiliaria

# Ejecutar migración
source Database/Migrations/update_usuarios_multirol_email.sql
```

**O ejecutar manualmente:**
```sql
USE inmobiliaria;

-- Verificar índices actuales
SHOW INDEX FROM Usuarios WHERE Key_name LIKE '%Email%';

-- Eliminar índice UNIQUE de Email
DROP INDEX Email ON Usuarios;

-- Agregar índice UNIQUE compuesto
ALTER TABLE Usuarios ADD UNIQUE INDEX UK_Usuarios_Email_Rol (Email, Rol);

-- Verificar cambios
SHOW INDEX FROM Usuarios;
```

### **2. Compilar y Ejecutar Aplicación**

```bash
dotnet build
dotnet run
```

### **3. Probar el Flujo**

#### **Test 1: Registro Normal**
1. Ir a `/Auth/Register`
2. Email nuevo → Debe funcionar normalmente

#### **Test 2: Multi-Rol**
1. Login con usuario existente (ej: propietario@inmueble.com)
2. Logout
3. Ir a `/Auth/Register`
4. Seleccionar ROL DIFERENTE (Inquilino)
5. Ingresar MISMO EMAIL (propietario@inmueble.com)
6. Sistema muestra campo de validación
7. Ingresar contraseña correcta
8. Validar → Datos se auto-completan
9. Cambiar username sugerido si es necesario
10. Registrar
11. ✅ Debe crear usuario INACTIVO sin error de duplicación

#### **Test 3: Activación por Admin**
```sql
-- Ver usuarios pendientes
SELECT * FROM Usuarios WHERE Estado = 0 ORDER BY FechaCreacion DESC;

-- Activar nuevo rol
CALL sp_ActivarUsuario(nuevo_usuario_id);

-- Verificar activación
SELECT u.Email, u.Rol, u.Estado, p.Estado AS PropietarioEstado, i.Estado AS InquilinoEstado
FROM Usuarios u
LEFT JOIN Propietarios p ON u.PropietarioId = p.Id
LEFT JOIN Inquilinos i ON u.InquilinoId = i.Id
WHERE u.Email = 'propietario@inmueble.com';
```

---

## 📊 Casos de Uso Resueltos

### **Antes (❌ No funcionaba):**
```
Usuario: propietario@inmueble.com, Rol: Propietario
Intenta registrarse como Inquilino con mismo email
→ Error: "Email ya registrado" ❌
→ No puede tener múltiples roles
```

### **Ahora (✅ Funciona):**
```
Usuario: propietario@inmueble.com, Rol: Propietario (ID: 5)
Se registra como Inquilino:
  1. Ingresa mismo email
  2. Valida con contraseña actual
  3. Sistema copia datos de Propietario a Inquilino
  4. Crea nuevo usuario:
     - Email: propietario@inmueble.com
     - Username: propietario_inquilino (diferente)
     - Password: (mismo hash)
     - Rol: Inquilino
     - InquilinoId: 8 (nuevo)
  5. ✅ Ahora tiene 2 cuentas con mismo email pero roles diferentes
```

### **Login con Multi-Rol:**
```
Futuro: Login mostrará selector de rol si email tiene múltiples usuarios
Por ahora: Usa el primer usuario encontrado
```

---

## ⚠️ Restricciones y Validaciones

### **Restricciones de BD:**
- ✅ `UNIQUE (Email, Rol)` → Mismo email con roles diferentes: OK
- ✅ `UNIQUE (NombreUsuario)` → Username debe ser único globalmente
- ✅ Triggers de validación FK por rol siguen funcionando

### **Validaciones de Negocio:**
1. **No puede registrar mismo email + mismo rol:**
   ```
   propietario@mail.com + Propietario ✅ (existe)
   propietario@mail.com + Propietario ❌ (duplicado)
   ```

2. **Username debe ser único:**
   ```
   propietario ✅ (existe como Propietario)
   propietario_inquilino ✅ (nuevo username para Inquilino)
   propietario ❌ (no puede repetir)
   ```

3. **Contraseña debe validarse:**
   - Si email existe, DEBE ingresar contraseña correcta
   - Si contraseña incorrecta → No permite continuar

4. **Estado INACTIVO por defecto:**
   - Nuevo usuario: INACTIVO
   - Nueva persona (Propietario/Inquilino): INACTIVA
   - Requiere activación de administrador

---

## 🔐 Seguridad

### **Validación de Contraseña:**
```csharp
// Backend valida contraseña antes de permitir multi-rol
if (!VerifyPassword(password, existingUser.ClaveHash))
{
    return (false, null, "Contraseña incorrecta");
}
```

### **Protección contra Ataques:**
- ✅ Validación de contraseña en backend (no solo frontend)
- ✅ Username único global previene suplantación
- ✅ Estado INACTIVO previene acceso inmediato
- ✅ Logs de auditoría para registro multi-rol

---

## 📝 Notas Técnicas

### **Copia de Datos:**
```csharp
// Ejemplo: Propietario → Inquilino
if (existingUser.Propietario != null)
{
    var inquilino = new Inquilino
    {
        Nombre = existingUser.Propietario.Nombre,
        Apellido = existingUser.Propietario.Apellido,
        Dni = existingUser.Propietario.Dni,  // Mismo DNI, diferentes tablas
        Telefono = existingUser.Propietario.Telefono,
        Email = existingUser.Email,
        Estado = false
    };
    personaId = await _inquilinoRepository.CreateAsync(inquilino);
}
```

### **Reutilización de Hash:**
```csharp
// Usa la misma contraseña hasheada
nuevoUsuario.ClaveHash = existingUser.ClaveHash;
```

### **Auto-sugerencia de Username:**
```javascript
// JavaScript sugiere username único
suggestedUsername: `${existingUsername}_${newRole.toLowerCase()}`
// Ejemplo: "propietario" → "propietario_inquilino"
```

---

## 🐛 Troubleshooting

### **Error: "Duplicate entry for key 'Email'"**
**Causa:** Migración SQL no ejecutada  
**Solución:** Ejecutar `update_usuarios_multirol_email.sql`

### **Campo de validación no aparece**
**Causa:** JavaScript no cargado o email no existe en BD  
**Solución:** F5 para refrescar, verificar console.log()

### **"Contraseña incorrecta" pero es correcta**
**Causa:** Usuario existente tiene contraseña diferente  
**Solución:** Verificar que sea el usuario correcto, intentar recuperación de contraseña

### **Datos no se auto-completan**
**Causa:** AJAX fallando o usuario sin persona asociada  
**Solución:** Ver console.log() del browser, verificar que usuario tenga Propietario/Inquilino/Empleado

---

## 📚 Referencias

- **Código Backend:** `Services/UsuarioService.cs` (líneas 522-644)
- **Controlador:** `Controllers/AuthController.cs` (líneas 334-394)
- **Vista:** `Views/Auth/Register.cshtml` (líneas 81-98)
- **JavaScript:** `wwwroot/js/Auth/register.js` (líneas 133-224)
- **Migración SQL:** `Database/Migrations/update_usuarios_multirol_email.sql`
- **Documentación anterior:** `REGISTRO_USUARIOS_README.md`

---

## ✅ Checklist de Deployment

- [ ] Ejecutar migración SQL
- [ ] Compilar aplicación sin errores
- [ ] Probar registro normal (email nuevo)
- [ ] Probar registro multi-rol (email existente)
- [ ] Verificar auto-completado de datos
- [ ] Verificar que username debe ser diferente
- [ ] Verificar que usuario queda INACTIVO
- [ ] Probar activación con sp_ActivarUsuario
- [ ] Verificar login exitoso después de activación
- [ ] Documentar proceso para usuarios finales

---

**Última actualización:** 2025-01-18  
**Versión:** 1.0  
**Rama:** `feature/multirol-system`
