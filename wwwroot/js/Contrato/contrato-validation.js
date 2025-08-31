// Contract date validation functionality
class ContratoDateValidator {
    constructor() {
        this.unavailableDates = [];
        this.init();
    }

    init() {
        const inmuebleSelect = document.getElementById('InmuebleId');
        const fechaInicioInput = document.getElementById('FechaInicio');
        const fechaFinInput = document.getElementById('FechaFin');

        if (inmuebleSelect && fechaInicioInput && fechaFinInput) {
            // Set minimum date to today (only for create, not edit)
            if (!document.querySelector('input[name="Id"]')) {
                const today = new Date().toISOString().split('T')[0];
                fechaInicioInput.min = today;
                fechaFinInput.min = today;
            }

            // Load initial data if property is already selected (only for create, not edit)
            if (inmuebleSelect.value && !document.querySelector('input[name="Id"]')) {
                this.loadUnavailableDates(inmuebleSelect.value);
            }

            // Event listeners
            inmuebleSelect.addEventListener('change', (e) => {
                if (e.target.value) {
                    this.loadUnavailableDates(e.target.value);
                    this.suggestNextAvailableDate(e.target.value);
                } else {
                    this.clearValidation();
                }
            });

            fechaInicioInput.addEventListener('change', () => {
                this.validateDateSelection();
                this.updateEndDateMin();
            });

            fechaFinInput.addEventListener('change', () => {
                this.validateDateSelection();
            });
        }
    }

    async loadUnavailableDates(inmuebleId) {
        try {
            const response = await fetch(`/api/ContratoApi/unavailable-dates/${inmuebleId}`);
            const data = await response.json();
            
            if (data.error) {
                console.error('Error loading unavailable dates:', data.error);
                return;
            }

            this.unavailableDates = data;
            this.validateDateSelection();
        } catch (error) {
            console.error('Error loading unavailable dates:', error);
        }
    }

    async suggestNextAvailableDate(inmuebleId) {
        try {
            const response = await fetch(`/api/ContratoApi/next-available-date/${inmuebleId}`);
            const data = await response.json();
            
            if (data.error) {
                console.error('Error loading next available date:', data.error);
                return;
            }

            if (data.date) {
                const fechaInicioInput = document.getElementById('FechaInicio');
                const suggestionDiv = document.getElementById('fecha-suggestion');
                
                if (fechaInicioInput && !fechaInicioInput.value) {
                    fechaInicioInput.value = data.date;
                    this.updateEndDateMin();
                }

                if (suggestionDiv) {
                    suggestionDiv.innerHTML = `<small class="text-info"><i class="fas fa-info-circle"></i> Próxima fecha disponible: ${this.formatDate(data.date)}</small>`;
                }
            }
        } catch (error) {
            console.error('Error loading next available date:', error);
        }
    }

    validateDateSelection() {
        const fechaInicioInput = document.getElementById('FechaInicio');
        const fechaFinInput = document.getElementById('FechaFin');

        // Always validate start date when it's selected
        if (fechaInicioInput.value) {
            this.validateStartDate();
        }

        // Validate full range only when both dates are selected
        if (fechaInicioInput.value && fechaFinInput.value) {
            return this.validateDateRange();
        }

        return true;
    }

    validateStartDate() {
        const fechaInicioInput = document.getElementById('FechaInicio');
        const startDate = new Date(fechaInicioInput.value);
        
        // Check if start date is in the past
        const today = new Date();
        today.setHours(0, 0, 0, 0);
        
        if (startDate < today) {
            this.showStartDateMessage('La fecha de inicio no puede ser anterior a hoy', 'error');
            return false;
        }

        // Check if start date overlaps with existing contracts
        const hasStartOverlap = this.unavailableDates.some(period => {
            const periodStart = new Date(period.start);
            const periodEnd = new Date(period.end);
            return startDate >= periodStart && startDate <= periodEnd;
        });

        if (hasStartOverlap) {
            this.showStartDateMessage('Fecha ocupada - no disponible', 'error');
            return false;
        }

        this.showStartDateMessage('Fecha disponible', 'success');
        return true;
    }

    validateDateRange() {
        const fechaInicioInput = document.getElementById('FechaInicio');
        const fechaFinInput = document.getElementById('FechaFin');
        const startDate = new Date(fechaInicioInput.value);
        const endDate = new Date(fechaFinInput.value);

        // Check if end date is after start date
        if (endDate <= startDate) {
            this.showValidationMessage('La fecha de fin debe ser posterior a la fecha de inicio', 'error');
            return false;
        }

        // Check if dates are not in the past
        const today = new Date();
        today.setHours(0, 0, 0, 0);
        
        if (endDate < today) {
            this.showValidationMessage('La fecha de fin no puede ser anterior a hoy', 'error');
            return false;
        }

        // Check if date range overlaps with existing contracts
        const hasOverlap = this.checkDateOverlap(startDate, endDate);
        
        if (hasOverlap) {
            this.showValidationMessage('Las fechas seleccionadas se superponen con un contrato existente', 'error');
            return false;
        }

        this.showValidationMessage('Las fechas seleccionadas están disponibles', 'success');
        return true;
    }

    checkDateOverlap(startDate, endDate) {
        return this.unavailableDates.some(period => {
            const periodStart = new Date(period.start);
            const periodEnd = new Date(period.end);
            
            return (
                (startDate >= periodStart && startDate <= periodEnd) ||
                (endDate >= periodStart && endDate <= periodEnd) ||
                (startDate <= periodStart && endDate >= periodEnd)
            );
        });
    }

    updateEndDateMin() {
        const fechaInicioInput = document.getElementById('FechaInicio');
        const fechaFinInput = document.getElementById('FechaFin');
        
        if (fechaInicioInput.value) {
            const startDate = new Date(fechaInicioInput.value);
            startDate.setDate(startDate.getDate() + 1);
            fechaFinInput.min = startDate.toISOString().split('T')[0];
        }
    }

    showValidationMessage(message, type) {
        const validationDiv = document.getElementById('date-validation-message');
        if (validationDiv) {
            const alertClass = type === 'error' ? 'alert-danger' : 'alert-success';
            const icon = type === 'error' ? 'fas fa-exclamation-triangle' : 'fas fa-check-circle';
            
            validationDiv.innerHTML = `
                <div class="alert ${alertClass} alert-dismissible fade show" role="alert">
                    <i class="${icon}"></i> ${message}
                    <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
                </div>
            `;
        }
    }

    showStartDateMessage(message, type) {
        const fechaInicioInput = document.getElementById('FechaInicio');
        const existingMessage = fechaInicioInput.parentNode.querySelector('.start-date-message');
        
        // Remove existing message
        if (existingMessage) {
            existingMessage.remove();
        }

        // Create new message element
        const messageElement = document.createElement('small');
        messageElement.className = `form-text start-date-message ${type === 'error' ? 'text-danger' : 'text-success'}`;
        messageElement.innerHTML = `<i class="${type === 'error' ? 'fas fa-exclamation-triangle' : 'fas fa-check-circle'}"></i> ${message}`;
        
        // Insert after the input
        fechaInicioInput.parentNode.appendChild(messageElement);
    }

    clearValidationMessage() {
        const validationDiv = document.getElementById('date-validation-message');
        if (validationDiv) {
            validationDiv.innerHTML = '';
        }
    }

    clearValidation() {
        this.unavailableDates = [];
        this.clearValidationMessage();
        
        const suggestionDiv = document.getElementById('fecha-suggestion');
        if (suggestionDiv) {
            suggestionDiv.innerHTML = '';
        }

        // Clear start date message
        const fechaInicioInput = document.getElementById('FechaInicio');
        if (fechaInicioInput) {
            const existingMessage = fechaInicioInput.parentNode.querySelector('.start-date-message');
            if (existingMessage) {
                existingMessage.remove();
            }
        }
    }

    formatDate(dateString) {
        const date = new Date(dateString);
        return date.toLocaleDateString('es-ES', {
            year: 'numeric',
            month: 'long',
            day: 'numeric'
        });
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    new ContratoDateValidator();
});
