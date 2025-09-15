// Propietario index page functionality
class PropietarioIndexManager {
    constructor() {
        this.init();
    }

    init() {
        this.bindToggleInmueblesEvents();
        console.log('PropietarioIndexManager initialized');
    }

    bindToggleInmueblesEvents() {
        $(document).ready(() => {
            $('.toggle-inmuebles').click((e) => {
                this.handleToggleInmuebles(e);
            });
        });
    }

    handleToggleInmuebles(event) {
        const button = $(event.currentTarget);
        const propietarioId = button.data('propietario-id');
        const container = $('.inmuebles-container[data-propietario-id="' + propietarioId + '"]');
        const content = container.find('.inmuebles-content');
        const icon = button.find('.toggle-icon');
        
        if (container.is(':visible')) {
            // Hide inmuebles
            container.slideUp();
            icon.removeClass('fa-chevron-up').addClass('fa-chevron-down');
        } else {
            // Show inmuebles
            if (content.is(':empty')) {
                // Load inmuebles via AJAX
                content.html('<div class="text-center p-3"><i class="fas fa-spinner fa-spin"></i> Cargando inmuebles...</div>');
                
                $.get('/Propietarios/GetInmuebles', { id: propietarioId })
                    .done((data) => {
                        content.html('<table class="table table-sm mb-0">' + data + '</table>');
                        container.slideDown();
                        icon.removeClass('fa-chevron-down').addClass('fa-chevron-up');
                    })
                    .fail(() => {
                        content.html('<div class="text-center p-3 text-danger"><i class="fas fa-exclamation-triangle"></i> Error al cargar inmuebles</div>');
                        container.slideDown();
                        icon.removeClass('fa-chevron-down').addClass('fa-chevron-up');
                    });
            } else {
                container.slideDown();
                icon.removeClass('fa-chevron-down').addClass('fa-chevron-up');
            }
        }
    }
}

// Initialize when DOM is loaded
$(document).ready(function() {
    new PropietarioIndexManager();
});
