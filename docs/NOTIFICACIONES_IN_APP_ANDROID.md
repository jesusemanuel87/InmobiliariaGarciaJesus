# 🔔 Sistema de Notificaciones In-App para Android

## 📋 **Descripción**

Sistema completo de notificaciones persistentes que se guardan en la base de datos y son consultables desde la app Android. **Mucho mejor que emails** porque:

✅ Las notificaciones aparecen **dentro de la app**  
✅ Se guardan en BD y **no se pierden**  
✅ El propietario puede **verlas cuando quiera**  
✅ Contador de notificaciones no leídas (badge) ⭐  
✅ Posibilidad de marcar como leídas  
✅ Historial completo de notificaciones  

---

## 🎯 **Flujo Completo**

```
1. Inquilino/Propietario registra un pago
   ↓
2. Backend guarda notificación en BD
   ↓
3. App Android consulta periódicamente o al abrir
   ↓
4. Muestra notificaciones no leídas con badge
   ↓
5. Usuario toca notificación → Se marca como leída
   ↓
6. Puede navegar al detalle del pago
```

---

## 📊 **Base de Datos**

### **Tabla: `notificaciones`**

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Id` | INT | PK Auto-increment |
| `PropietarioId` | INT | FK a propietarios |
| `Tipo` | VARCHAR(50) | Tipo de notificación |
| `Titulo` | VARCHAR(200) | Título corto |
| `Mensaje` | TEXT | Mensaje completo |
| `Datos` | JSON | Datos adicionales (pagoId, contratoId, etc.) |
| `Leida` | BOOLEAN | Si fue leída o no |
| `FechaCreacion` | DATETIME | Cuándo se creó |
| `FechaLeida` | DATETIME | Cuándo se leyó (NULL si no leída) |

### **Tipos de Notificaciones:**
- `PagoRegistrado` - Cuando se registra un pago
- `PagoVencido` - Cuando un pago está vencido
- `ProximoVencimiento` - Recordatorio de vencimiento próximo
- `NuevoContrato` - Cuando se crea un contrato nuevo
- `ContratoFinalizado` - Cuando termina un contrato

---

## 🌐 **API Endpoints**

### **1. Obtener Todas las Notificaciones**

```http
GET /api/NotificacionesApi
Authorization: Bearer {token}
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "tipo": "PagoRegistrado",
      "titulo": "💰 Pago Recibido",
      "mensaje": "Se ha registrado el pago de $150,000.00 de Juan Pérez por el inmueble en Av. Illia 123",
      "datos": "{\"pagoId\":1,\"contratoId\":5,\"inmuebleId\":10,\"monto\":150000,\"numeroCuota\":1}",
      "leida": false,
      "fechaCreacion": "2025-10-30T19:30:00",
      "fechaLeida": null
    }
  ]
}
```

---

### **2. Obtener Solo No Leídas**

```http
GET /api/NotificacionesApi/no-leidas
Authorization: Bearer {token}
```

**Response:** Mismo formato, pero solo las notificaciones con `leida: false`

---

### **3. Obtener Contador (para Badge)**

```http
GET /api/NotificacionesApi/contador
Authorization: Bearer {token}
```

**Response:**
```json
{
  "success": true,
  "data": 5,  // ← Número de notificaciones no leídas
  "message": null
}
```

---

### **4. Marcar una Notificación como Leída**

```http
PATCH /api/NotificacionesApi/{notificacionId}/marcar-leida
Authorization: Bearer {token}
```

**Response:**
```json
{
  "success": true,
  "message": "Notificación marcada como leída"
}
```

---

### **5. Marcar TODAS como Leídas**

```http
PATCH /api/NotificacionesApi/marcar-todas-leidas
Authorization: Bearer {token}
```

**Response:**
```json
{
  "success": true,
  "message": "5 notificaciones marcadas como leídas"
}
```

---

### **6. Eliminar una Notificación**

```http
DELETE /api/NotificacionesApi/{notificacionId}
Authorization: Bearer {token}
```

**Response:**
```json
{
  "success": true,
  "message": "Notificación eliminada"
}
```

---

## 📱 **Implementación en Android**

### **Paso 1: Modelos de Datos**

```kotlin
data class Notificacion(
    val id: Int,
    val tipo: String,
    val titulo: String,
    val mensaje: String,
    val datos: String?,
    val leida: Boolean,
    val fechaCreacion: String,
    val fechaLeida: String?
)

data class DatosNotificacion(
    val pagoId: Int?,
    val contratoId: Int?,
    val inmuebleId: Int?,
    val monto: Double?,
    val numeroCuota: Int?
)
```

---

### **Paso 2: Retrofit Interface**

```kotlin
interface InmobiliariaApiService {
    // Obtener todas las notificaciones
    @GET("NotificacionesApi")
    suspend fun getNotificaciones(
        @Header("Authorization") token: String
    ): ApiResponse<List<Notificacion>>
    
    // Solo no leídas
    @GET("NotificacionesApi/no-leidas")
    suspend fun getNotificacionesNoLeidas(
        @Header("Authorization") token: String
    ): ApiResponse<List<Notificacion>>
    
    // Contador para badge
    @GET("NotificacionesApi/contador")
    suspend fun getContadorNoLeidas(
        @Header("Authorization") token: String
    ): ApiResponse<Int>
    
    // Marcar como leída
    @PATCH("NotificacionesApi/{id}/marcar-leida")
    suspend fun marcarComoLeida(
        @Path("id") notificacionId: Int,
        @Header("Authorization") token: String
    ): ApiResponse<Unit>
    
    // Marcar todas como leídas
    @PATCH("NotificacionesApi/marcar-todas-leidas")
    suspend fun marcarTodasLeidas(
        @Header("Authorization") token: String
    ): ApiResponse<Unit>
    
    // Eliminar notificación
    @DELETE("NotificacionesApi/{id}")
    suspend fun eliminarNotificacion(
        @Path("id") notificacionId: Int,
        @Header("Authorization") token: String
    ): ApiResponse<Unit>
}
```

---

### **Paso 3: UI - Badge en el Toolbar**

```xml
<!-- En tu layout principal (activity_main.xml o similar) -->
<androidx.constraintlayout.widget.ConstraintLayout>
    
    <androidx.appcompat.widget.Toolbar
        android:id="@/toolbar"
        android:layout_width="match_parent"
        android:layout_height="?attr/actionBarSize">
        
        <!-- Botón de notificaciones con badge -->
        <FrameLayout
            android:id="@+id/btnNotificaciones"
            android:layout_width="wrap_content"
            android:layout_height="wrap_content"
            android:layout_gravity="end"
            android:padding="8dp">
            
            <ImageView
                android:layout_width="24dp"
                android:layout_height="24dp"
                android:src="@drawable/ic_notifications"
                android:tint="@color/white"/>
            
            <TextView
                android:id="@+id/badgeNotificaciones"
                android:layout_width="20dp"
                android:layout_height="20dp"
                android:layout_gravity="top|end"
                android:background="@drawable/badge_background"
                android:gravity="center"
                android:text="5"
                android:textColor="@color/white"
                android:textSize="12sp"
                android:visibility="gone"/>
        </FrameLayout>
    </androidx.appcompat.widget.Toolbar>
    
</androidx.constraintlayout.widget.ConstraintLayout>
```

**Badge Background (`drawable/badge_background.xml`):**
```xml
<?xml version="1.0" encoding="utf-8"?>
<shape xmlns:android="http://schemas.android.com/apk/res/android"
    android:shape="oval">
    <solid android:color="@color/red"/>
    <size android:width="20dp" android:height="20dp"/>
</shape>
```

---

### **Paso 4: Actualizar Badge Periódicamente**

```kotlin
class MainActivity : AppCompatActivity() {
    
    private lateinit var badgeNotificaciones: TextView
    private val handler = Handler(Looper.getMainLooper())
    private val updateBadgeRunnable = object : Runnable {
        override fun run() {
            actualizarBadge()
            handler.postDelayed(this, 30000) // Cada 30 segundos
        }
    }
    
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_main)
        
        badgeNotificaciones = findViewById(R.id.badgeNotificaciones)
        
        // Configurar click en botón de notificaciones
        findViewById<View>(R.id.btnNotificaciones).setOnClickListener {
            abrirNotificaciones()
        }
        
        // Iniciar actualización periódica
        handler.post(updateBadgeRunnable)
    }
    
    override fun onDestroy() {
        super.onDestroy()
        handler.removeCallbacks(updateBadgeRunnable)
    }
    
    private fun actualizarBadge() {
        lifecycleScope.launch {
            try {
                val token = "Bearer ${PreferenceHelper.getToken(this@MainActivity)}"
                val response = RetrofitClient.api.getContadorNoLeidas(token)
                
                if (response.success && response.data != null) {
                    val contador = response.data
                    
                    runOnUiThread {
                        if (contador > 0) {
                            badgeNotificaciones.text = contador.toString()
                            badgeNotificaciones.visibility = View.VISIBLE
                        } else {
                            badgeNotificaciones.visibility = View.GONE
                        }
                    }
                }
            } catch (e: Exception) {
                Log.e("MainActivity", "Error al actualizar badge", e)
            }
        }
    }
    
    private fun abrirNotificaciones() {
        val intent = Intent(this, NotificacionesActivity::class.java)
        startActivity(intent)
    }
}
```

---

### **Paso 5: Activity de Notificaciones (RecyclerView)**

```kotlin
class NotificacionesActivity : AppCompatActivity() {
    
    private lateinit var recyclerView: RecyclerView
    private lateinit var adapter: NotificacionesAdapter
    private val notificaciones = mutableListOf<Notificacion>()
    
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_notificaciones)
        
        // Setup RecyclerView
        recyclerView = findViewById(R.id.recyclerViewNotificaciones)
        adapter = NotificacionesAdapter(notificaciones) { notificacion ->
            onNotificacionClick(notificacion)
        }
        recyclerView.adapter = adapter
        recyclerView.layoutManager = LinearLayoutManager(this)
        
        // Cargar notificaciones
        cargarNotificaciones()
    }
    
    private fun cargarNotificaciones() {
        lifecycleScope.launch {
            try {
                val token = "Bearer ${PreferenceHelper.getToken(this@NotificacionesActivity)}"
                val response = RetrofitClient.api.getNotificaciones(token)
                
                if (response.success && response.data != null) {
                    notificaciones.clear()
                    notificaciones.addAll(response.data)
                    adapter.notifyDataSetChanged()
                }
            } catch (e: Exception) {
                Toast.makeText(this@NotificacionesActivity, 
                    "Error al cargar notificaciones", Toast.LENGTH_SHORT).show()
            }
        }
    }
    
    private fun onNotificacionClick(notificacion: Notificacion) {
        // 1. Marcar como leída
        lifecycleScope.launch {
            try {
                val token = "Bearer ${PreferenceHelper.getToken(this@NotificacionesActivity)}"
                RetrofitClient.api.marcarComoLeida(notificacion.id, token)
            } catch (e: Exception) {
                Log.e("Notificaciones", "Error al marcar como leída", e)
            }
        }
        
        // 2. Navegar según el tipo y datos
        val datos = Gson().fromJson(notificacion.datos, DatosNotificacion::class.java)
        
        when (notificacion.tipo) {
            "PagoRegistrado" -> {
                // Navegar a detalle del pago
                val intent = Intent(this, DetalleContratoActivity::class.java)
                intent.putExtra("contratoId", datos.contratoId)
                startActivity(intent)
            }
            // Otros tipos...
        }
    }
}
```

---

### **Paso 6: Adapter del RecyclerView**

```kotlin
class NotificacionesAdapter(
    private val notificaciones: List<Notificacion>,
    private val onClick: (Notificacion) -> Unit
) : RecyclerView.Adapter<NotificacionesAdapter.ViewHolder>() {
    
    class ViewHolder(view: View) : RecyclerView.ViewHolder(view) {
        val tvTitulo: TextView = view.findViewById(R.id.tvTitulo)
        val tvMensaje: TextView = view.findViewById(R.id.tvMensaje)
        val tvFecha: TextView = view.findViewById(R.id.tvFecha)
        val indicadorNoLeida: View = view.findViewById(R.id.indicadorNoLeida)
    }
    
    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): ViewHolder {
        val view = LayoutInflater.from(parent.context)
            .inflate(R.layout.item_notificacion, parent, false)
        return ViewHolder(view)
    }
    
    override fun onBindViewHolder(holder: ViewHolder, position: Int) {
        val notificacion = notificaciones[position]
        
        holder.tvTitulo.text = notificacion.titulo
        holder.tvMensaje.text = notificacion.mensaje
        holder.tvFecha.text = formatearFecha(notificacion.fechaCreacion)
        
        // Mostrar indicador de no leída
        holder.indicadorNoLeida.visibility = if (!notificacion.leida) {
            View.VISIBLE
        } else {
            View.GONE
        }
        
        // Estilo diferente si no está leída
        if (!notificacion.leida) {
            holder.tvTitulo.setTypeface(null, Typeface.BOLD)
            holder.itemView.setBackgroundColor(Color.parseColor("#E3F2FD"))
        } else {
            holder.tvTitulo.setTypeface(null, Typeface.NORMAL)
            holder.itemView.setBackgroundColor(Color.WHITE)
        }
        
        holder.itemView.setOnClickListener {
            onClick(notificacion)
        }
    }
    
    override fun getItemCount() = notificaciones.size
    
    private fun formatearFecha(fecha: String): String {
        // Formato: "Hace 2 horas" o "30/10/2025"
        // Implementar lógica de formateo
        return fecha
    }
}
```

---

## 🎨 **Layout del Item de Notificación**

```xml
<!-- item_notificacion.xml -->
<androidx.cardview.widget.CardView
    xmlns:android="http://schemas.android.com/apk/res/android"
    xmlns:app="http://schemas.android.com/apk/res-auto"
    android:layout_width="match_parent"
    android:layout_height="wrap_content"
    android:layout_margin="8dp"
    app:cardCornerRadius="8dp"
    app:cardElevation="2dp">
    
    <LinearLayout
        android:layout_width="match_parent"
        android:layout_height="wrap_content"
        android:orientation="horizontal"
        android:padding="16dp">
        
        <!-- Indicador de no leída -->
        <View
            android:id="@+id/indicadorNoLeida"
            android:layout_width="8dp"
            android:layout_height="match_parent"
            android:background="@color/blue"
            android:layout_marginEnd="12dp"/>
        
        <LinearLayout
            android:layout_width="0dp"
            android:layout_height="wrap_content"
            android:layout_weight="1"
            android:orientation="vertical">
            
            <TextView
                android:id="@+id/tvTitulo"
                android:layout_width="match_parent"
                android:layout_height="wrap_content"
                android:text="💰 Pago Recibido"
                android:textSize="16sp"
                android:textStyle="bold"/>
            
            <TextView
                android:id="@+id/tvMensaje"
                android:layout_width="match_parent"
                android:layout_height="wrap_content"
                android:layout_marginTop="4dp"
                android:text="Se ha registrado el pago de $150,000.00"
                android:textSize="14sp"
                android:maxLines="2"
                android:ellipsize="end"/>
            
            <TextView
                android:id="@+id/tvFecha"
                android:layout_width="match_parent"
                android:layout_height="wrap_content"
                android:layout_marginTop="8dp"
                android:text="Hace 2 horas"
                android:textSize="12sp"
                android:textColor="@color/gray"/>
        </LinearLayout>
    </LinearLayout>
</androidx.cardview.widget.CardView>
```

---

## 🔄 **Polling vs Push Notifications**

### **Opción 1: Polling (Implementado arriba)**
✅ Más simple de implementar  
✅ No requiere Firebase  
⚠️ Consulta cada 30 segundos (consumo de batería moderado)  

### **Opción 2: Firebase Cloud Messaging (FCM)** 
✅ Notificaciones instantáneas  
✅ Menos consumo de batería  
❌ Requiere integrar Firebase  
❌ Backend debe enviar push via FCM  

**Recomendación:** Empezar con Polling (Opción 1), luego migrar a FCM si es necesario.

---

## 📊 **Resumen de Implementación**

### **Backend (✅ Ya está):**
1. ✅ Tabla `notificaciones` en BD
2. ✅ Modelo `Notificacion.cs`
3. ✅ `NotificacionesApiController` con 6 endpoints
4. ✅ `NotificacionService` que guarda notificaciones al registrar pagos
5. ✅ Compilación exitosa

### **Android (⏳ Por implementar):**
1. ⏳ Crear modelos de datos
2. ⏳ Agregar endpoints a Retrofit
3. ⏳ Crear UI de notificaciones (RecyclerView)
4. ⏳ Implementar badge en toolbar
5. ⏳ Configurar polling cada 30 segundos
6. ⏳ Navegación según tipo de notificación

---

## 🧪 **Testing**

### **1. Crear Tabla:**
```sql
-- Ejecutar desde MySQL Workbench
source D:/Documents/ULP/2025/NET/Proyecto/InmobiliariaGarciaJesus/Database/Scripts/create_notificaciones_table.sql
```

### **2. Probar API desde Postman:**

```bash
# Obtener notificaciones
curl http://localhost:5000/api/NotificacionesApi \
  -H "Authorization: Bearer {token}"

# Contador
curl http://localhost:5000/api/NotificacionesApi/contador \
  -H "Authorization: Bearer {token}"

# Marcar como leída
curl -X PATCH http://localhost:5000/api/NotificacionesApi/1/marcar-leida \
  -H "Authorization: Bearer {token}"
```

### **3. Registrar un Pago:**
El sistema automáticamente creará una notificación.

---

## 🚀 **¡Listo para Usar!**

✅ Sistema de notificaciones in-app implementado  
✅ Las notificaciones se guardan en BD  
✅ API REST completa con 6 endpoints  
✅ Documentación con código Kotlin completo  
✅ Ejemplos de UI y layouts  

**Mucho mejor que emails para una app móvil! 🎉**
