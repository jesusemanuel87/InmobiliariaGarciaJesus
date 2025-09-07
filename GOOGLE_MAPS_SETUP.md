# Configuración de Google Maps API

## Descripción
Este documento explica cómo configurar la API de Google Maps para mostrar mapas interactivos en las vistas de Inmuebles (Edit y Details).

## Pasos para configurar Google Maps API

### 1. Obtener una API Key de Google Maps

1. Ve a [Google Cloud Console](https://console.cloud.google.com/)
2. Crea un nuevo proyecto o selecciona uno existente
3. Habilita la API de Google Maps JavaScript API:
   - Ve a "APIs & Services" > "Library"
   - Busca "Maps JavaScript API"
   - Haz clic en "Enable"

### 2. Crear una API Key

1. Ve a "APIs & Services" > "Credentials"
2. Haz clic en "Create Credentials" > "API Key"
3. Copia la API Key generada

### 3. Configurar restricciones (Recomendado)

1. Haz clic en la API Key creada
2. En "Application restrictions":
   - Selecciona "HTTP referrers (web sites)"
   - Agrega tu dominio: `localhost:*`, `127.0.0.1:*`, y tu dominio de producción
3. En "API restrictions":
   - Selecciona "Restrict key"
   - Selecciona "Maps JavaScript API"

### 4. Actualizar el código

Reemplaza `YOUR_API_KEY` en los siguientes archivos:

#### Views/Inmuebles/Edit.cshtml
```javascript
script.src = 'https://maps.googleapis.com/maps/api/js?key=TU_API_KEY_AQUI&callback=initMap';
```

#### Views/Inmuebles/Details.cshtml
```javascript
script.src = 'https://maps.googleapis.com/maps/api/js?key=TU_API_KEY_AQUI&callback=initMap';
```

### 5. Alternativa: Configuración por appsettings.json

Para mayor seguridad, puedes almacenar la API Key en `appsettings.json`:

```json
{
  "GoogleMaps": {
    "ApiKey": "TU_API_KEY_AQUI"
  }
}
```

Y luego pasarla desde el controlador a la vista:

```csharp
ViewBag.GoogleMapsApiKey = _configuration["GoogleMaps:ApiKey"];
```

En la vista:
```javascript
script.src = 'https://maps.googleapis.com/maps/api/js?key=@ViewBag.GoogleMapsApiKey&callback=initMap';
```

## Funcionalidades implementadas

### Integración con API Georef de Argentina
- **API utilizada**: `https://apis.datos.gob.ar/georef/api/`
- **Dropdowns dinámicos** para Provincia y Localidad
- **Valores por defecto**: San Luis (provincia) y San Luis (localidad)
- **Cascading dropdowns**: Las localidades se cargan automáticamente al seleccionar una provincia
- **Cache inteligente**: Los datos se almacenan localmente para mejorar rendimiento
- **Fallback**: Datos por defecto en caso de fallo de la API

### En la vista Create (Crear Inmueble)
- Dropdown de provincias cargado desde API Georef
- Dropdown de localidades que se actualiza según la provincia seleccionada
- San Luis preseleccionado por defecto

### En la vista Edit (Editar Inmueble)
- Dropdowns de provincia y localidad con valores actuales del inmueble
- Mapa interactivo que muestra la ubicación del inmueble
- Marcador en las coordenadas especificadas
- Botón para abrir en Google Maps
- Zoom nivel 15 para vista detallada

### En la vista Details (Detalles del Inmueble)
- Mapa interactivo con mayor zoom (nivel 16)
- Marcador con animación de caída
- Info window que se abre automáticamente
- Información del inmueble en el info window
- Botón para abrir en Google Maps

### En la vista Index (Lista de Inmuebles)
- Columna "Ubicación" que muestra Localidad y Provincia
- Botón directo a Google Maps para inmuebles con coordenadas

## Campos agregados al modelo Inmueble

- **Localidad**: Campo de texto opcional (100 caracteres)
- **Provincia**: Campo de texto opcional (100 caracteres)
- **GoogleMapsUrl**: Propiedad calculada que genera el enlace a Google Maps

## Base de datos

Ejecutar el script `database_inmuebles_location_update.sql` para agregar los nuevos campos a la tabla Inmuebles.

## Notas importantes

1. **Límites de uso**: Google Maps API tiene límites de uso gratuito. Revisa los precios en [Google Maps Platform Pricing](https://cloud.google.com/maps-platform/pricing)

2. **Seguridad**: Nunca expongas tu API Key en el código fuente público. Usa variables de entorno o configuración segura.

3. **Fallback**: El código incluye detección automática de la API de Google Maps y carga dinámica si no está disponible.

4. **Coordenadas**: Los campos Latitud y Longitud ya existían en el modelo. Se mantuvieron para compatibilidad.

## API Georef de Argentina

### Descripción
Se integró la API oficial del gobierno argentino para obtener provincias y localidades de forma dinámica.

### Endpoints utilizados
- **Provincias**: `https://apis.datos.gob.ar/georef/api/provincias?campos=id,nombre&max=24`
- **Localidades**: `https://apis.datos.gob.ar/georef/api/localidades?provincia={nombre}&campos=id,nombre&max=1000`

### Características
- **Sin autenticación**: La API es pública y no requiere API Key
- **Cache local**: Los datos se almacenan en memoria para mejorar rendimiento
- **Fallback**: Datos por defecto en caso de fallo de la API
- **Ordenamiento**: Provincias y localidades se ordenan alfabéticamente

### Archivos agregados
- `wwwroot/js/georef-api.js`: Servicio JavaScript para consumir la API

### Configuración por defecto
- **Provincia**: San Luis
- **Localidad**: San Luis

## Ejemplo de coordenadas para Argentina

- **San Luis**: Latitud: -33.2950, Longitud: -66.3356
- **Buenos Aires**: Latitud: -34.6037, Longitud: -58.3816
- **Córdoba**: Latitud: -31.4201, Longitud: -64.1888
- **Rosario**: Latitud: -32.9442, Longitud: -60.6505
- **Mendoza**: Latitud: -32.8895, Longitud: -68.8458
