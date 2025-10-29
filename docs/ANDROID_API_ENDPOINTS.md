# 📱 APIs Disponibles para Android

## 🌐 Base URL

```
http://TU_IP:5000/api
```

**Ejemplo:** `http://192.168.1.100:5000/api`

---

## 🗺️ 1. API de Provincias y Localidades (Georef)

### **Obtener Provincias**

```http
GET /api/GeorefApi/provincias
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": "02",
      "nombre": "Buenos Aires"
    },
    {
      "id": "74",
      "nombre": "San Luis"
    }
  ],
  "message": null,
  "errors": null
}
```

**Código Android (Retrofit):**
```kotlin
@GET("GeorefApi/provincias")
suspend fun getProvincias(): ApiResponse<List<Provincia>>

data class Provincia(
    val id: String,
    val nombre: String
)
```

---

### **Obtener Localidades por Provincia**

```http
GET /api/GeorefApi/localidades/{provinciaNombre}
```

**Ejemplo:**
```http
GET /api/GeorefApi/localidades/San%20Luis
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": "740007",
      "nombre": "San Luis"
    },
    {
      "id": "740014",
      "nombre": "Villa Mercedes"
    },
    {
      "id": "740021",
      "nombre": "Merlo"
    }
  ],
  "message": null,
  "errors": null
}
```

**Código Android (Retrofit):**
```kotlin
@GET("GeorefApi/localidades/{provincia}")
suspend fun getLocalidades(
    @Path("provincia") provincia: String
): ApiResponse<List<Localidad>>

data class Localidad(
    val id: String,
    val nombre: String
)
```

---

## 🏠 2. API de Tipos de Inmuebles

### **Obtener Todos los Tipos**

```http
GET /api/TiposInmuebleApi
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "nombre": "Casa",
      "descripcion": "Vivienda unifamiliar"
    },
    {
      "id": 2,
      "nombre": "Departamento",
      "descripcion": "Unidad en edificio"
    },
    {
      "id": 3,
      "nombre": "Local",
      "descripcion": "Local comercial"
    },
    {
      "id": 4,
      "nombre": "Terreno",
      "descripcion": "Lote baldío"
    },
    {
      "id": 5,
      "nombre": "Oficina",
      "descripcion": "Espacio de oficina"
    }
  ],
  "message": null,
  "errors": null
}
```

**Código Android (Retrofit):**
```kotlin
@GET("TiposInmuebleApi")
suspend fun getTiposInmueble(): ApiResponse<List<TipoInmueble>>

data class TipoInmueble(
    val id: Int,
    val nombre: String,
    val descripcion: String?
)
```

---

### **Obtener Tipo por ID**

```http
GET /api/TiposInmuebleApi/{id}
```

**Ejemplo:**
```http
GET /api/TiposInmuebleApi/1
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "nombre": "Casa",
    "descripcion": "Vivienda unifamiliar"
  },
  "message": null,
  "errors": null
}
```

---

## 🔐 3. API de Autenticación

### **Login**

```http
POST /api/AuthApi/login
Content-Type: application/json
```

**Request Body:**
```json
{
  "email": "propietario@example.com",
  "password": "password123"
}
```

**Response (Éxito):**
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIs...",
    "usuario": {
      "id": 1,
      "email": "propietario@example.com",
      "nombre": "Juan Pérez",
      "rol": "Propietario"
    }
  },
  "message": "Login exitoso"
}
```

**Código Android:**
```kotlin
@POST("AuthApi/login")
suspend fun login(@Body request: LoginRequest): ApiResponse<LoginResponse>

data class LoginRequest(
    val email: String,
    val password: String
)

data class LoginResponse(
    val token: String,
    val usuario: Usuario
)

data class Usuario(
    val id: Int,
    val email: String,
    val nombre: String,
    val rol: String
)
```

---

## 🏡 4. API de Inmuebles (Propietarios)

### **Obtener Inmuebles del Propietario**

```http
GET /api/InmueblesApi/propietario/mis-inmuebles
Authorization: Bearer {token}
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "direccion": "Av. Illia 123",
      "localidad": "San Luis",
      "provincia": "San Luis",
      "tipoId": 1,
      "tipoNombre": "Casa",
      "uso": 0,
      "precio": 150000.00,
      "ambientes": 3,
      "estado": 1,
      "estadoNombre": "Activo",
      "latitud": -33.3017,
      "longitud": -66.3378,
      "imagenPortadaUrl": "/uploads/inmuebles/1/portada.jpg"
    }
  ]
}
```

---

### **Crear Inmueble**

```http
POST /api/InmueblesApi/propietario
Authorization: Bearer {token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "direccion": "Av. Illia 456",
  "localidad": "San Luis",
  "provincia": "San Luis",
  "tipoId": 1,
  "uso": 0,
  "precio": 180000.00,
  "ambientes": 4,
  "superficie": 150.5,
  "banos": 2,
  "cochera": true,
  "patio": true,
  "piscina": false,
  "mascotas": true,
  "latitud": -33.3017,
  "longitud": -66.3378
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 10,
    "direccion": "Av. Illia 456",
    "estado": 1
  },
  "message": "Inmueble creado exitosamente"
}
```

---

### **Actualizar Estado del Inmueble**

```http
PATCH /api/InmueblesApi/propietario/{id}/estado
Authorization: Bearer {token}
Content-Type: application/json
```

**Request Body (opción 1 - string):**
```json
{
  "estado": "Activo"
}
```

**Request Body (opción 2 - boolean):**
```json
{
  "activo": true
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 5,
    "estado": 1,
    "estadoNombre": "Activo"
  },
  "message": "Estado del inmueble actualizado a Activo"
}
```

---

## 📋 Enums y Constantes

### **UsoInmueble**
```kotlin
enum class UsoInmueble(val value: Int) {
    RESIDENCIAL(0),
    COMERCIAL(1),
    INDUSTRIAL(2)
}
```

### **EstadoInmueble**
```kotlin
enum class EstadoInmueble(val value: Int) {
    ACTIVO(1),
    INACTIVO(2),
    NO_DISPONIBLE(3),
    RESERVADO(4)
}
```

---

## 🔧 Configuración de Retrofit (Android)

### **1. Crear ApiService**

```kotlin
interface InmobiliariaApiService {
    // Georef
    @GET("GeorefApi/provincias")
    suspend fun getProvincias(): ApiResponse<List<Provincia>>
    
    @GET("GeorefApi/localidades/{provincia}")
    suspend fun getLocalidades(
        @Path("provincia") provincia: String
    ): ApiResponse<List<Localidad>>
    
    // Tipos de Inmuebles
    @GET("TiposInmuebleApi")
    suspend fun getTiposInmueble(): ApiResponse<List<TipoInmueble>>
    
    // Auth
    @POST("AuthApi/login")
    suspend fun login(@Body request: LoginRequest): ApiResponse<LoginResponse>
    
    // Inmuebles
    @GET("InmueblesApi/propietario/mis-inmuebles")
    suspend fun getMisInmuebles(
        @Header("Authorization") token: String
    ): ApiResponse<List<Inmueble>>
    
    @POST("InmueblesApi/propietario")
    suspend fun crearInmueble(
        @Header("Authorization") token: String,
        @Body inmueble: InmuebleCreateRequest
    ): ApiResponse<InmuebleResponse>
    
    @PATCH("InmueblesApi/propietario/{id}/estado")
    suspend fun actualizarEstadoInmueble(
        @Path("id") id: Int,
        @Header("Authorization") token: String,
        @Body request: ActualizarEstadoRequest
    ): ApiResponse<InmuebleResponse>
}
```

---

### **2. Crear Retrofit Instance**

```kotlin
object RetrofitClient {
    private const val BASE_URL = "http://192.168.1.100:5000/api/"
    
    private val okHttpClient = OkHttpClient.Builder()
        .connectTimeout(30, TimeUnit.SECONDS)
        .readTimeout(30, TimeUnit.SECONDS)
        .writeTimeout(30, TimeUnit.SECONDS)
        .addInterceptor(HttpLoggingInterceptor().apply {
            level = HttpLoggingInterceptor.Level.BODY
        })
        .build()
    
    val api: InmobiliariaApiService by lazy {
        Retrofit.Builder()
            .baseUrl(BASE_URL)
            .client(okHttpClient)
            .addConverterFactory(GsonConverterFactory.create())
            .build()
            .create(InmobiliariaApiService::class.java)
    }
}
```

---

### **3. Modelos de Respuesta**

```kotlin
data class ApiResponse<T>(
    val success: Boolean,
    val data: T?,
    val message: String?,
    val errors: List<String>?
)
```

---

## 🧪 Testing con Postman

### **Test 1: Obtener Provincias**
```bash
curl http://192.168.1.100:5000/api/GeorefApi/provincias
```

### **Test 2: Obtener Localidades**
```bash
curl "http://192.168.1.100:5000/api/GeorefApi/localidades/San%20Luis"
```

### **Test 3: Obtener Tipos de Inmuebles**
```bash
curl http://192.168.1.100:5000/api/TiposInmuebleApi
```

### **Test 4: Login**
```bash
curl -X POST http://192.168.1.100:5000/api/AuthApi/login \
  -H "Content-Type: application/json" \
  -d '{"email":"propietario@example.com","password":"password123"}'
```

---

## ✅ Resumen de Endpoints Disponibles

| Endpoint | Método | Descripción | Auth |
|----------|--------|-------------|------|
| `/api/GeorefApi/provincias` | GET | Lista de provincias | ❌ No |
| `/api/GeorefApi/localidades/{provincia}` | GET | Localidades por provincia | ❌ No |
| `/api/TiposInmuebleApi` | GET | Tipos de inmuebles | ❌ No |
| `/api/TiposInmuebleApi/{id}` | GET | Tipo específico | ❌ No |
| `/api/AuthApi/login` | POST | Autenticación | ❌ No |
| `/api/InmueblesApi/propietario/mis-inmuebles` | GET | Inmuebles del propietario | ✅ Sí |
| `/api/InmueblesApi/propietario` | POST | Crear inmueble | ✅ Sí |
| `/api/InmueblesApi/propietario/{id}/estado` | PATCH | Cambiar estado | ✅ Sí |

---

## 🎯 Siguiente Paso para Android

### **Reemplazar Spinners Hardcodeados:**

**ANTES (hardcoded):**
```kotlin
val tipos = arrayOf("Casa", "Departamento", "Local", "Terreno", "Oficina")
```

**AHORA (desde API):**
```kotlin
lifecycleScope.launch {
    try {
        val response = RetrofitClient.api.getTiposInmueble()
        if (response.success) {
            val tipos = response.data ?: emptyList()
            // Llenar spinner con tipos.map { it.nombre }
        }
    } catch (e: Exception) {
        // Fallback a hardcoded si falla
    }
}
```

**Para Provincias y Localidades:** Usar los endpoints de Georef igual que antes, pero ahora desde tu servidor local con fallback a BD.

---

## 🚀 **Todo Listo para Consumir desde Android!**

1. ✅ **Provincias y Localidades**: `/api/GeorefApi/*` (con fallback a BD)
2. ✅ **Tipos de Inmuebles**: `/api/TiposInmuebleApi`
3. ✅ **Autenticación**: `/api/AuthApi/login`
4. ✅ **CRUD Inmuebles**: `/api/InmueblesApi/propietario/*`

**Reemplaza la BASE_URL en tu app Android y empieza a consumir! 🎉**
