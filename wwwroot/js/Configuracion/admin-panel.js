// Configuracion admin panel functionality
class ConfiguracionAdminPanelManager {
    constructor() {
        this.init();
    }

    init() {
        $(document).ready(() => {
            this.bindMesesToggle();
            this.bindSaveConfiguration();
            console.log('ConfiguracionAdminPanelManager initialized');
        });
    }

    bindMesesToggle() {
        $('.mes-toggle').click((e) => {
            const $btn = $(e.target);
            const isEnabled = $btn.data('enabled');
            
            if (isEnabled) {
                $btn.removeClass('btn-success').addClass('btn-secondary');
                $btn.data('enabled', false);
            } else {
                $btn.removeClass('btn-secondary').addClass('btn-success');
                $btn.data('enabled', true);
            }
        });
    }

    bindSaveConfiguration() {
        $('#guardarMesesMinimos').click(() => {
            const mesesHabilitados = [];
            $('.mes-toggle').each(function() {
                if ($(this).data('enabled')) {
                    mesesHabilitados.push(parseInt($(this).data('mes')));
                }
            });

            $.ajax({
                url: '/Configuracion/UpdateMesesMinimos',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(mesesHabilitados),
                success: (response) => {
                    if (response.success) {
                        this.showAlert('success', response.message);
                    } else {
                        this.showAlert('danger', response.message);
                    }
                },
                error: () => {
                    this.showAlert('danger', 'Error al guardar la configuración');
                }
            });
        });
    }

    updateConfiguracion(clave, inputId, descripcion, tipo) {
        const valor = $('#' + inputId).val();
        
        $.ajax({
            url: '/Configuracion/UpdateConfiguracion',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                Clave: clave,
                Valor: valor,
                Descripcion: descripcion,
                Tipo: tipo
            }),
            success: (response) => {
                if (response.success) {
                    this.showAlert('success', response.message);
                } else {
                    this.showAlert('danger', response.message);
                }
            },
            error: () => {
                this.showAlert('danger', 'Error al actualizar la configuración');
            }
        });
    }

    showAlert(type, message) {
        const alertHtml = `
            <div class="alert alert-${type} alert-dismissible fade show" role="alert">
                <i class="fas fa-${type === 'success' ? 'check-circle' : 'exclamation-circle'}"></i> ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        `;
        
        $('.container-fluid .row:first .col-12').after(alertHtml);
        
        // Auto-dismiss after 5 seconds
        setTimeout(() => {
            $('.alert').fadeOut();
        }, 5000);
    }
}

// Global functions for backward compatibility
let configuracionAdminManager;

function updateConfiguracion(clave, inputId, descripcion, tipo) {
    if (configuracionAdminManager) {
        configuracionAdminManager.updateConfiguracion(clave, inputId, descripcion, tipo);
    }
}

// Initialize when DOM is loaded
$(document).ready(function() {
    configuracionAdminManager = new ConfiguracionAdminPanelManager();
});
