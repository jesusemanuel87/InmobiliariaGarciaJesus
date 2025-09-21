// EmpleadoUtils - Utility functions for Empleados module
class EmpleadoUtils {
    
    // Show alert messages
    static showAlert(message, type = 'success', container = '#alertContainer') {
        const alertClass = type === 'success' ? 'alert-success' : 'alert-danger';
        const iconClass = type === 'success' ? 'fa-check-circle' : 'fa-exclamation-circle';
        
        const alertHtml = `
            <div class="alert ${alertClass} alert-dismissible fade show" role="alert">
                <i class="fas ${iconClass} me-2"></i>
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
            </div>
        `;
        
        $(container).html(alertHtml);
        
        // Auto-dismiss after 5 seconds
        setTimeout(() => {
            $(container + ' .alert').alert('close');
        }, 5000);
    }

    // Clear alerts
    static clearAlerts(container = '#alertContainer') {
        $(container).empty();
    }

    // Validate form data
    static validateEmpleadoForm(formData) {
        const errors = {};

        // DNI validation
        if (!formData.Dni || formData.Dni.trim() === '') {
            errors.Dni = ['El DNI es requerido'];
        } else if (!/^\d{7,8}$/.test(formData.Dni.trim())) {
            errors.Dni = ['El DNI debe tener entre 7 y 8 dígitos'];
        }

        // Nombre validation
        if (!formData.Nombre || formData.Nombre.trim() === '') {
            errors.Nombre = ['El nombre es requerido'];
        } else if (formData.Nombre.trim().length < 2) {
            errors.Nombre = ['El nombre debe tener al menos 2 caracteres'];
        }

        // Apellido validation
        if (!formData.Apellido || formData.Apellido.trim() === '') {
            errors.Apellido = ['El apellido es requerido'];
        } else if (formData.Apellido.trim().length < 2) {
            errors.Apellido = ['El apellido debe tener al menos 2 caracteres'];
        }

        // Email validation
        if (!formData.Email || formData.Email.trim() === '') {
            errors.Email = ['El email es requerido'];
        } else {
            const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (!emailRegex.test(formData.Email.trim())) {
                errors.Email = ['El formato del email no es válido'];
            }
        }

        // Telefono validation (optional but if provided, must be valid)
        if (formData.Telefono && formData.Telefono.trim() !== '') {
            if (!/^\d{10,15}$/.test(formData.Telefono.trim().replace(/[\s\-\(\)]/g, ''))) {
                errors.Telefono = ['El teléfono debe tener entre 10 y 15 dígitos'];
            }
        }

        // Rol validation
        if (!formData.Rol || formData.Rol.trim() === '') {
            errors.Rol = ['El rol es requerido'];
        }

        // Usuario y contraseña validation (solo requeridos para crear nuevo empleado)
        const isNewEmployee = !formData.Id || formData.Id === '0' || formData.Id === 0;
        
        if (isNewEmployee) {
            // Para empleados nuevos, usuario y contraseña son obligatorios
            if (!formData.NombreUsuario || formData.NombreUsuario.trim() === '') {
                errors.NombreUsuario = ['El nombre de usuario es requerido'];
            }
            
            if (!formData.Password || formData.Password.trim() === '') {
                errors.Password = ['La contraseña es requerida'];
            } else if (formData.Password.trim().length < 6) {
                errors.Password = ['La contraseña debe tener al menos 6 caracteres'];
            }
        } else {
            // Para empleados existentes, validar solo si se proporcionan
            if (formData.Password && formData.Password.trim() !== '' && formData.Password.trim().length < 6) {
                errors.Password = ['La contraseña debe tener al menos 6 caracteres'];
            }
        }

        return {
            isValid: Object.keys(errors).length === 0,
            errors: errors
        };
    }

    // Display validation errors
    static displayValidationErrors(errors) {
        // Clear previous errors
        $('.is-invalid').removeClass('is-invalid');
        $('.invalid-feedback').remove();

        // Display new errors
        Object.keys(errors).forEach(field => {
            const input = $(`[name="${field}"]`);
            if (input.length > 0) {
                input.addClass('is-invalid');
                
                const errorMessages = Array.isArray(errors[field]) ? errors[field] : [errors[field]];
                const errorHtml = errorMessages.map(msg => `<div class="invalid-feedback">${msg}</div>`).join('');
                
                input.after(errorHtml);
            }
        });
    }

    // Clear validation errors
    static clearValidationErrors() {
        $('.is-invalid').removeClass('is-invalid');
        $('.invalid-feedback').remove();
    }

    // Format date for display
    static formatDate(dateString) {
        if (!dateString) return '';
        
        const date = new Date(dateString);
        return date.toLocaleDateString('es-AR', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric'
        });
    }

    // Format datetime for display
    static formatDateTime(dateString) {
        if (!dateString) return '';
        
        const date = new Date(dateString);
        return date.toLocaleDateString('es-AR', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
    }

    // Get form data as object
    static getFormData(formSelector) {
        const formData = {};
        $(formSelector).find('input, select, textarea').each(function() {
            const $field = $(this);
            const name = $field.attr('name');
            if (name) {
                if ($field.attr('type') === 'checkbox') {
                    formData[name] = $field.is(':checked');
                } else {
                    formData[name] = $field.val();
                }
            }
        });
        return formData;
    }

    // Populate form with data
    static populateForm(formSelector, data) {
        Object.keys(data).forEach(key => {
            const $field = $(formSelector).find(`[name="${key}"]`);
            if ($field.length > 0) {
                if ($field.attr('type') === 'checkbox') {
                    $field.prop('checked', data[key]);
                } else {
                    $field.val(data[key]);
                }
            }
        });
    }

    // Reset form
    static resetForm(formSelector) {
        $(formSelector)[0].reset();
        this.clearValidationErrors();
    }
}
