# 🧪 Test: Actualizar Estado de Inmueble

## 📋 Problema Reportado
El endpoint `PATCH /api/InmueblesApi/{id}/estado` no está actualizando el campo `estado` en la base de datos.

---

## 🔍 Verificación en Base de Datos

### **1. Ver Estado Actual del Inmueble**
```sql
SELECT 
    Id,
    Direccion,
    Estado,
    CASE 
        WHEN Estado = 1 THEN 'Activo'
        WHEN Estado = 0 THEN 'Inactivo'
        ELSE 'Desconocido'
    END AS EstadoTexto,
    PropietarioId,
    FechaCreacion
FROM inmuebles
WHERE Id = 5;  -- Reemplazar con el ID que quieres probar
```

**Resultado Esperado:**
```
Id | Direccion        | Estado | EstadoTexto | PropietarioId
5  | Pueyrredon 859   | 1      | Activo      | 1
```

---

## 🧪 Test en Postman

### **Paso 1: Login**
```http
POST http://10.226.44.156:5000/api/AuthApi/login
Content-Type: application/json

{
  "email": "jose.perez@email.com",
  "password": "tu_password"
}
```

**Copiar el token de la respuesta.**

---

### **Paso 2: Inactivar Inmueble**
```http
PATCH http://10.226.44.156:5000/api/InmueblesApi/5/estado
Authorization: Bearer {token}
Content-Type: application/json

{
  "activo": false
}
```

**Respuesta Esperada (200 OK):**
```json
{
  "success": true,
  "message": "Estado del inmueble actualizado a Inactivo",
  "data": {
    "id": 5,
    "direccion": "Pueyrredon 859",
    "estado": "Inactivo",
    "disponibilidad": "Disponible"
  }
}
```

**Respuesta Error si hay Contrato (400 Bad Request):**
```json
{
  "success": false,
  "message": "No se puede inactivar el inmueble. El inmueble está Reservado con un contrato vigente."
}
```

---

### **Paso 3: Verificar Cambio en BD**
```sql
SELECT 
    Id,
    Direccion,
    Estado,
    CASE 
        WHEN Estado = 1 THEN 'Activo'
        WHEN Estado = 0 THEN 'Inactivo'
        ELSE 'Desconocido'
    END AS EstadoTexto
FROM inmuebles
WHERE Id = 5;
```

**Debe mostrar:**
```
Estado = 0 (Inactivo)
```

---

### **Paso 4: Activar Nuevamente**
```http
PATCH http://10.226.44.156:5000/api/InmueblesApi/5/estado
Authorization: Bearer {token}
Content-Type: application/json

{
  "activo": true
}
```

**Verificar en BD:**
```
Estado = 1 (Activo)
```

---

## 🐛 Debugging si No Funciona

### **1. Verificar Logs del Servidor**
Buscar en la consola:
```
info: InmobiliariaGarciaJesus.Controllers.Api.InmueblesApiController[0]
      Estado de inmueble ID: 5 actualizado a Inactivo por propietario ID: 1
```

### **2. Verificar Conversión de Estado**
```sql
-- Ver valor RAW en la BD
SELECT Id, Direccion, Estado, HEX(Estado) AS EstadoHex
FROM inmuebles
WHERE Id = 5;
```

### **3. Ver si Hay Contratos Bloqueando**
```sql
SELECT 
    c.Id AS ContratoId,
    c.InmuebleId,
    c.Estado AS EstadoContrato,
    c.FechaInicio,
    c.FechaFin
FROM contratos c
WHERE c.InmuebleId = 5
  AND c.Estado IN ('Activo', 'Reservado');
```

Si hay resultados, el endpoint rechazará la inactivación.

---

## ✅ Casos de Prueba

| Caso | Activo (Request) | Tiene Contrato | Resultado Esperado |
|------|------------------|----------------|-------------------|
| 1    | `true` | No | ✅ Estado=1 (Activo) |
| 2    | `false` | No | ✅ Estado=0 (Inactivo) |
| 3    | `false` | Sí (Activo) | ❌ Error 400 |
| 4    | `false` | Sí (Reservado) | ❌ Error 400 |
| 5    | `true` | Sí (Activo) | ✅ Estado=1 (Permitido) |

---

## 🔧 Modelo C# del Request

```csharp
public class ActualizarEstadoInmuebleDto
{
    [Required(ErrorMessage = "El estado es obligatorio")]
    public bool Activo { get; set; }
}
```

**Importante:** El campo JSON debe llamarse `"activo"` (minúscula) por convención de JSON.

---

## 📱 Modelo Kotlin para Android

```kotlin
// Request
data class ActualizarEstadoRequest(
    val activo: Boolean
)

// Uso en Retrofit
@PATCH("InmueblesApi/{id}/estado")
suspend fun actualizarEstado(
    @Path("id") id: Int,
    @Body request: ActualizarEstadoRequest
): ApiResponse<InmuebleDto>
```

---

## 🎯 Soluciones a Problemas Comunes

### **Problema 1: Campo JSON Incorrecto**
❌ Incorrecto:
```json
{
  "disponible": false,  // ← Campo antiguo
  "estado": false
}
```

✅ Correcto:
```json
{
  "activo": false  // ← Campo correcto
}
```

### **Problema 2: Token Expirado**
```json
{
  "success": false,
  "message": "No autorizado"
}
```
**Solución:** Hacer login nuevamente para obtener un token válido.

### **Problema 3: No Guarda en BD**
- Verificar que el servidor se haya reiniciado después de los cambios
- Revisar logs por errores de EF Core
- Verificar conexión a la base de datos

---

## 📊 Mapeo de Estado

| BD (MySQL) | C# Enum | JSON API |
|------------|---------|----------|
| `1` | `EstadoInmueble.Activo` | `"Activo"` |
| `0` | `EstadoInmueble.Inactivo` | `"Inactivo"` |

**Conversión en DbContext:**
```csharp
modelBuilder.Entity<Inmueble>()
    .Property(i => i.Estado)
    .HasConversion(
        v => v == EstadoInmueble.Activo ? 1 : 0,  // C# → BD
        v => v == 1 ? EstadoInmueble.Activo : EstadoInmueble.Inactivo  // BD → C#
    );
```
