# GUION PARA VIDEO DEMOSTRACIÓN - API ANDROID INMOBILIARIA

**Aplicación:** InmobiliariaGarciaJesus - API REST para Android  
**Herramienta:** Postman  
**Fecha:** Noviembre 2025  
**Base URL:** `http://localhost:5000/api` o `https://tudominio.com/api`

---

## ⚠️ ASPECTOS IMPORTANTES DE SEGURIDAD

✅ **NUNCA se envía el ID del propietario en las peticiones**  
✅ El `propietarioId` se recupera SIEMPRE del token JWT  
✅ Todas las consultas validan que los datos pertenezcan al propietario autenticado  
✅ Todas las funcionalidades requieren autenticación **EXCEPTO** Login  

---

## 📋 ÍNDICE DEL VIDEO

1. [Login (Sin autenticación)](#1-login)
2. [Ver Perfil (Con autenticación)](#2-ver-perfil)
3. [Editar Perfil (Con autenticación)](#3-editar-perfil)
4. [Cambiar Contraseña (Con autenticación)](#4-cambiar-contraseña)
5. [Listar Inmuebles (Con autenticación)](#5-listar-inmuebles)
6. [Habilitar/Deshabilitar Inmueble (Con autenticación)](#6-habilitardeshabilitar-inmueble)
7. [Crear Inmueble con Foto (Con autenticación)](#7-crear-inmueble-con-foto)
8. [Listar Contratos y Pagos (Con autenticación)](#8-listar-contratos-y-pagos)

---

## 🎬 PREPARACIÓN DEL ENTORNO

### Antes de Empezar el Video

1. **Iniciar el servidor:**
   - Abrir Visual Studio 2022
   - Abrir solución `InmobiliariaGarciaJesus.sln`
   - Ejecutar con IIS Express o Kestrel
   - Verificar que esté corriendo en `http://localhost:5000` o el puerto configurado

2. **Abrir Postman:**
   - Crear nueva colección llamada "API Inmobiliaria Android"
   - Configurar variable de entorno `baseUrl` = `http://localhost:5000/api`

3. **Tener datos de prueba:**
   - Email de propietario de prueba: `propietario1@test.com`
   - Contraseña: `password123`
   - Tener al menos 1 inmueble y 1 contrato activo en la BD

---

## 1. LOGIN

### 🎯 Objetivo
Demostrar la autenticación de un propietario y obtención del token JWT.

### 📝 Guion Verbal
> "Comenzamos con el endpoint de Login. Este es el ÚNICO endpoint que NO requiere autenticación previa. Vamos a autenticar un propietario para obtener el token JWT que usaremos en todas las siguientes peticiones."

### 🔧 Configuración en Postman

**Request:**
```
POST {{baseUrl}}/AuthApi/login
Content-Type: application/json
```

**Body (JSON):**
```json
{
  "email": "propietario1@test.com",
  "password": "password123"
}
```

### 📹 Pasos para el Video

1. **Mostrar Postman:**
   - Crear nuevo request llamado "1. Login"
   - Método: POST
   - URL: `{{baseUrl}}/AuthApi/login`
   - Headers: `Content-Type: application/json`

2. **Mostrar el Body:**
   - Seleccionar tab "Body"
   - Seleccionar "raw" y "JSON"
   - Pegar el JSON con email y password
   - **IMPORTANTE:** Mencionar que NO se envía ID del propietario

3. **Ejecutar Request:**
   - Click en "Send"
   - Mostrar response exitoso (Status 200 OK)

4. **Analizar Response:**
```json
{
  "success": true,
  "message": "Login exitoso",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "propietario": {
      "id": 1,
      "nombre": "Juan",
      "apellido": "García",
      "nombreCompleto": "Juan García",
      "dni": "12345678",
      "telefono": "2664123456",
      "email": "propietario1@test.com",
      "estado": true,
      "fotoPerfil": "/uploads/perfiles/foto.jpg"
    },
    "expiracion": "2025-11-04T21:00:00Z"
  }
}
```

5. **Guardar el Token:**
   - Copiar el token desde el response
   - En Postman, ir a la colección "API Inmobiliaria Android"
   - En tab "Variables", crear variable `authToken`
   - Pegar el token como valor
   - **MENCIONAR:** "Este token contiene el ID del propietario encriptado"

6. **Mostrar el Código (Visual Studio):**
   - Abrir `Controllers/Api/AuthApiController.cs`
   - Ir a línea 44: método `Login`
   - **EXPLICAR:**
     - Línea 58: Busca usuario por email (NO por ID)
     - Línea 69: Verifica que sea un propietario
     - Línea 76: Verifica la contraseña con BCrypt
     - Línea 94: **GENERA EL TOKEN** con `_jwtService.GenerarToken(usuario, usuario.Propietario)`
     - **PUNTO CLAVE:** El token contiene el `propietarioId` encriptado

### ✅ Verificación
- Status 200 OK
- Token JWT recibido
- Datos del propietario correctos
- Token guardado en variable de Postman

---

## 2. VER PERFIL

### 🎯 Objetivo
Obtener los datos del perfil del propietario autenticado usando el token.

### 📝 Guion Verbal
> "Ahora que tenemos el token, podemos acceder a los endpoints protegidos. Vamos a obtener el perfil del propietario. Observen que NO enviamos el ID del propietario, el servidor lo extrae automáticamente del token JWT."

### 🔧 Configuración en Postman

**Request:**
```
GET {{baseUrl}}/PropietarioApi/perfil
Authorization: Bearer {{authToken}}
```

### 📹 Pasos para el Video

1. **Crear Request:**
   - Nuevo request: "2. Ver Perfil"
   - Método: GET
   - URL: `{{baseUrl}}/PropietarioApi/perfil`

2. **Configurar Autenticación:**
   - Tab "Authorization"
   - Type: "Bearer Token"
   - Token: `{{authToken}}`
   - **MENCIONAR:** "El token se envía automáticamente en el header Authorization"

3. **Ejecutar Request:**
   - Click en "Send"
   - Status 200 OK

4. **Analizar Response:**
```json
{
  "success": true,
  "message": null,
  "data": {
    "id": 1,
    "nombre": "Juan",
    "apellido": "García",
    "nombreCompleto": "Juan García",
    "dni": "12345678",
    "telefono": "2664123456",
    "email": "propietario1@test.com",
    "direccion": "Av. Illia 123",
    "estado": true,
    "fotoPerfil": "/uploads/perfiles/foto.jpg"
  }
}
```

5. **Mostrar el Código:**
   - Abrir `Controllers/Api/PropietarioApiController.cs`
   - Línea 48: método `ObtenerPerfil()`
   - **EXPLICAR:**
     - Línea 52: `var propietarioId = _jwtService.ObtenerPropietarioId(User);`
     - **PUNTO CLAVE:** El ID se extrae del token, NO del request
     - Línea 58: Busca propietario por el ID extraído del token
     - La consulta valida que los datos sean del propietario autenticado

### ✅ Verificación
- Status 200 OK
- Datos del perfil recibidos
- NO se envió ID en la petición

---

## 3. EDITAR PERFIL

### 🎯 Objetivo
Actualizar datos del perfil (nombre, apellido, teléfono, dirección).

### 📝 Guion Verbal
> "Ahora vamos a editar el perfil. Nuevamente, NO enviamos el ID del propietario. El servidor sabe qué perfil actualizar porque extrae el ID del token."

### 🔧 Configuración en Postman

**Request:**
```
PUT {{baseUrl}}/PropietarioApi/perfil
Authorization: Bearer {{authToken}}
Content-Type: application/json
```

**Body (JSON):**
```json
{
  "nombre": "Juan Carlos",
  "apellido": "García Pérez",
  "telefono": "2664987654",
  "direccion": "Av. Illia 456, San Luis"
}
```

### 📹 Pasos para el Video

1. **Crear Request:**
   - Nuevo request: "3. Editar Perfil"
   - Método: PUT
   - URL: `{{baseUrl}}/PropietarioApi/perfil`
   - Authorization: Bearer Token `{{authToken}}`

2. **Configurar Body:**
   - Tab "Body" → "raw" → "JSON"
   - Pegar JSON con datos actualizados
   - **RESALTAR:** "No incluimos el ID del propietario"

3. **Ejecutar Request:**
   - Click en "Send"
   - Status 200 OK

4. **Analizar Response:**
```json
{
  "success": true,
  "message": "Perfil actualizado exitosamente",
  "data": {
    "id": 1,
    "nombre": "Juan Carlos",
    "apellido": "García Pérez",
    "nombreCompleto": "Juan Carlos García Pérez",
    "telefono": "2664987654",
    "direccion": "Av. Illia 456, San Luis",
    ...
  }
}
```

5. **Mostrar el Código:**
   - Línea 101: método `ActualizarPerfil`
   - Línea 114: `var propietarioId = _jwtService.ObtenerPropietarioId(User);`
   - Línea 120-126: Busca y valida que el propietario existe
   - Líneas 129-132: Actualiza SOLO los campos permitidos
   - **SEGURIDAD:** NO se puede modificar Email ni DNI desde la app

### ✅ Verificación
- Status 200 OK
- Datos actualizados correctamente
- Response incluye datos nuevos

---

## 4. CAMBIAR CONTRASEÑA

### 🎯 Objetivo
Cambiar la contraseña del propietario autenticado.

### 📝 Guion Verbal
> "Para cambiar la contraseña, el propietario debe proporcionar su contraseña actual y la nueva. El sistema valida la contraseña actual antes de permitir el cambio."

### 🔧 Configuración en Postman

**Request:**
```
PUT {{baseUrl}}/Propietarios/changePassword
Authorization: Bearer {{authToken}}
Content-Type: application/x-www-form-urlencoded
```

**Body (form-urlencoded):**
```
currentPassword: password123
newPassword: nuevoPassword456
```

### 📹 Pasos para el Video

1. **Crear Request:**
   - Nuevo request: "4. Cambiar Contraseña"
   - Método: PUT
   - URL: `{{baseUrl}}/Propietarios/changePassword`
   - Authorization: Bearer Token `{{authToken}}`

2. **Configurar Body:**
   - Tab "Body" → "x-www-form-urlencoded"
   - Key: `currentPassword`, Value: `password123`
   - Key: `newPassword`, Value: `nuevoPassword456`
   - **MENCIONAR:** "Formato form-urlencoded para compatibilidad Android"

3. **Ejecutar Request:**
   - Click en "Send"
   - Status 200 OK (respuesta vacía)

4. **Mostrar el Código:**
   - Línea 257: método `ChangePassword`
   - Línea 283: `var usuarioId = _jwtService.ObtenerUsuarioId(User);`
   - Línea 296: Verifica contraseña actual con BCrypt
   - Línea 302: Hash de la nueva contraseña con BCrypt
   - **SEGURIDAD:** Contraseñas nunca se guardan en texto plano

### ✅ Verificación
- Status 200 OK
- Respuesta vacía (solo status)
- Contraseña cambiada exitosamente

---

## 5. LISTAR INMUEBLES

### 🎯 Objetivo
Obtener todos los inmuebles del propietario autenticado.

### 📝 Guion Verbal
> "Vamos a listar todos los inmuebles. El sistema automáticamente filtra SOLO los inmuebles que pertenecen al propietario autenticado. No es posible ver inmuebles de otros propietarios."

### 🔧 Configuración en Postman

**Request:**
```
GET {{baseUrl}}/InmueblesApi
Authorization: Bearer {{authToken}}
```

### 📹 Pasos para el Video

1. **Crear Request:**
   - Nuevo request: "5. Listar Inmuebles"
   - Método: GET
   - URL: `{{baseUrl}}/InmueblesApi`
   - Authorization: Bearer Token `{{authToken}}`

2. **Ejecutar Request:**
   - Click en "Send"
   - Status 200 OK

3. **Analizar Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "direccion": "Calle Falsa 123",
      "localidad": "San Luis",
      "provincia": "San Luis",
      "tipoId": 1,
      "tipoNombre": "Casa",
      "ambientes": 3,
      "superficie": 120.5,
      "latitud": -33.3017,
      "longitud": -66.3378,
      "estado": "Activo",
      "disponibilidad": "No Disponible",
      "precio": 250000.00,
      "uso": "Residencial",
      "imagenPortadaUrl": "http://localhost:5000/uploads/inmuebles/1/foto.jpg",
      "imagenes": [...]
    }
  ]
}
```

4. **Mostrar el Código:**
   - Línea 44: método `ListarInmuebles()`
   - Línea 48: `var propietarioId = _jwtService.ObtenerPropietarioId(User);`
   - Línea 54-58: Query con filtro `Where(i => i.PropietarioId == propietarioId.Value)`
   - **SEGURIDAD:** Solo retorna inmuebles del propietario autenticado
   - Líneas 61-65: Carga relaciones (Tipo, Imágenes, Contratos)

### ✅ Verificación
- Status 200 OK
- Lista de inmuebles del propietario
- Incluye imágenes y estado de disponibilidad

---

## 6. HABILITAR/DESHABILITAR INMUEBLE

### 🎯 Objetivo
Cambiar el estado de un inmueble entre Activo e Inactivo.

### 📝 Guion Verbal
> "El propietario puede habilitar o deshabilitar sus inmuebles. El sistema valida que el inmueble pertenezca al propietario y que no tenga contratos activos antes de permitir la deshabilitación."

### 🔧 Configuración en Postman

**Request:**
```
PATCH {{baseUrl}}/InmueblesApi/1/estado
Authorization: Bearer {{authToken}}
Content-Type: application/json
```

**Body (JSON) - Opción 1:**
```json
{
  "estado": "Activo"
}
```

**Body (JSON) - Opción 2:**
```json
{
  "activo": true
}
```

### 📹 Pasos para el Video

1. **Crear Request:**
   - Nuevo request: "6. Habilitar/Deshabilitar Inmueble"
   - Método: PATCH
   - URL: `{{baseUrl}}/InmueblesApi/1/estado` (reemplazar 1 con ID real)
   - Authorization: Bearer Token `{{authToken}}`

2. **Configurar Body:**
   - Tab "Body" → "raw" → "JSON"
   - Opción 1: `{"estado": "Inactivo"}`
   - **MENCIONAR:** "Acepta formato string o boolean"

3. **Ejecutar Request:**
   - Click en "Send"
   - Status 200 OK

4. **Analizar Response:**
```json
{
  "success": true,
  "message": "Estado del inmueble actualizado a Inactivo",
  "data": {
    "id": 1,
    "direccion": "Calle Falsa 123",
    "estado": "Inactivo",
    ...
  }
}
```

5. **Mostrar el Código:**
   - Línea 204: método `ActualizarEstado`
   - Línea 217: Extrae propietarioId del token
   - Línea 233: Valida que el inmueble pertenezca al propietario
   - Líneas 252-265: **VALIDACIÓN:** Verifica que no haya contratos activos
   - Línea 268-269: Actualiza estado

### ✅ Verificación
- Status 200 OK
- Estado actualizado correctamente
- Validación de propiedad funcionando

---

## 7. CREAR INMUEBLE CON FOTO

### 🎯 Objetivo
Crear un nuevo inmueble con una foto en base64 (por defecto deshabilitado).

### 📝 Guion Verbal
> "Los propietarios pueden agregar nuevos inmuebles desde la app móvil. La foto se envía en formato base64. Por seguridad, todos los inmuebles se crean DESHABILITADOS hasta que un administrador los revise y active."

### 🔧 Configuración en Postman

**Request:**
```
POST {{baseUrl}}/InmueblesApi
Authorization: Bearer {{authToken}}
Content-Type: application/json
```

**Body (JSON):**
```json
{
  "direccion": "Av. Libertador 789",
  "localidad": "San Luis",
  "provincia": "San Luis",
  "tipoId": 2,
  "ambientes": 2,
  "superficie": 75.5,
  "latitud": -33.3017,
  "longitud": -66.3378,
  "precio": 180000.00,
  "uso": 0,
  "imagenBase64": "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==",
  "imagenNombre": "foto_inmueble.jpg"
}
```

### 📹 Pasos para el Video

1. **Preparar imagen base64:**
   - Usar una herramienta online o comando: 
   - `[Convert]::ToBase64String([IO.File]::ReadAllBytes("foto.jpg"))`
   - Copiar string base64 (sin prefijo data:image)

2. **Crear Request:**
   - Nuevo request: "7. Crear Inmueble con Foto"
   - Método: POST
   - URL: `{{baseUrl}}/InmueblesApi`
   - Authorization: Bearer Token `{{authToken}}`

3. **Configurar Body:**
   - Tab "Body" → "raw" → "JSON"
   - Pegar JSON completo
   - **RESALTAR:** "NO enviamos propietarioId, se toma del token"
   - **RESALTAR:** "Uso: 0=Residencial, 1=Comercial, 2=Industrial"

4. **Ejecutar Request:**
   - Click en "Send"
   - Status 201 Created

5. **Analizar Response:**
```json
{
  "success": true,
  "message": "Inmueble creado exitosamente (estado: inactivo)",
  "data": {
    "id": 5,
    "direccion": "Av. Libertador 789",
    "estado": "Inactivo",
    "imagenPortadaUrl": "http://localhost:5000/uploads/inmuebles/5/guid.jpg",
    ...
  }
}
```

6. **Mostrar el Código:**
   - Línea 133: método `CrearInmueble`
   - Línea 146: Extrae propietarioId del token
   - Líneas 153-168: Crea objeto Inmueble con PropietarioId del token
   - Línea 166: **Estado = Inactivo por defecto**
   - Líneas 174-177: Procesa y guarda imagen base64
   - Línea 343: método `GuardarImagenInmueble` decodifica base64

### ✅ Verificación
- Status 201 Created
- Inmueble creado con estado Inactivo
- Foto guardada correctamente
- PropietarioId asignado del token

---

## 8. LISTAR CONTRATOS Y PAGOS

### 🎯 Objetivo
Obtener contratos activos de los inmuebles del propietario con sus pagos.

### 📝 Guion Verbal
> "Finalmente, vamos a consultar los contratos activos y sus pagos. El sistema solo retorna contratos de inmuebles que pertenecen al propietario autenticado. Cada contrato incluye su listado completo de pagos."

### 🔧 Configuración en Postman

**Request 1 - Todos los contratos:**
```
GET {{baseUrl}}/ContratosApi
Authorization: Bearer {{authToken}}
```

**Request 2 - Contratos de un inmueble específico:**
```
GET {{baseUrl}}/ContratosApi/inmueble/1
Authorization: Bearer {{authToken}}
```

### 📹 Pasos para el Video

#### Parte A: Todos los Contratos

1. **Crear Request:**
   - Nuevo request: "8a. Listar Todos los Contratos"
   - Método: GET
   - URL: `{{baseUrl}}/ContratosApi`
   - Authorization: Bearer Token `{{authToken}}`

2. **Ejecutar Request:**
   - Click en "Send"
   - Status 200 OK

3. **Analizar Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "fechaInicio": "2025-01-01",
      "fechaFin": "2025-12-31",
      "precio": 250000.00,
      "estado": "Activo",
      "inmueble": {
        "id": 1,
        "direccion": "Calle Falsa 123",
        "localidad": "San Luis",
        "provincia": "San Luis",
        "ambientes": 3,
        "imagenPortadaUrl": "/uploads/..."
      },
      "inquilino": {
        "id": 1,
        "nombreCompleto": "María López",
        "dni": "87654321",
        "telefono": "2664111222",
        "email": "maria@test.com"
      },
      "pagos": [
        {
          "id": 1,
          "numero": 1,
          "importe": 250000.00,
          "intereses": 0.00,
          "multas": 0.00,
          "totalAPagar": 250000.00,
          "fechaVencimiento": "2025-01-10",
          "fechaPago": null,
          "estado": "Pendiente",
          "metodoPago": null,
          "observaciones": null
        },
        {
          "id": 2,
          "numero": 2,
          "importe": 250000.00,
          "estado": "Pagado",
          "fechaPago": "2025-02-08",
          "metodoPago": "Transferencia"
        }
      ]
    }
  ]
}
```

4. **Mostrar el Código:**
   - Línea 156: método `ListarTodosLosContratos()`
   - Línea 160: Extrae propietarioId del token
   - Líneas 167-170: Obtiene IDs de inmuebles del propietario
   - Línea 177: **Filtro:** `Where(c => inmueblesIds.Contains(c.InmuebleId) && c.Estado == EstadoContrato.Activo)`
   - **SEGURIDAD:** Solo contratos activos de inmuebles del propietario
   - Líneas 186-189: Carga pagos de cada contrato

#### Parte B: Contratos de un Inmueble

1. **Crear Request:**
   - Nuevo request: "8b. Contratos por Inmueble"
   - Método: GET
   - URL: `{{baseUrl}}/ContratosApi/inmueble/1`
   - Authorization: Bearer Token `{{authToken}}`

2. **Ejecutar Request:**
   - Click en "Send"
   - Status 200 OK

3. **Mostrar el Código:**
   - Línea 43: método `ListarContratosPorInmueble`
   - Línea 54-65: Valida que el inmueble pertenezca al propietario
   - Línea 72: Filtro por inmuebleId y estado Activo
   - Líneas 81-84: Carga pagos de cada contrato

### ✅ Verificación
- Status 200 OK
- Solo contratos activos retornados
- Incluye datos completos: inmueble, inquilino, pagos
- Validación de propiedad funcionando

---

## 🎬 CIERRE DEL VIDEO

### 📝 Guion Final
> "Hemos demostrado todas las funcionalidades del API para la aplicación móvil Android:
>
> 1. ✅ **Login** - Único endpoint sin autenticación
> 2. ✅ **Ver y editar perfil** - Con extracción automática del ID del token
> 3. ✅ **Cambiar contraseña** - Con validación de contraseña actual
> 4. ✅ **Listar inmuebles** - Solo los del propietario autenticado
> 5. ✅ **Habilitar/Deshabilitar inmuebles** - Con validaciones de seguridad
> 6. ✅ **Crear inmueble con foto** - Estado inactivo por defecto
> 7. ✅ **Listar contratos y pagos** - Solo contratos activos de inmuebles propios
>
> **Puntos clave de seguridad:**
> - El ID del propietario NUNCA se envía en las peticiones
> - Se extrae automáticamente del token JWT en cada endpoint
> - Todas las consultas validan que los datos pertenezcan al propietario autenticado
> - Solo Login permite acceso sin autenticación
>
> Gracias por ver esta demostración."

---

## 📚 RECURSOS ADICIONALES

### Variables de Postman
```
baseUrl: http://localhost:5000/api
authToken: (se obtiene del login)
```

### Endpoints Summary
| Método | Endpoint | Autenticación | Descripción |
|--------|----------|---------------|-------------|
| POST | /AuthApi/login | ❌ No | Login y obtención de token |
| GET | /PropietarioApi/perfil | ✅ Sí | Ver perfil |
| PUT | /PropietarioApi/perfil | ✅ Sí | Editar perfil |
| PUT | /Propietarios/changePassword | ✅ Sí | Cambiar contraseña |
| GET | /InmueblesApi | ✅ Sí | Listar inmuebles |
| PATCH | /InmueblesApi/{id}/estado | ✅ Sí | Habilitar/Deshabilitar |
| POST | /InmueblesApi | ✅ Sí | Crear inmueble |
| GET | /ContratosApi | ✅ Sí | Listar todos los contratos |
| GET | /ContratosApi/inmueble/{id} | ✅ Sí | Contratos por inmueble |

### Archivos de Código a Mostrar
- `Controllers/Api/AuthApiController.cs` - Login y autenticación
- `Controllers/Api/PropietarioApiController.cs` - Gestión de perfil
- `Controllers/Api/InmueblesApiController.cs` - Gestión de inmuebles
- `Controllers/Api/ContratosApiController.cs` - Gestión de contratos
- `Services/JwtService.cs` - Extracción de ID del token

---

## ⏱️ TIMING ESTIMADO

- Introducción y preparación: 2 min
- Login: 5 min
- Ver perfil: 3 min
- Editar perfil: 4 min
- Cambiar contraseña: 4 min
- Listar inmuebles: 4 min
- Habilitar/Deshabilitar: 5 min
- Crear inmueble: 6 min
- Listar contratos: 5 min
- Cierre: 2 min

**Total estimado: 40 minutos**

---

## 🎥 TIPS PARA LA GRABACIÓN

1. **Antes de grabar:**
   - Reiniciar el servidor para logs limpios
   - Limpiar consola de Postman
   - Preparar todos los requests con anticipación
   - Probar cada endpoint una vez

2. **Durante la grabación:**
   - Hablar claro y pausado
   - Mostrar código antes de ejecutar cada request
   - Resaltar líneas clave con el cursor
   - Pausar después de cada response para analizar

3. **Herramientas recomendadas:**
   - OBS Studio para grabar pantalla
   - Zoom nivel 125% en Visual Studio y Postman
   - Tema claro para mejor visibilidad
   - Ocultar información sensible (tokens reales, IPs públicas)

4. **Edición:**
   - Acelerar partes repetitivas
   - Agregar subtítulos en puntos clave
   - Destacar con flechas/círculos el ID que NO se envía
   - Mostrar comparación antes/después en edición de perfil

---

**FIN DEL GUION**
