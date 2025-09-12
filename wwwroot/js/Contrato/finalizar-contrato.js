// Contract finalization functionality
class ContratoFinalizarManager {
    constructor() {
        this.contratoIdActual = 0;
        this.init();
    }

    init() {
        $(document).ready(() => {
            this.bindEvents();
            console.log('ContratoFinalizarManager initialized');
        });
    }

    bindEvents() {
        // Form submission handler
        $('form').on('submit', (e) => {
            this.handleFormSubmit(e);
        });

        // Make procesarPago function available globally
        window.procesarPago = (pagar) => {
            this.procesarPago(pagar);
        };
    }

    handleFormSubmit(event) {
        event.preventDefault();
        
        const formData = $(event.target).serialize();
        
        $.ajax({
            url: '/Contratos/Finalizar',
            type: 'POST',
            data: formData,
            success: (response) => {
                this.handleFinalizationResponse(response);
            },
            error: () => {
                alert('Error al procesar la solicitud');
            }
        });
    }

    handleFinalizationResponse(response) {
        // Log for browser console
        if (response.logData) {
            console.log('[FINALIZAR CONTRATO]', response.logData);
        }
        
        if (response.success) {
            this.contratoIdActual = response.contratoId;
            
            // Show summary in modal
            let resumen = '<ul class="mb-2">';
            if (response.multaTerminacion > 0) {
                resumen += '<li><strong>Multa por Terminación:</strong> <span class="text-danger">$' + response.multaTerminacion.toLocaleString() + '</span></li>';
            }
            if (response.importeAdeudado > 0) {
                resumen += '<li><strong>Deuda Pendiente:</strong> <span class="text-warning">$' + response.importeAdeudado.toLocaleString() + '</span></li>';
            }
            resumen += '<li><strong>Total a Pagar:</strong> <span class="text-primary fs-5">$' + response.multaTotal.toLocaleString() + '</span></li>';
            resumen += '</ul>';
            
            $('#resumenPago').html(resumen);
            $('#modalConfirmacionPago').modal('show');
        } else {
            alert('Error: ' + response.message);
        }
    }

    procesarPago(pagar) {
        $.ajax({
            url: '/Contratos/ProcesarPagoFinalizacion',
            type: 'POST',
            data: {
                contratoId: this.contratoIdActual,
                procesarPago: pagar
            },
            success: () => {
                window.location.href = '/Contratos';
            },
            error: () => {
                alert('Error al procesar el pago');
            }
        });
    }
}

// Initialize when DOM is loaded
$(document).ready(function() {
    new ContratoFinalizarManager();
});
