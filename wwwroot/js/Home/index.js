$(document).ready(function() {
    // Inicializar dropdowns de provincia y localidad
    var provinciaActual = $('#provinciaActual').val() || 'San Luis';
    var localidadActual = $('#localidadActual').val() || 'San Luis';
    
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
    $('#provinciaFilter').val('San Luis');
    $('#localidadFilter').val('San Luis');
    $('input[name="fechaDesde"]').val('');
    $('input[name="fechaHasta"]').val('');
    
    // Reset price inputs to original range
    const minPrice = parseFloat($('#precioMinimo').val()) || 0;
    const maxPrice = parseFloat($('#precioMaximo').val()) || 1000000;
    $('#precioMin').val(minPrice);
    $('#precioMax').val(maxPrice);
    
    // Recargar localidades para San Luis
    georefService.cargarLocalidades('San Luis', document.getElementById('localidadFilter'), 'San Luis');
    
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
    // Redirigir a la página de detalles del inmueble
    window.location.href = '/Inmuebles/Details/' + inmuebleId;
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
