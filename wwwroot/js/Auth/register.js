// Auth register functionality
class AuthRegisterManager {
    constructor() {
        this.emailTimeout = null;
        this.usernameTimeout = null;
        this.isMultiRoleMode = false;
        this.existingUserData = null;
        this.init();
    }

    init() {
        $(document).ready(() => {
            this.bindUserTypeToggle();
            this.bindPasswordToggles();
            this.bindEmailValidation();
            this.bindUsernameValidation();
            this.bindExistingAccountValidation();
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
        const validateEmail = () => {
            const email = $('#Email').val();
            const rol = $('#tipoUsuario').val();
            const helpDiv = $('#emailHelp');
            
            if (!email || !rol || email.indexOf('@') === -1) {
                helpDiv.html('');
                $('#existingAccountValidation').hide();
                return;
            }

            clearTimeout(this.emailTimeout);
            
            this.emailTimeout = setTimeout(() => {
                helpDiv.html('<i class="fas fa-spinner fa-spin"></i> Verificando...').removeClass('text-success text-danger text-warning');
                
                $.post('/Auth/CheckEmailAvailability', { email: email, rol: rol })
                    .done((data) => {
                        if (data.available) {
                            // Email disponible para registro normal
                            helpDiv.html('<i class="fas fa-check text-success"></i> Email disponible')
                                   .removeClass('text-danger text-warning').addClass('text-success');
                            $('#existingAccountValidation').hide();
                            this.isMultiRoleMode = false;
                        } else if (data.existingEmail) {
                            // Email existe con diferente rol - activar modo multi-rol
                            helpDiv.html('<i class="fas fa-info-circle text-warning"></i> Este email ya está registrado con otro rol')
                                   .removeClass('text-success text-danger').addClass('text-warning');
                            $('#existingAccountValidation').show();
                        } else if (data.sameRole) {
                            // Email existe con mismo rol - error
                            helpDiv.html('<i class="fas fa-times text-danger"></i> Ya existe una cuenta con este email y rol')
                                   .removeClass('text-success text-warning').addClass('text-danger');
                            $('#existingAccountValidation').hide();
                        } else {
                            // Email no disponible (caso genérico)
                            helpDiv.html('<i class="fas fa-times text-danger"></i> Email ya registrado')
                                   .removeClass('text-success text-warning').addClass('text-danger');
                            $('#existingAccountValidation').hide();
                        }
                    })
                    .fail(() => {
                        helpDiv.html('<i class="fas fa-exclamation-triangle text-danger"></i> Error al verificar')
                               .removeClass('text-success text-warning').addClass('text-danger');
                    });
            }, 800);
        };

        // Validar en tiempo real mientras escribe
        $('#Email').on('input', validateEmail);
        
        // Validar también al salir del campo
        $('#Email').on('blur', validateEmail);
        
        // Re-validar email cuando cambia el tipo de usuario
        $('#tipoUsuario').on('change', () => {
            const email = $('#Email').val();
            $('#existingAccountValidation').hide();
            this.isMultiRoleMode = false;
            
            if (email && email.indexOf('@') > -1) {
                validateEmail();
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

    bindExistingAccountValidation() {
        // Toggle para mostrar/ocultar contraseña de cuenta existente
        $('#toggleExistingPassword').click(() => {
            const field = document.getElementById('ExistingPassword');
            const icon = document.querySelector('#toggleExistingPassword i');
            
            if (field.type === 'password') {
                field.type = 'text';
                icon.classList.remove('fa-eye');
                icon.classList.add('fa-eye-slash');
            } else {
                field.type = 'password';
                icon.classList.remove('fa-eye-slash');
                icon.classList.add('fa-eye');
            }
        });

        // Validar cuenta existente
        $('#validateExistingAccount').click(() => {
            const email = $('#Email').val();
            const rol = $('#tipoUsuario').val();
            const password = $('#ExistingPassword').val();
            const helpDiv = $('#existingPasswordHelp');
            
            if (!password) {
                helpDiv.html('<i class="fas fa-times text-danger"></i> Ingrese su contraseña')
                       .removeClass('text-success').addClass('text-danger');
                return;
            }

            helpDiv.html('<i class="fas fa-spinner fa-spin"></i> Validando...')
                   .removeClass('text-success text-danger').addClass('text-info');
            
            $.post('/Auth/ValidateExistingEmailForMultiRole', { email, rol, password })
                .done((response) => {
                    if (response.success && response.data) {
                        helpDiv.html('<i class="fas fa-check text-success"></i> Validación exitosa')
                               .removeClass('text-danger text-info').addClass('text-success');
                        
                        // Almacenar datos del usuario existente
                        this.existingUserData = response.data;
                        this.isMultiRoleMode = true;
                        
                        // Autocompletar campos
                        this.autofillExistingUserData(response.data);
                        
                        // Ocultar sección de validación y mostrar resumen
                        setTimeout(() => {
                            $('#existingAccountValidation').html(
                                '<div class="alert alert-success">' +
                                '<i class="fas fa-check-circle"></i> Cuenta validada. ' +
                                'Completamos sus datos automáticamente. Por favor, elija un nombre de usuario diferente.' +
                                '</div>'
                            );
                        }, 1000);
                    } else if (response.emailNotFound) {
                        helpDiv.html('<i class="fas fa-info-circle text-info"></i> Email no registrado, continúe con el formulario')
                               .removeClass('text-danger text-success').addClass('text-info');
                        $('#existingAccountValidation').hide();
                    } else {
                        helpDiv.html('<i class="fas fa-times text-danger"></i> ' + (response.error || 'Error de validación'))
                               .removeClass('text-success text-info').addClass('text-danger');
                    }
                })
                .fail(() => {
                    helpDiv.html('<i class="fas fa-times text-danger"></i> Error al validar')
                           .removeClass('text-success text-info').addClass('text-danger');
                });
        });
    }

    autofillExistingUserData(data) {
        // Autocompletar datos personales
        $('#Nombre').val(data.nombre).prop('readonly', true).addClass('bg-light');
        $('#Apellido').val(data.apellido).prop('readonly', true).addClass('bg-light');
        $('#Dni').val(data.dni).prop('readonly', true).addClass('bg-light');
        $('#Telefono').val(data.telefono).prop('readonly', true).addClass('bg-light');
        
        // Sugerir username
        $('#NombreUsuario').val(data.suggestedUsername);
        $('#NombreUsuario').trigger('input'); // Validar disponibilidad
        
        // Ocultar campos de contraseña (se usará la existente)
        $('#Password').closest('.row').hide();
        $('#ConfirmPassword').closest('.col-md-6').hide();
        
        // Agregar campo hidden con flag de multirol
        if ($('#isMultiRole').length === 0) {
            $('#registerForm').append('<input type="hidden" id="isMultiRole" name="isMultiRole" value="true" />');
            $('#registerForm').append('<input type="hidden" id="existingPassword" name="existingPassword" value="' + $('#ExistingPassword').val() + '" />');
        }
    }
}

// Initialize when DOM is loaded
$(document).ready(function() {
    new AuthRegisterManager();
});
