# 📱 Configuración de Retrofit para Android - Inmobiliaria API

Guía completa para integrar la API REST de la Inmobiliaria en tu aplicación Android usando Retrofit + Kotlin.

---

## 📦 Dependencias (build.gradle.kts - Module)

```kotlin
plugins {
    alias(libs.plugins.android.application)
    alias(libs.plugins.kotlin.android)
    id("kotlin-kapt")
}

android {
    // ... configuración existente ...
    
    buildFeatures {
        viewBinding = true
        dataBinding = true
    }
    
    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_17
        targetCompatibility = JavaVersion.VERSION_17
    }
}

dependencies {
    // Retrofit y Gson
    implementation("com.squareup.retrofit2:retrofit:2.9.0")
    implementation("com.squareup.retrofit2:converter-gson:2.9.0")
    implementation("com.squareup.okhttp3:okhttp:4.12.0")
    implementation("com.squareup.okhttp3:logging-interceptor:4.12.0")
    
    // Coroutines para llamadas asíncronas
    implementation("org.jetbrains.kotlinx:kotlinx-coroutines-android:1.7.3")
    implementation("org.jetbrains.kotlinx:kotlinx-coroutines-core:1.7.3")
    
    // ViewModel y LiveData
    implementation("androidx.lifecycle:lifecycle-viewmodel-ktx:2.7.0")
    implementation("androidx.lifecycle:lifecycle-livedata-ktx:2.7.0")
    implementation("androidx.lifecycle:lifecycle-runtime-ktx:2.7.0")
    
    // DataStore para almacenar token JWT
    implementation("androidx.datastore:datastore-preferences:1.0.0")
    
    // Glide para cargar imágenes
    implementation("com.github.bumptech.glide:glide:4.16.0")
    kapt("com.github.bumptech.glide:compiler:4.16.0")
}
```

---

## 🔧 Estructura de Paquetes Recomendada

```
com.inmobiliaria.app/
├── data/
│   ├── api/
│   │   ├── ApiService.kt
│   │   ├── AuthApiService.kt
│   │   ├── InmueblesApiService.kt
│   │   ├── ContratosApiService.kt
│   │   └── PropietarioApiService.kt
│   ├── interceptors/
│   │   └── AuthInterceptor.kt
│   ├── models/
│   │   ├── request/
│   │   │   ├── LoginRequest.kt
│   │   │   └── CambiarPasswordRequest.kt
│   │   └── response/
│   │       ├── ApiResponse.kt
│   │       ├── LoginResponse.kt
│   │       ├── PropietarioDto.kt
│   │       ├── InmuebleDto.kt
│   │       └── ContratoDto.kt
│   └── repository/
│       ├── AuthRepository.kt
│       └── InmuebleRepository.kt
├── ui/
│   ├── auth/
│   ├── home/
│   └── inmuebles/
└── utils/
    ├── Constants.kt
    ├── TokenManager.kt
    └── NetworkResult.kt
```

---

## 🌐 1. Configuración de Constantes

**`utils/Constants.kt`**
```kotlin
package com.inmobiliaria.app.utils

object Constants {
    // Base URL - CAMBIAR según tu IP
    const val BASE_URL = "http://10.226.44.156:5000/api/"
    
    // Endpoints
    object Endpoints {
        const val LOGIN = "AuthApi/login"
        const val LOGIN_FORM = "AuthApi/login-form"
        const val CAMBIAR_PASSWORD = "AuthApi/cambiar-password"
        const val PERFIL = "PropietarioApi/perfil"
        const val INMUEBLES = "InmueblesApi"
        const val CONTRATOS = "ContratosApi"
    }
    
    // SharedPreferences Keys
    const val PREFS_NAME = "InmobiliariaPrefs"
    const val KEY_TOKEN = "jwt_token"
    const val KEY_USER_ID = "user_id"
    const val KEY_USER_EMAIL = "user_email"
    
    // Request Timeout
    const val CONNECT_TIMEOUT = 30L
    const val READ_TIMEOUT = 30L
    const val WRITE_TIMEOUT = 30L
}
```

---

## 🔐 2. Token Manager (DataStore)

**`utils/TokenManager.kt`**
```kotlin
package com.inmobiliaria.app.utils

import android.content.Context
import androidx.datastore.core.DataStore
import androidx.datastore.preferences.core.Preferences
import androidx.datastore.preferences.core.edit
import androidx.datastore.preferences.core.stringPreferencesKey
import androidx.datastore.preferences.preferencesDataStore
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.map

private val Context.dataStore: DataStore<Preferences> by preferencesDataStore(name = "auth_prefs")

class TokenManager(private val context: Context) {
    
    companion object {
        private val TOKEN_KEY = stringPreferencesKey("jwt_token")
        private val USER_EMAIL_KEY = stringPreferencesKey("user_email")
        private val USER_ID_KEY = stringPreferencesKey("user_id")
    }
    
    // Guardar token
    suspend fun saveToken(token: String) {
        context.dataStore.edit { preferences ->
            preferences[TOKEN_KEY] = token
        }
    }
    
    // Obtener token
    fun getToken(): Flow<String?> {
        return context.dataStore.data.map { preferences ->
            preferences[TOKEN_KEY]
        }
    }
    
    // Guardar datos de usuario
    suspend fun saveUserData(email: String, userId: String) {
        context.dataStore.edit { preferences ->
            preferences[USER_EMAIL_KEY] = email
            preferences[USER_ID_KEY] = userId
        }
    }
    
    // Limpiar datos (logout)
    suspend fun clearData() {
        context.dataStore.edit { preferences ->
            preferences.clear()
        }
    }
    
    // Verificar si hay token
    suspend fun hasToken(): Boolean {
        var hasToken = false
        context.dataStore.data.map { preferences ->
            hasToken = !preferences[TOKEN_KEY].isNullOrEmpty()
        }
        return hasToken
    }
}
```

---

## 🔑 3. Interceptor de Autenticación

**`data/interceptors/AuthInterceptor.kt`**
```kotlin
package com.inmobiliaria.app.data.interceptors

import android.content.Context
import com.inmobiliaria.app.utils.TokenManager
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.runBlocking
import okhttp3.Interceptor
import okhttp3.Response

class AuthInterceptor(context: Context) : Interceptor {
    
    private val tokenManager = TokenManager(context)
    
    override fun intercept(chain: Interceptor.Chain): Response {
        val requestBuilder = chain.request().newBuilder()
        
        // Agregar token JWT a todas las peticiones
        val token = runBlocking {
            tokenManager.getToken().first()
        }
        
        if (!token.isNullOrEmpty()) {
            requestBuilder.addHeader("Authorization", "Bearer $token")
        }
        
        // Agregar headers comunes
        requestBuilder.addHeader("Accept", "application/json")
        requestBuilder.addHeader("Content-Type", "application/json")
        
        return chain.proceed(requestBuilder.build())
    }
}
```

---

## 🏗️ 4. Configuración de Retrofit

**`data/api/RetrofitClient.kt`**
```kotlin
package com.inmobiliaria.app.data.api

import android.content.Context
import com.inmobiliaria.app.data.interceptors.AuthInterceptor
import com.inmobiliaria.app.utils.Constants
import okhttp3.OkHttpClient
import okhttp3.logging.HttpLoggingInterceptor
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import java.util.concurrent.TimeUnit

object RetrofitClient {
    
    @Volatile
    private var retrofit: Retrofit? = null
    
    fun getInstance(context: Context): Retrofit {
        return retrofit ?: synchronized(this) {
            retrofit ?: buildRetrofit(context).also { retrofit = it }
        }
    }
    
    private fun buildRetrofit(context: Context): Retrofit {
        val okHttpClient = OkHttpClient.Builder()
            .addInterceptor(AuthInterceptor(context))
            .addInterceptor(loggingInterceptor())
            .connectTimeout(Constants.CONNECT_TIMEOUT, TimeUnit.SECONDS)
            .readTimeout(Constants.READ_TIMEOUT, TimeUnit.SECONDS)
            .writeTimeout(Constants.WRITE_TIMEOUT, TimeUnit.SECONDS)
            .build()
        
        return Retrofit.Builder()
            .baseUrl(Constants.BASE_URL)
            .client(okHttpClient)
            .addConverterFactory(GsonConverterFactory.create())
            .build()
    }
    
    private fun loggingInterceptor(): HttpLoggingInterceptor {
        return HttpLoggingInterceptor().apply {
            level = HttpLoggingInterceptor.Level.BODY
        }
    }
}
```

---

## 📡 5. Definición de Servicios API

### **5.1 AuthApiService.kt**

```kotlin
package com.inmobiliaria.app.data.api

import com.inmobiliaria.app.data.models.request.CambiarPasswordRequest
import com.inmobiliaria.app.data.models.request.LoginRequest
import com.inmobiliaria.app.data.models.response.ApiResponse
import com.inmobiliaria.app.data.models.response.LoginResponse
import retrofit2.Response
import retrofit2.http.Body
import retrofit2.http.POST

interface AuthApiService {
    
    @POST("AuthApi/login")
    suspend fun login(@Body request: LoginRequest): Response<ApiResponse<LoginResponse>>
    
    @POST("AuthApi/cambiar-password")
    suspend fun cambiarPassword(@Body request: CambiarPasswordRequest): Response<ApiResponse<Unit>>
}
```

### **5.2 PropietarioApiService.kt**

```kotlin
package com.inmobiliaria.app.data.api

import com.inmobiliaria.app.data.models.response.ApiResponse
import com.inmobiliaria.app.data.models.response.PropietarioDto
import okhttp3.MultipartBody
import retrofit2.Response
import retrofit2.http.*

interface PropietarioApiService {
    
    @GET("PropietarioApi/perfil")
    suspend fun obtenerPerfil(): Response<ApiResponse<PropietarioDto>>
    
    @Multipart
    @PUT("PropietarioApi/actualizar-foto")
    suspend fun actualizarFoto(@Part foto: MultipartBody.Part): Response<ApiResponse<String>>
}
```

### **5.3 InmueblesApiService.kt**

```kotlin
package com.inmobiliaria.app.data.api

import com.inmobiliaria.app.data.models.response.ApiResponse
import com.inmobiliaria.app.data.models.response.InmuebleDto
import retrofit2.Response
import retrofit2.http.GET
import retrofit2.http.Path

interface InmueblesApiService {
    
    @GET("InmueblesApi")
    suspend fun listarInmuebles(): Response<ApiResponse<List<InmuebleDto>>>
    
    @GET("InmueblesApi/{id}")
    suspend fun obtenerInmueble(@Path("id") id: Int): Response<ApiResponse<InmuebleDto>>
}
```

### **5.4 ContratosApiService.kt**

```kotlin
package com.inmobiliaria.app.data.api

import com.inmobiliaria.app.data.models.response.ApiResponse
import com.inmobiliaria.app.data.models.response.ContratoDto
import retrofit2.Response
import retrofit2.http.GET
import retrofit2.http.Path
import retrofit2.http.Query

interface ContratosApiService {
    
    @GET("ContratosApi")
    suspend fun listarContratos(
        @Query("estado") estado: String? = null
    ): Response<ApiResponse<List<ContratoDto>>>
    
    @GET("ContratosApi/{id}")
    suspend fun obtenerContrato(@Path("id") id: Int): Response<ApiResponse<ContratoDto>>
}
```

---

## 📦 6. Wrapper para Resultados de Red

**`utils/NetworkResult.kt`**
```kotlin
package com.inmobiliaria.app.utils

sealed class NetworkResult<T>(
    val data: T? = null,
    val message: String? = null
) {
    class Success<T>(data: T) : NetworkResult<T>(data)
    class Error<T>(message: String, data: T? = null) : NetworkResult<T>(data, message)
    class Loading<T> : NetworkResult<T>()
}
```

---

## 🗂️ 7. Repository Pattern

**`data/repository/AuthRepository.kt`**
```kotlin
package com.inmobiliaria.app.data.repository

import android.content.Context
import com.inmobiliaria.app.data.api.AuthApiService
import com.inmobiliaria.app.data.api.RetrofitClient
import com.inmobiliaria.app.data.models.request.LoginRequest
import com.inmobiliaria.app.data.models.response.LoginResponse
import com.inmobiliaria.app.utils.NetworkResult
import com.inmobiliaria.app.utils.TokenManager
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext

class AuthRepository(private val context: Context) {
    
    private val api = RetrofitClient.getInstance(context).create(AuthApiService::class.java)
    private val tokenManager = TokenManager(context)
    
    suspend fun login(email: String, password: String): NetworkResult<LoginResponse> {
        return withContext(Dispatchers.IO) {
            try {
                val request = LoginRequest(email, password)
                val response = api.login(request)
                
                if (response.isSuccessful && response.body() != null) {
                    val apiResponse = response.body()!!
                    
                    if (apiResponse.success && apiResponse.data != null) {
                        // Guardar token
                        tokenManager.saveToken(apiResponse.data.token)
                        tokenManager.saveUserData(
                            email = apiResponse.data.propietario.email,
                            userId = apiResponse.data.propietario.id.toString()
                        )
                        
                        NetworkResult.Success(apiResponse.data)
                    } else {
                        NetworkResult.Error(apiResponse.message ?: "Error desconocido")
                    }
                } else {
                    NetworkResult.Error("Error: ${response.code()} - ${response.message()}")
                }
            } catch (e: Exception) {
                NetworkResult.Error("Error de conexión: ${e.localizedMessage}")
            }
        }
    }
    
    suspend fun logout() {
        tokenManager.clearData()
    }
}
```

**`data/repository/InmuebleRepository.kt`**
```kotlin
package com.inmobiliaria.app.data.repository

import android.content.Context
import com.inmobiliaria.app.data.api.InmueblesApiService
import com.inmobiliaria.app.data.api.RetrofitClient
import com.inmobiliaria.app.data.models.response.InmuebleDto
import com.inmobiliaria.app.utils.NetworkResult
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext

class InmuebleRepository(context: Context) {
    
    private val api = RetrofitClient.getInstance(context).create(InmueblesApiService::class.java)
    
    suspend fun obtenerInmuebles(): NetworkResult<List<InmuebleDto>> {
        return withContext(Dispatchers.IO) {
            try {
                val response = api.listarInmuebles()
                
                if (response.isSuccessful && response.body() != null) {
                    val apiResponse = response.body()!!
                    
                    if (apiResponse.success && apiResponse.data != null) {
                        NetworkResult.Success(apiResponse.data)
                    } else {
                        NetworkResult.Error(apiResponse.message ?: "Error al obtener inmuebles")
                    }
                } else {
                    NetworkResult.Error("Error: ${response.code()}")
                }
            } catch (e: Exception) {
                NetworkResult.Error("Error de conexión: ${e.localizedMessage}")
            }
        }
    }
    
    suspend fun obtenerInmueble(id: Int): NetworkResult<InmuebleDto> {
        return withContext(Dispatchers.IO) {
            try {
                val response = api.obtenerInmueble(id)
                
                if (response.isSuccessful && response.body() != null) {
                    val apiResponse = response.body()!!
                    
                    if (apiResponse.success && apiResponse.data != null) {
                        NetworkResult.Success(apiResponse.data)
                    } else {
                        NetworkResult.Error(apiResponse.message ?: "Inmueble no encontrado")
                    }
                } else {
                    NetworkResult.Error("Error: ${response.code()}")
                }
            } catch (e: Exception) {
                NetworkResult.Error("Error de conexión: ${e.localizedMessage}")
            }
        }
    }
}
```

---

## 🎨 8. ViewModel Example

**`ui/auth/LoginViewModel.kt`**
```kotlin
package com.inmobiliaria.app.ui.auth

import android.app.Application
import androidx.lifecycle.AndroidViewModel
import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.viewModelScope
import com.inmobiliaria.app.data.models.response.LoginResponse
import com.inmobiliaria.app.data.repository.AuthRepository
import com.inmobiliaria.app.utils.NetworkResult
import kotlinx.coroutines.launch

class LoginViewModel(application: Application) : AndroidViewModel(application) {
    
    private val repository = AuthRepository(application.applicationContext)
    
    private val _loginResult = MutableLiveData<NetworkResult<LoginResponse>>()
    val loginResult: LiveData<NetworkResult<LoginResponse>> = _loginResult
    
    fun login(email: String, password: String) {
        viewModelScope.launch {
            _loginResult.value = NetworkResult.Loading()
            _loginResult.value = repository.login(email, password)
        }
    }
}
```

---

## 🖥️ 9. Uso en Activity/Fragment

**`ui/auth/LoginActivity.kt`**
```kotlin
package com.inmobiliaria.app.ui.auth

import android.content.Intent
import android.os.Bundle
import android.view.View
import android.widget.Toast
import androidx.activity.viewModels
import androidx.appcompat.app.AppCompatActivity
import com.inmobiliaria.app.databinding.ActivityLoginBinding
import com.inmobiliaria.app.ui.home.MainActivity
import com.inmobiliaria.app.utils.NetworkResult

class LoginActivity : AppCompatActivity() {
    
    private lateinit var binding: ActivityLoginBinding
    private val viewModel: LoginViewModel by viewModels()
    
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        binding = ActivityLoginBinding.inflate(layoutInflater)
        setContentView(binding.root)
        
        setupObservers()
        setupListeners()
    }
    
    private fun setupObservers() {
        viewModel.loginResult.observe(this) { result ->
            when (result) {
                is NetworkResult.Loading -> {
                    showLoading(true)
                }
                is NetworkResult.Success -> {
                    showLoading(false)
                    Toast.makeText(this, "Login exitoso", Toast.LENGTH_SHORT).show()
                    navigateToMain()
                }
                is NetworkResult.Error -> {
                    showLoading(false)
                    Toast.makeText(this, result.message, Toast.LENGTH_LONG).show()
                }
            }
        }
    }
    
    private fun setupListeners() {
        binding.btnLogin.setOnClickListener {
            val email = binding.etEmail.text.toString()
            val password = binding.etPassword.text.toString()
            
            if (validateInputs(email, password)) {
                viewModel.login(email, password)
            }
        }
    }
    
    private fun validateInputs(email: String, password: String): Boolean {
        if (email.isEmpty()) {
            binding.etEmail.error = "Email requerido"
            return false
        }
        if (password.isEmpty()) {
            binding.etPassword.error = "Contraseña requerida"
            return false
        }
        return true
    }
    
    private fun showLoading(show: Boolean) {
        binding.progressBar.visibility = if (show) View.VISIBLE else View.GONE
        binding.btnLogin.isEnabled = !show
    }
    
    private fun navigateToMain() {
        startActivity(Intent(this, MainActivity::class.java))
        finish()
    }
}
```

---

## ⚠️ AndroidManifest.xml

Agregar permiso de internet y configuración de red:

```xml
<manifest xmlns:android="http://schemas.android.com/apk/res/android">
    
    <!-- Permisos -->
    <uses-permission android:name="android.permission.INTERNET" />
    <uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />
    
    <application
        android:usesCleartextTraffic="true"
        ...>
        
        <!-- Activities -->
        
    </application>
</manifest>
```

---

## 🧪 Testing con Postman/Thunder Client

Antes de integrar en Android, prueba los endpoints:

### **1. Login**
```
POST http://10.226.44.156:5000/api/AuthApi/login
Content-Type: application/json

{
  "email": "jose.perez@email.com",
  "password": "123456"
}
```

### **2. Obtener Inmuebles (con token)**
```
GET http://10.226.44.156:5000/api/InmueblesApi/
Authorization: Bearer {token_obtenido_del_login}
```

---

## 📱 Próximos Pasos

1. ✅ Copiar los modelos desde `ANDROID_MODELS.md`
2. ✅ Implementar esta configuración de Retrofit
3. ✅ Crear las pantallas UI
4. ✅ Probar la integración con la API

---

## 🐛 Troubleshooting Común

### **Error: Unable to resolve host**
```kotlin
// Verifica que la IP sea correcta
const val BASE_URL = "http://10.226.44.156:5000/api/"
```

### **Error 401 Unauthorized**
```kotlin
// Verifica que el token se esté enviando correctamente
// Revisa AuthInterceptor y TokenManager
```

### **Error: Cleartext traffic not permitted**
```xml
<!-- Agregar en AndroidManifest.xml -->
android:usesCleartextTraffic="true"
```

---

**🚀 ¡Listo! Ahora tienes la configuración completa de Retrofit para consumir la API de la Inmobiliaria desde Android.**
