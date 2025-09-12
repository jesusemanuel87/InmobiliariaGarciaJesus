// InquilinoUtils - Utility functions for Inquilinos module
class InquilinoUtils {
    
    // Format validation errors for display
    static displayValidationErrors(errors) {
        // Clear previous errors
        $('.is-invalid').removeClass('is-invalid');
        $('.text-danger').remove();

        // Display new errors
        for (const [field, messages] of Object.entries(errors)) {
            const input = $(`#${field.toLowerCase()}`);
            if (input.length) {
                input.addClass('is-invalid');
                const errorDiv = $('<div class="text-danger"></div>').text(messages[0]);
                input.after(errorDiv);
            }
        }
    }

    // Show alert messages
    static showAlert(type, message) {
        const alertHtml = `
            <div class="alert alert-${type} alert-dismissible fade show" role="alert">
                <i class="fas fa-${type === 'success' ? 'check-circle' : 'exclamation-triangle'}"></i>
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        `;
        
        $('#alertContainer').html(alertHtml);
        
        // Auto dismiss after 5 seconds
        setTimeout(() => {
            $('.alert').fadeOut();
        }, 5000);
    }

    // Format date for display
    static formatDate(dateString) {
        if (!dateString) return '';
        
        const date = new Date(dateString);
        return date.toLocaleDateString('es-ES', {
            year: 'numeric',
            month: '2-digit',
            day: '2-digit',
            hour: '2-digit',
            minute: '2-digit'
        });
    }

    // Validate form data before submission
    static validateFormData(formData) {
        const errors = {};

        if (!formData.Nombre || formData.Nombre.trim() === '') {
            errors.nombre = ['El nombre es obligatorio'];
        }

        if (!formData.Apellido || formData.Apellido.trim() === '') {
            errors.apellido = ['El apellido es obligatorio'];
        }

        if (!formData.Dni || formData.Dni.trim() === '') {
            errors.dni = ['El DNI es obligatorio'];
        }

        if (!formData.Email || formData.Email.trim() === '') {
            errors.email = ['El email es obligatorio'];
        } else if (!InquilinoUtils.isValidEmail(formData.Email)) {
            errors.email = ['El formato del email no es válido'];
        }

        return {
            isValid: Object.keys(errors).length === 0,
            errors: errors
        };
    }

    // Email validation
    static isValidEmail(email) {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return emailRegex.test(email);
    }

    // Clean form data
    static cleanFormData(formData) {
        const cleaned = { ...formData };
        
        // Trim string fields
        ['Nombre', 'Apellido', 'Dni', 'Email', 'Telefono', 'Direccion'].forEach(field => {
            if (cleaned[field] && typeof cleaned[field] === 'string') {
                cleaned[field] = cleaned[field].trim();
            }
        });

        return cleaned;
    }

    // Setup form validation events
    static setupFormValidation(formSelector) {
        $(formSelector).find('input, textarea').on('input', function() {
            $(this).removeClass('is-invalid');
            $(this).next('.text-danger').remove();
        });
    }
}
