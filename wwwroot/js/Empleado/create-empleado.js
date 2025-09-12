// Empleado creation functionality
class EmpleadoCreateManager {
    constructor() {
        this.emailTimeout = null;
        this.dniTimeout = null;
        this.usernameTimeout = null;
        this.init();
    }

    init() {
        $(document).ready(() => {
            this.bindPasswordToggle();
            this.bindEmailValidation();
            this.bindDniValidation();
            this.bindUsernameValidation();
            this.bindUsernameGeneration();
            this.setDefaultDate();
            console.log('EmpleadoCreateManager initialized');
        });
    }

    bindPasswordToggle() {
        $('#togglePassword').click(() => {
            const passwordField = document.getElementById('password');
            const icon = document.querySelector('#togglePassword i');
            
            if (passwordField.type === 'password') {
                passwordField.type = 'text';
                icon.classList.remove('fa-eye');
                icon.classList.add('fa-eye-slash');
            } else {
                passwordField.type = 'password';
                icon.classList.remove('fa-eye-slash');
                icon.classList.add('fa-eye');
            }
        });
    }

    bindEmailValidation() {
        $('#Email').on('input', (e) => {
            const email = $(e.target).val();
            const helpDiv = $('#emailHelp');
            
            clearTimeout(this.emailTimeout);
            
            if (email.length > 0 && email.indexOf('@') > -1) {
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
            
            if (dni.length >= 7) {
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

    bindUsernameValidation() {
        $('#nombreUsuario').on('input', (e) => {
            const username = $(e.target).val();
            const helpDiv = $('#usernameHelp');
            
            clearTimeout(this.usernameTimeout);
            
            if (username.length >= 3) {
                this.usernameTimeout = setTimeout(() => {
                    $.post('/Auth/CheckUsernameAvailability', { username: username })
                        .done((data) => {
                            if (data.available) {
                                helpDiv.html('<i class="fas fa-check text-success"></i> Nombre de usuario disponible')
                                       .removeClass('text-danger').addClass('text-success');
                            } else {
                                helpDiv.html('<i class="fas fa-times text-danger"></i> Nombre de usuario ya está en uso')
                                       .removeClass('text-success').addClass('text-danger');
                            }
                        });
                }, 500);
            } else {
                helpDiv.html('');
            }
        });
    }

    bindUsernameGeneration() {
        $('#Nombre, #Apellido').on('input', () => {
            const nombre = $('#Nombre').val().toLowerCase();
            const apellido = $('#Apellido').val().toLowerCase();
            
            if (nombre && apellido) {
                const username = nombre.charAt(0) + apellido;
                $('#nombreUsuario').val(username);
                $('#nombreUsuario').trigger('input'); // Trigger validation
            }
        });
    }

    setDefaultDate() {
        $('#FechaIngreso').val(new Date().toISOString().split('T')[0]);
    }
}

// Initialize when DOM is loaded
$(document).ready(function() {
    new EmpleadoCreateManager();
});
