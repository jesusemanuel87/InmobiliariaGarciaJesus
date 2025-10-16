# Implementación de Google Maps API - Documentación

## Resumen

Se ha implementado un sistema centralizado para manejar la carga dinámica de Google Maps API en el proyecto InmobiliariaGarciaJesus. Esta implementación permite pasar la API Key desde el backend (appsettings.json) a través de los controladores hacia las vistas y finalmente a los archivos JavaScript.

## Arquitectura de la Implementación

### 1. Configuración Backend

#### appsettings.json
```json
{
  "GoogleMaps": {
    "ApiKey": "TU_API_KEY_AQUI"
  }
}
```

#### InmueblesController.cs
- Se inyecta `IConfiguration` en el constructor
- Se pasa la API Key a las vistas mediante `ViewBag.GoogleMapsApiKey = _configuration["GoogleMaps:ApiKey"];`
- Métodos actualizados: `Index()`, `Details()`, `Create()`, `Edit()`

### 2. Servicio JavaScript Centralizado

#### `/wwwroot/js/shared/google-maps-service.js`

Características principales:
- **Singleton Pattern**: Una sola instancia global (`window.googleMapsService`)
- **Carga Asíncrona**: Evita cargas múltiples de la API
- **Gestión de Callbacks**: Cola de callbacks para ejecutar cuando la API esté lista
- **Manejo de Errores**: Logging y recuperación de errores
- **API Limpia**: Métodos simples para configurar y usar

Métodos principales:
```javascript
// Configurar API Key
googleMapsService.setApiKey(apiKey);

// Cargar Maps y ejecutar callback
googleMapsService.loadGoogleMaps(callback);

// Método de conveniencia
googleMapsService.initializeMap(initMapFunction, apiKey);
```

### 3. Actualización de Archivos JavaScript

#### Archivos modificados:
- `/wwwroot/js/Inmueble/details-inmueble.js`
- `/wwwroot/js/Inmueble/edit-inmueble.js`

**Antes:**
```javascript
// Código hardcodeado con YOUR_API_KEY
script.src = 'https://maps.googleapis.com/maps/api/js?key=YOUR_API_KEY&callback=initMap';
```

**Después:**
```javascript
// Uso del servicio centralizado
if (window.googleMapsService) {
    window.googleMapsService.loadGoogleMaps(() => this.initMap());
} else {
    console.error('GoogleMapsService no está disponible');
}
```

### 4. Actualización de Vistas

#### Vistas modificadas:
- `Views/Inmuebles/Details.cshtml`
- `Views/Inmuebles/Edit.cshtml`
- `Views/Inmuebles/Create.cshtml`

**Patrón implementado:**
```html
@section Scripts {
    <script src="~/js/shared/google-maps-service.js"></script>
    <script src="~/js/Inmueble/[archivo-especifico].js"></script>
    <script>
        // Configurar Google Maps API Key
        @if (!string.IsNullOrEmpty(ViewBag.GoogleMapsApiKey as string))
        {
            <text>
            if (window.googleMapsService) {
                window.googleMapsService.setApiKey('@ViewBag.GoogleMapsApiKey');
            }
            </text>
        }

        // Inicialización específica de la vista...
    </script>
}
```

## Flujo de Datos

```
appsettings.json 
    ↓ (IConfiguration)
InmueblesController 
    ↓ (ViewBag.GoogleMapsApiKey)
Vista Razor 
    ↓ (JavaScript inline)
GoogleMapsService 
    ↓ (API Call)
Google Maps API 
    ↓ (Callback)
Función initMap() específica
```

## Beneficios de la Implementación

### ✅ Ventajas

1. **Centralización**: Un solo punto de configuración para la API Key
2. **Reutilización**: El servicio puede usarse en cualquier parte del proyecto
3. **Prevención de Cargas Múltiples**: Evita cargar la API varias veces
4. **Manejo de Errores**: Logging y recuperación centralizada
5. **Mantenibilidad**: Fácil actualización de la API Key sin tocar código
6. **Seguridad**: API Key no hardcodeada en archivos JavaScript
7. **Flexibilidad**: Soporte para múltiples callbacks y mapas simultáneos

### 🔧 Características Técnicas

- **Compatibilidad**: Funciona con el código existente
- **Performance**: Carga asíncrona y diferida de la API
- **Escalabilidad**: Fácil agregar nuevos mapas o vistas
- **Testing**: Método `reset()` para facilitar pruebas
- **Debugging**: Logging detallado para troubleshooting

## Uso en Nuevas Vistas

Para implementar Google Maps en una nueva vista:

### 1. En el Controller:
```csharp
public async Task<IActionResult> MiVista()
{
    // ... lógica del controller
    ViewBag.GoogleMapsApiKey = _configuration["GoogleMaps:ApiKey"];
    return View();
}
```

### 2. En la Vista:
```html
@section Scripts {
    <script src="~/js/shared/google-maps-service.js"></script>
    <script>
        @if (!string.IsNullOrEmpty(ViewBag.GoogleMapsApiKey as string))
        {
            <text>
            if (window.googleMapsService) {
                window.googleMapsService.setApiKey('@ViewBag.GoogleMapsApiKey');
            }
            </text>
        }

        // Tu código de inicialización
        function initMiMapa() {
            // Lógica del mapa
        }

        // Cargar Maps cuando esté listo
        $(document).ready(function() {
            if (window.googleMapsService) {
                window.googleMapsService.loadGoogleMaps(initMiMapa);
            }
        });
    </script>
}
```

### 3. En archivo JavaScript separado (opcional):
```javascript
class MiMapaManager {
    initializeGoogleMaps() {
        if (window.googleMapsService) {
            window.googleMapsService.loadGoogleMaps(() => this.initMap());
        }
    }

    initMap() {
        // Tu lógica del mapa aquí
    }
}
```

## Troubleshooting

### Problemas Comunes

1. **"GoogleMapsService no está disponible"**
   - Verificar que `google-maps-service.js` esté incluido antes que otros scripts
   - Verificar que no hay errores de JavaScript que impidan la carga

2. **"Google Maps API Key no configurada"**
   - Verificar `appsettings.json` tiene la sección `GoogleMaps:ApiKey`
   - Verificar que el controller pasa `ViewBag.GoogleMapsApiKey`

3. **Mapas no se cargan**
   - Verificar la API Key en la consola del navegador
   - Verificar que la API Key tiene permisos para Maps JavaScript API
   - Verificar restricciones de dominio en Google Cloud Console

### Debugging

Activar logging en la consola:
```javascript
// El servicio ya incluye console.log para debugging
// Revisar la consola del navegador para ver el flujo de carga
```

## Seguridad

### Recomendaciones:

1. **Restricciones de API Key**: Configurar restricciones de dominio en Google Cloud Console
2. **Límites de Uso**: Configurar quotas apropiadas
3. **Monitoreo**: Revisar uso de la API regularmente
4. **Variables de Entorno**: Considerar usar variables de entorno para producción

### Configuración de Producción:

```json
// appsettings.Production.json
{
  "GoogleMaps": {
    "ApiKey": "${GOOGLE_MAPS_API_KEY}" // Variable de entorno
  }
}
```

## Mantenimiento

### Actualizaciones Futuras:

1. **Nuevas Funcionalidades**: Agregar métodos al `GoogleMapsService`
2. **Optimizaciones**: Implementar caché local de tiles si es necesario
3. **Monitoreo**: Agregar métricas de uso de la API
4. **Testing**: Implementar tests unitarios para el servicio

---

**Fecha de Implementación**: 2025-01-18  
**Versión**: 1.0  
**Autor**: Sistema de Inmobiliaria García Jesús
