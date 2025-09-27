$(document).ready(function() {
    // Inicializar dropdowns de provincia y localidad
    var provinciaActual = $('#provinciaActual').val() || '';
    var localidadActual = $('#localidadActual').val() || '';
    
    georefService.inicializarDropdowns('provinciaFilter', 'localidadFilter', provinciaActual, localidadActual);
    
    // Inicializar slider de precios con delay para asegurar que DOM esté listo
    setTimeout(function() {
        inicializarSliderPrecios();
    }, 100);
    
    // Auto-submit en cambios de filtros
    $('#provinciaFilter, #localidadFilter').on('change', function() {
        setTimeout(() => $('#filtrosForm').submit(), 500);
    });
});

function inicializarSliderPrecios() {
    const minPrice = parseFloat($('#precioMinimo').val()) || 0;
    const maxPrice = parseFloat($('#precioMaximo').val()) || 1000000;
    
    // Get current values from form inputs (these persist after form submission)
    const currentMin = parseFloat($('#precioMin').val()) || minPrice;
    const currentMax = parseFloat($('#precioMax').val()) || maxPrice;
    
    
    // Set slider attributes and values to match current form values
    $('#priceRangeMin').attr('min', minPrice).attr('max', maxPrice).val(currentMin);
    $('#priceRangeMax').attr('min', minPrice).attr('max', maxPrice).val(currentMax);
    
    // Force update the slider values after setting them
    document.getElementById('priceRangeMin').value = currentMin;
    document.getElementById('priceRangeMax').value = currentMax;
    
    // Update visual slider to reflect current values
    updateRangeSlider();
    
    // Event listeners for range inputs
    $('#priceRangeMin, #priceRangeMax').on('input', function() {
        const minVal = parseFloat($('#priceRangeMin').val());
        const maxVal = parseFloat($('#priceRangeMax').val());
        
        // Prevent overlap
        if (minVal >= maxVal) {
            if (this.id === 'priceRangeMin') {
                $('#priceRangeMin').val(maxVal - 1000);
            } else {
                $('#priceRangeMax').val(minVal + 1000);
            }
        }
        
        // Update number inputs
        $('#precioMin').val($('#priceRangeMin').val());
        $('#precioMax').val($('#priceRangeMax').val());
        
        updateRangeSlider();
    });
    
    // Event listeners for number inputs
    $('#precioMin, #precioMax').on('input', function() {
        const minVal = parseFloat($('#precioMin').val()) || minPrice;
        const maxVal = parseFloat($('#precioMax').val()) || maxPrice;
        
        $('#priceRangeMin').val(minVal);
        $('#priceRangeMax').val(maxVal);
        
        updateRangeSlider();
    });
}

function updateRangeSlider() {
    const minPrice = parseFloat($('#precioMinimo').val()) || 0;
    const maxPrice = parseFloat($('#precioMaximo').val()) || 1000000;
    const currentMin = parseFloat($('#priceRangeMin').val()) || minPrice;
    const currentMax = parseFloat($('#priceRangeMax').val()) || maxPrice;
    
    // Ensure we have valid range
    if (maxPrice <= minPrice) return;
    
    const minPercent = Math.max(0, Math.min(100, ((currentMin - minPrice) / (maxPrice - minPrice)) * 100));
    const maxPercent = Math.max(0, Math.min(100, ((currentMax - minPrice) / (maxPrice - minPrice)) * 100));
    
    $('#rangeFill').css({
        'left': minPercent + '%',
        'width': Math.max(0, maxPercent - minPercent) + '%'
    });
}

function limpiarFiltros() {
    $('#provinciaFilter').val('');
    $('#localidadFilter').val('');
    $('input[name="fechaDesde"]').val('');
    $('input[name="fechaHasta"]').val('');
    $('select[name="tipo"]').val('');
    $('select[name="uso"]').val('');
    
    // Reset price inputs to original range
    const minPrice = parseFloat($('#precioMinimo').val()) || 0;
    const maxPrice = parseFloat($('#precioMaximo').val()) || 1000000;
    $('#precioMin').val(minPrice);
    $('#precioMax').val(maxPrice);
    
    // Limpiar localidades cuando no hay provincia seleccionada
    document.getElementById('localidadFilter').innerHTML = '<option value="">Seleccione primero una provincia</option>';
    
    // Submit form
    $('#filtrosForm').submit();
}

function cambiarVista(vista) {
    const container = $('#propertyContainer');
    const buttons = $('.btn-group .btn');
    
    buttons.removeClass('active');
    
    if (vista === 'list') {
        container.removeClass('row').addClass('list-view');
        buttons.eq(1).addClass('active');
    } else {
        container.removeClass('list-view').addClass('row');
        buttons.eq(0).addClass('active');
    }
}

function verDetalles(inmuebleId) {
    // Mostrar modal con detalles del inmueble
    mostrarModalDetalles(inmuebleId);
}

// Animación de carga de cards
function animarCards() {
    $('.property-card').each(function(index) {
        $(this).css('animation-delay', (index * 0.1) + 's');
    });
}

// Ejecutar animación al cargar
$(window).on('load', function() {
    animarCards();
});

// ===== FUNCIONES DEL MODAL DE DETALLES =====

// Variable global para el manager del modal
let modalInmuebleManager = null;

// Función principal para mostrar el modal de detalles
function mostrarModalDetalles(inmuebleId) {
    // Mostrar loading
    $('#inmuebleModalContainer').html(`
        <div class="modal fade show" style="display: block; background: rgba(0,0,0,0.5);">
            <div class="modal-dialog modal-dialog-centered">
                <div class="modal-content">
                    <div class="modal-body text-center py-5">
                        <div class="spinner-border text-primary mb-3" role="status">
                            <span class="visually-hidden">Cargando...</span>
                        </div>
                        <h5>Cargando detalles del inmueble...</h5>
                    </div>
                </div>
            </div>
        </div>
    `);

    // Cargar el modal via AJAX
    $.get('/Home/GetInmuebleDetails', { id: inmuebleId })
        .done(function(data) {
            $('#inmuebleModalContainer').html(data);
            
            // Mostrar el modal
            const modal = new bootstrap.Modal(document.getElementById('inmuebleDetailsModal'));
            modal.show();
            
            // Inicializar el manager del modal
            if (window.modalInmuebleData) {
                modalInmuebleManager = new ModalInmuebleManager(
                    window.modalInmuebleData.id,
                    window.modalInmuebleData.latitud,
                    window.modalInmuebleData.longitud,
                    window.modalInmuebleData.direccion,
                    window.modalInmuebleData.localidad,
                    window.modalInmuebleData.provincia
                );
            }

            // Limpiar el contenedor cuando se cierre el modal
            $('#inmuebleDetailsModal').on('hidden.bs.modal', function() {
                $('#inmuebleModalContainer').empty();
                modalInmuebleManager = null;
            });
        })
        .fail(function(xhr) {
            let errorMessage = 'Error al cargar los detalles del inmueble.';
            if (xhr.status === 404) {
                errorMessage = 'El inmueble solicitado no está disponible.';
            }
            
            $('#inmuebleModalContainer').html(`
                <div class="modal fade show" style="display: block; background: rgba(0,0,0,0.5);">
                    <div class="modal-dialog modal-dialog-centered">
                        <div class="modal-content">
                            <div class="modal-header bg-danger text-white">
                                <h5 class="modal-title">Error</h5>
                                <button type="button" class="btn-close btn-close-white" onclick="cerrarModalError()"></button>
                            </div>
                            <div class="modal-body text-center">
                                <i class="fas fa-exclamation-triangle fa-3x text-danger mb-3"></i>
                                <h5>${errorMessage}</h5>
                                <p class="text-muted">Por favor, intenta nuevamente más tarde.</p>
                            </div>
                            <div class="modal-footer">
                                <button type="button" class="btn btn-secondary" onclick="cerrarModalError()">Cerrar</button>
                            </div>
                        </div>
                    </div>
                </div>
            `);
        });
}

// Función para cerrar modal de error
function cerrarModalError() {
    $('#inmuebleModalContainer').empty();
}

// Manager para el modal de inmueble
class ModalInmuebleManager {
    constructor(inmuebleId, latitud, longitud, direccion, localidad, provincia) {
        this.inmuebleId = inmuebleId;
        this.latitud = latitud;
        this.longitud = longitud;
        this.direccion = direccion;
        this.localidad = localidad;
        this.provincia = provincia;
        this.map = null;
        this.init();
    }

    init() {
        console.log('Inicializando ModalInmuebleManager para inmueble:', this.inmuebleId);
        this.cargarImagenes();
        this.initializeGoogleMaps();
    }

    cargarImagenes() {
        $.get('/Home/GetInmuebleImagenes', { id: this.inmuebleId })
            .done((data) => {
                $('#modal-imagenes-container').html(data);
            })
            .fail(() => {
                $('#modal-imagenes-container').html(`
                    <div class="text-center py-4 text-danger">
                        <i class="fas fa-exclamation-triangle fa-2x mb-2"></i>
                        <p>Error al cargar las imágenes</p>
                    </div>
                `);
            });
    }

    initializeGoogleMaps() {
        if (this.latitud && this.longitud) {
            if (window.googleMapsService) {
                window.googleMapsService.loadGoogleMaps(() => this.initMap());
            } else {
                console.warn('GoogleMapsService no está disponible');
                $('#modal-map').html(`
                    <div class="d-flex align-items-center justify-content-center h-100 bg-light rounded">
                        <div class="text-center text-muted">
                            <i class="fas fa-map fa-2x mb-2"></i>
                            <p class="mb-0">Mapa no disponible</p>
                        </div>
                    </div>
                `);
            }
        }
    }

    initMap() {
        try {
            const location = { lat: parseFloat(this.latitud), lng: parseFloat(this.longitud) };
            
            this.map = new google.maps.Map(document.getElementById('modal-map'), {
                zoom: 16,
                center: location,
                mapTypeId: google.maps.MapTypeId.ROADMAP,
                styles: [
                    {
                        featureType: 'poi',
                        elementType: 'labels',
                        stylers: [{ visibility: 'off' }]
                    }
                ]
            });
            
            const marker = new google.maps.Marker({
                position: location,
                map: this.map,
                title: this.direccion,
                animation: google.maps.Animation.DROP
            });

            const infoContent = `
                <div style="max-width: 200px;">
                    <strong>${this.direccion}</strong><br>
                    ${this.localidad ? this.localidad : ''}
                    ${this.provincia ? ', ' + this.provincia : ''}
                </div>
            `;

            const infoWindow = new google.maps.InfoWindow({
                content: infoContent
            });

            marker.addListener('click', () => {
                infoWindow.open(this.map, marker);
            });

            // Mostrar info window por defecto
            setTimeout(() => {
                infoWindow.open(this.map, marker);
            }, 500);

        } catch (error) {
            console.error('Error inicializando Google Maps:', error);
            $('#modal-map').html(`
                <div class="d-flex align-items-center justify-content-center h-100 bg-light rounded">
                    <div class="text-center text-muted">
                        <i class="fas fa-exclamation-triangle fa-2x mb-2"></i>
                        <p class="mb-0">Error al cargar el mapa</p>
                    </div>
                </div>
            `);
        }
    }
}

// Funciones de contacto (placeholder)
function contactarInmobiliaria() {
    alert('Funcionalidad de contacto en desarrollo.\n\nPor favor, contacta directamente con la inmobiliaria García Jesús para más información.');
}

function solicitarVisita(inmuebleId) {
    alert(`Funcionalidad de solicitud de visita en desarrollo.\n\nInmueble ID: ${inmuebleId}\n\nPor favor, contacta directamente con la inmobiliaria para coordinar una visita.`);
}
