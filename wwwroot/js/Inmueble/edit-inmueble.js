// Inmueble edit functionality
class InmuebleEditManager {
    constructor(inmuebleId, provincia, localidad, latitud, longitud, direccion) {
        this.inmuebleId = inmuebleId;
        this.provincia = provincia;
        this.localidad = localidad;
        this.latitud = latitud;
        this.longitud = longitud;
        this.direccion = direccion;
        this.init();
    }

    init() {
        $(document).ready(() => {
            this.cargarImagenes();
            this.initializeGeorefDropdowns();
            this.initializeGoogleMaps();
            console.log('InmuebleEditManager initialized');
        });
    }

    initializeGeorefDropdowns() {
        // Initialize province and locality dropdowns
        // Use model values if available, otherwise default to San Luis
        const provinciaActual = this.provincia || 'San Luis';
        const localidadActual = this.localidad || 'San Luis';
        
        if (typeof georefService !== 'undefined') {
            georefService.inicializarDropdowns('provinciaSelect', 'localidadSelect', provinciaActual, localidadActual);
        } else {
            console.error('georefService not available');
        }
    }

    cargarImagenes() {
        $.get(`/Inmuebles/GetImagenes`, { id: this.inmuebleId })
            .done((data) => {
                $('#imagenes-container').html(data);
            })
            .fail(() => {
                $('#imagenes-container').html('<div class="col-12 text-center text-danger"><i class="fas fa-exclamation-triangle"></i> Error al cargar imágenes</div>');
            });
    }

    eliminarImagen(imagenId) {
        if (confirm('¿Está seguro de que desea eliminar esta imagen?')) {
            $.post('/Inmuebles/DeleteImage', { 
                id: imagenId, 
                inmuebleId: this.inmuebleId 
            })
            .done((response) => {
                this.cargarImagenes();
                this.mostrarMensaje('Imagen eliminada correctamente', 'success');
            })
            .fail(() => {
                this.mostrarMensaje('Error al eliminar la imagen', 'danger');
            });
        }
    }

    establecerPortada(imagenId) {
        console.log('Estableciendo portada para imagen ID:', imagenId, 'Inmueble ID:', this.inmuebleId);
        
        $.post('/Inmuebles/SetPortada', { 
            id: imagenId, 
            inmuebleId: this.inmuebleId 
        })
        .done((response) => {
            console.log('Respuesta del servidor:', response);
            if (response.success) {
                this.cargarImagenes();
                this.mostrarMensaje(response.message || 'Imagen de portada actualizada', 'success');
            } else {
                this.mostrarMensaje(response.message || 'Error al establecer imagen de portada', 'danger');
            }
        })
        .fail((xhr, status, error) => {
            console.error('Error en la petición:', xhr, status, error);
            this.mostrarMensaje('Error al establecer imagen de portada: ' + error, 'danger');
        });
    }

    mostrarMensaje(mensaje, tipo) {
        // Remove previous messages
        $('.alert-temp').remove();
        
        const alertHtml = `<div class="alert alert-${tipo} alert-dismissible fade show alert-temp" role="alert">
                           ${mensaje}
                           <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
                           </div>`;
        
        // Insert at the beginning of images container
        $('#imagenes-container').prepend(alertHtml);
        
        // Auto-hide after 3 seconds
        setTimeout(() => {
            $('.alert-temp').fadeOut();
        }, 3000);
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
            zoom: 15,
            center: location
        });
        const marker = new google.maps.Marker({
            position: location,
            map: map,
            title: this.direccion
        });
    }
}

// Global functions for backward compatibility
let inmuebleEditManager;

function cargarImagenes() {
    if (inmuebleEditManager) {
        inmuebleEditManager.cargarImagenes();
    }
}

function eliminarImagen(imagenId) {
    if (inmuebleEditManager) {
        inmuebleEditManager.eliminarImagen(imagenId);
    }
}

function establecerPortada(imagenId) {
    if (inmuebleEditManager) {
        inmuebleEditManager.establecerPortada(imagenId);
    }
}
