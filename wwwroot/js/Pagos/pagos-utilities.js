// Pagos Utilities
class PagosUtilities {
    static showToast(message, type = 'success') {
        const toast = document.getElementById('successToast');
        if (!toast) {
            console.warn('Toast element not found');
            alert(message);
            return;
        }
        
        const toastHeader = toast.querySelector('.toast-header');
        const toastMessage = document.getElementById('toastMessage');
        
        // Configure toast type
        if (type === 'error') {
            toastHeader.className = 'toast-header bg-danger text-white';
            toastHeader.querySelector('i').className = 'fas fa-exclamation-circle me-2';
            toastHeader.querySelector('strong').textContent = 'Error';
        } else {
            toastHeader.className = 'toast-header bg-success text-white';
            toastHeader.querySelector('i').className = 'fas fa-check-circle me-2';
            toastHeader.querySelector('strong').textContent = 'Éxito';
        }
        
        toastMessage.textContent = message;
        
        const bsToast = new bootstrap.Toast(toast);
        bsToast.show();
    }

    static displayValidationErrors(errors) {
        // Clear previous errors
        document.querySelectorAll('.is-invalid').forEach(el => {
            el.classList.remove('is-invalid');
        });
        document.querySelectorAll('.invalid-feedback').forEach(el => {
            el.remove();
        });

        // Display new errors
        for (const [field, messages] of Object.entries(errors)) {
            const input = document.querySelector(`[name="${field}"]`);
            if (input) {
                input.classList.add('is-invalid');
                
                const feedback = document.createElement('div');
                feedback.className = 'invalid-feedback';
                feedback.textContent = messages.join(', ');
                
                input.parentNode.appendChild(feedback);
            }
        }
    }

    static formatCurrency(amount) {
        return parseFloat(amount).toLocaleString('es-AR', {
            style: 'currency',
            currency: 'ARS',
            minimumFractionDigits: 2
        });
    }

    static formatDate(dateString) {
        const date = new Date(dateString);
        return date.toLocaleDateString('es-AR');
    }

    static isDateOverdue(dateString) {
        const date = new Date(dateString);
        const today = new Date();
        return date < today;
    }
}
