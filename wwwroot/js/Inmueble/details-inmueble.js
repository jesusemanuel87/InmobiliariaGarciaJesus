// Inmueble details functionality
class InmuebleDetailsManager {
    constructor(inmuebleId, latitud, longitud, direccion, localidad, provincia) {
        this.inmuebleId = inmuebleId;
        this.latitud = latitud;
        this.longitud = longitud;
        this.direccion = direccion;
        this.localidad = localidad;
        this.provincia = provincia;
        this.init();
    }

    init() {
        $(document).ready(() => {
            this.cargarImagenes();
            this.initializeGoogleMaps();
            console.log('InmuebleDetailsManager initialized');
        });
    }

    cargarImagenes() {
        $.get('/Inmuebles/GetImagenes', { id: this.inmuebleId, readOnly: true })
            .done((data) => {
                $('#imagenes-container').html(data);
            })
            .fail(() => {
                $('#imagenes-container').html('<div class="col-12 text-center text-danger"><i class="fas fa-exclamation-triangle"></i> Error al cargar imágenes</div>');
            });
    }

    initializeGoogleMaps() {
        if (this.latitud && this.longitud) {
            // Load Google Maps when document is ready
            if (typeof google !== 'undefined' && google.maps) {
                this.initMap();
            } else {
                // Load Google Maps API if not available
                window.initMap = () => this.initMap();
                const script = document.createElement('script');
                script.src = 'https://maps.googleapis.com/maps/api/js?key=YOUR_API_KEY&callback=initMap';
                script.async = true;
                script.defer = true;
                document.head.appendChild(script);
            }
        }
    }

    initMap() {
        const location = { lat: parseFloat(this.latitud), lng: parseFloat(this.longitud) };
        const map = new google.maps.Map(document.getElementById('map'), {
            zoom: 16,
            center: location,
            mapTypeId: google.maps.MapTypeId.ROADMAP
        });
        
        const marker = new google.maps.Marker({
            position: location,
            map: map,
            title: this.direccion,
            animation: google.maps.Animation.DROP
        });

        const infoContent = `<div><strong>${this.direccion}</strong><br>` +
                          `${this.localidad ? this.localidad : ''}` +
                          `${this.provincia ? ', ' + this.provincia : ''}</div>`;

        const infoWindow = new google.maps.InfoWindow({
            content: infoContent
        });

        marker.addListener('click', () => {
            infoWindow.open(map, marker);
        });

        // Show info window by default
        infoWindow.open(map, marker);
    }
}

// Global function for backward compatibility
let inmuebleDetailsManager;

function cargarImagenes() {
    if (inmuebleDetailsManager) {
        inmuebleDetailsManager.cargarImagenes();
    }
}
