// Empleado delete functionality
class EmpleadoDeleteManager {
    constructor() {
        this.init();
    }

    init() {
        $(document).ready(() => {
            this.bindConfirmationToggle();
            this.bindFormSubmission();
            console.log('EmpleadoDeleteManager initialized');
        });
    }

    bindConfirmationToggle() {
        $('#confirmBaja').change((e) => {
            $('#btnConfirmar').prop('disabled', !e.target.checked);
        });
    }

    bindFormSubmission() {
        $('form').submit((e) => {
            if (!confirm('¿Está seguro que desea dar de baja a este empleado? Esta acción desactivará su acceso al sistema.')) {
                e.preventDefault();
                return false;
            }
        });
    }
}

// Initialize when DOM is loaded
$(document).ready(function() {
    new EmpleadoDeleteManager();
});
