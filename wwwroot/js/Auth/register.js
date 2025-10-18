// Auth register functionality
class AuthRegisterManager {
    constructor() {
        this.emailTimeout = null;
        this.usernameTimeout = null;
        this.init();
    }

    init() {
        $(document).ready(() => {
            this.bindUserTypeToggle();
            this.bindPasswordToggles();
            this.bindEmailValidation();
            this.bindUsernameValidation();
            console.log('AuthRegisterManager initialized');
        });
    }

    bindUserTypeToggle() {
        $('#tipoUsuario').change((e) => {
            const selectedValue = $(e.target).val();
            if (selectedValue === '1' || selectedValue === '2') {
                $('#personalInfo').show();
                $('#Nombre, #Apellido, #Dni, #Telefono').attr('required', true);
            } else {
                $('#personalInfo').hide();
                $('#Nombre, #Apellido, #Dni, #Telefono').attr('required', false);
            }
        });
    }

    bindPasswordToggles() {
        $('#togglePassword1').click(() => {
            this.togglePasswordVisibility('Password', '#togglePassword1');
        });

        $('#togglePassword2').click(() => {
            this.togglePasswordVisibility('ConfirmPassword', '#togglePassword2');
        });
    }

    togglePasswordVisibility(fieldId, buttonSelector) {
        const field = document.getElementById(fieldId);
        const icon = document.querySelector(buttonSelector + ' i');
        
        if (field.type === 'password') {
            field.type = 'text';
            icon.classList.remove('fa-eye');
            icon.classList.add('fa-eye-slash');
        } else {
            field.type = 'password';
            icon.classList.remove('fa-eye-slash');
            icon.classList.add('fa-eye');
        }
    }

    bindEmailValidation() {
        $('#Email').on('input', (e) => {
            const email = $(e.target).val();
            const helpDiv = $('#emailHelp');
            
            clearTimeout(this.emailTimeout);
            
            if (email.length > 0 && email.indexOf('@') > -1) {
                this.emailTimeout = setTimeout(() => {
                    $.post('/Auth/CheckEmailAvailability', { email: email })
                        .done((data) => {
                            if (data.available) {
                                helpDiv.html('<i class="fas fa-check text-success"></i> Email disponible')
                                       .removeClass('text-danger').addClass('text-success');
                            } else {
                                helpDiv.html('<i class="fas fa-times text-danger"></i> Email ya registrado')
                                       .removeClass('text-success').addClass('text-danger');
                            }
                        })
                        .fail(() => {
                            helpDiv.html('');
                        });
                }, 500);
            } else {
                helpDiv.html('');
            }
        });
    }

    bindUsernameValidation() {
        $('#NombreUsuario').on('input', (e) => {
            const username = $(e.target).val();
            const helpDiv = $('#usernameHelp');
            
            clearTimeout(this.usernameTimeout);
            
            if (username.length >= 3) {
                this.usernameTimeout = setTimeout(() => {
                    $.post('/Auth/CheckUsernameAvailability', { username: username })
                        .done((data) => {
                            if (data.available) {
                                helpDiv.html('<i class="fas fa-check text-success"></i> Disponible')
                                       .removeClass('text-danger').addClass('text-success');
                            } else {
                                helpDiv.html('<i class="fas fa-times text-danger"></i> Ya está en uso')
                                       .removeClass('text-success').addClass('text-danger');
                            }
                        })
                        .fail(() => {
                            helpDiv.html('');
                        });
                }, 500);
            } else {
                helpDiv.html('');
            }
        });
    }
}

// Initialize when DOM is loaded
$(document).ready(function() {
    new AuthRegisterManager();
});
