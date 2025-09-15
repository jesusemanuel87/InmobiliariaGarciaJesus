// Empleado edit functionality
class EmpleadoEditManager {
    constructor(originalDni, originalEmail) {
        this.originalDni = originalDni;
        this.originalEmail = originalEmail;
        this.emailTimeout = null;
        this.dniTimeout = null;
        this.init();
    }

    init() {
        $(document).ready(() => {
            this.bindEmailValidation();
            this.bindDniValidation();
            console.log('EmpleadoEditManager initialized');
        });
    }

    bindEmailValidation() {
        $('#Email').on('input', (e) => {
            const email = $(e.target).val();
            const helpDiv = $('#emailHelp');
            
            clearTimeout(this.emailTimeout);
            
            if (email !== this.originalEmail && email.length > 0 && email.indexOf('@') > -1) {
                this.emailTimeout = setTimeout(() => {
                    $.post('/Empleados/CheckEmailExists', { email: email })
                        .done((data) => {
                            if (data.exists) {
                                helpDiv.html('<i class="fas fa-times text-danger"></i> Email ya está en uso')
                                       .removeClass('text-success').addClass('text-danger');
                            } else {
                                helpDiv.html('<i class="fas fa-check text-success"></i> Email disponible')
                                       .removeClass('text-danger').addClass('text-success');
                            }
                        });
                }, 500);
            } else {
                helpDiv.html('');
            }
        });
    }

    bindDniValidation() {
        $('#Dni').on('input', (e) => {
            const dni = $(e.target).val();
            const helpDiv = $('#dniHelp');
            
            clearTimeout(this.dniTimeout);
            
            if (dni !== this.originalDni && dni.length >= 7) {
                this.dniTimeout = setTimeout(() => {
                    $.post('/Empleados/CheckDniExists', { dni: dni })
                        .done((data) => {
                            if (data.exists) {
                                helpDiv.html('<i class="fas fa-times text-danger"></i> DNI ya está en uso')
                                       .removeClass('text-success').addClass('text-danger');
                            } else {
                                helpDiv.html('<i class="fas fa-check text-success"></i> DNI disponible')
                                       .removeClass('text-danger').addClass('text-success');
                            }
                        });
                }, 500);
            } else {
                helpDiv.html('');
            }
        });
    }
}

// Global variable for manager instance
let empleadoEditManager;
