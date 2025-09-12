// Contract index page functionality
class ContratoIndexManager {
    constructor() {
        this.init();
    }

    init() {
        this.bindTogglePagosEvents();
        console.log('ContratoIndexManager initialized');
    }

    bindTogglePagosEvents() {
        $(document).ready(() => {
            $('.toggle-pagos').click((e) => {
                this.handleTogglePagos(e);
            });
        });
    }

    handleTogglePagos(event) {
        const button = $(event.currentTarget);
        const contratoId = button.data('contrato-id');
        const container = $('.pagos-container[data-contrato-id="' + contratoId + '"]');
        const content = container.find('.pagos-content');
        const icon = button.find('.toggle-icon');
        
        if (container.is(':visible')) {
            // Hide pagos
            container.slideUp();
            icon.removeClass('fa-chevron-up').addClass('fa-chevron-down');
        } else {
            // Show pagos
            if (content.is(':empty')) {
                // Load pagos via AJAX
                content.html('<tr><td colspan="8" class="text-center p-3"><i class="fas fa-spinner fa-spin"></i> Cargando pagos...</td></tr>');
                
                $.get('/Contratos/GetPagos', { id: contratoId })
                    .done((data) => {
                        content.html(data);
                        container.slideDown();
                        icon.removeClass('fa-chevron-down').addClass('fa-chevron-up');
                    })
                    .fail(() => {
                        content.html('<tr><td colspan="8" class="text-center p-3 text-danger"><i class="fas fa-exclamation-triangle"></i> Error al cargar pagos</td></tr>');
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
    new ContratoIndexManager();
});
