// Contract creation functionality
class ContratoCreateManager {
    constructor() {
        this.configuraciones = [];
        this.inmuebles = [];
        this.mesesDisponibles = [];
        this.mesesSeleccionados = 12;
        this.init();
    }

    init() {
        // Wait for DOM and validation scripts to load
        setTimeout(() => {
            this.loadViewBagData();
            this.setupElements();
            this.bindEvents();
            console.log('ContratoCreateManager initialized');
        }, 500);
    }

    loadViewBagData() {
        // These will be populated by the view
        if (window.contratoCreateData) {
            this.configuraciones = window.contratoCreateData.configuraciones || [];
            this.inmuebles = window.contratoCreateData.inmuebles || [];
        }

        console.log('Configuraciones:', this.configuraciones);
        console.log('Inmuebles:', this.inmuebles);

        this.extractAvailableMonths();
    }

    extractAvailableMonths() {
        this.mesesDisponibles = [];
        
        if (this.configuraciones && this.configuraciones.length > 0) {
            this.configuraciones.forEach(config => {
                const match = config.clave.match(/MESES_MINIMOS_(\d+)/);
                if (match && config.valor.toLowerCase() === 'true') {
                    this.mesesDisponibles.push(parseInt(match[1]));
                }
            });

            // Sort and use the smallest as initial value
            this.mesesDisponibles.sort((a, b) => a - b);
            if (this.mesesDisponibles.length > 0) {
                this.mesesSeleccionados = this.mesesDisponibles[0];
            }
        }

        console.log('Meses disponibles:', this.mesesDisponibles);
        console.log('Meses seleccionados:', this.mesesSeleccionados);
    }

    setupElements() {
        this.fechaInicioEl = document.getElementById('fechaInicio');
        this.fechaFinEl = document.getElementById('fechaFin');
        this.inmuebleEl = document.getElementById('InmuebleId');
        this.precioEl = document.querySelector('input[name="Precio"]');

        // Verify elements exist
        if (!this.fechaInicioEl || !this.fechaFinEl || !this.inmuebleEl || !this.precioEl) {
            console.error('Required elements not found:', {
                fechaInicio: !!this.fechaInicioEl,
                fechaFin: !!this.fechaFinEl,
                inmueble: !!this.inmuebleEl,
                precio: !!this.precioEl
            });
            return false;
        }

        return true;
    }

    bindEvents() {
        if (!this.setupElements()) return;

        // Start date change event
        this.fechaInicioEl.addEventListener('change', () => {
            this.handleStartDateChange();
        });

        // Property selection change event
        this.inmuebleEl.addEventListener('change', () => {
            this.handlePropertyChange();
        });

        // Duration button events
        this.bindDurationButtons();
    }

    handleStartDateChange() {
        const fechaInicio = this.fechaInicioEl.value;
        console.log('Fecha de inicio cambiada:', fechaInicio);
        
        if (fechaInicio) {
            const mesesActivos = this.getActiveButtonMonths();
            const fechaFin = this.calculateEndDate(fechaInicio, mesesActivos);
            if (fechaFin) {
                this.fechaFinEl.value = fechaFin;
                console.log('Fecha de fin establecida:', fechaFin, 'con', mesesActivos, 'meses');
                
                // Trigger change event for validation
                this.fechaFinEl.dispatchEvent(new Event('change'));
            }
        }
    }

    handlePropertyChange() {
        const inmuebleId = parseInt(this.inmuebleEl.value);
        console.log('Inmueble seleccionado:', inmuebleId);
        
        if (inmuebleId && this.inmuebles && this.inmuebles.length > 0) {
            const inmueble = this.inmuebles.find(i => (i.id || i.Id) === inmuebleId);
            console.log('Inmueble encontrado:', inmueble);
            
            if (inmueble && inmueble.precio) {
                this.setPriceFromProperty(inmueble.precio);
            }
        }
    }

    setPriceFromProperty(precio) {
        this.precioEl.value = precio;
        
        // Show formatted price
        const precioFormateado = this.formatPrice(precio);
        
        // Remove existing formatted price
        const existingFormatted = this.precioEl.parentNode.querySelector('.precio-formateado');
        if (existingFormatted) {
            existingFormatted.remove();
        }
        
        // Add new formatted price display
        const formatElement = document.createElement('small');
        formatElement.className = 'form-text text-muted precio-formateado';
        formatElement.innerHTML = precioFormateado;
        this.precioEl.parentNode.appendChild(formatElement);
        
        console.log('Precio establecido:', precio, 'Formateado:', precioFormateado);
    }

    bindDurationButtons() {
        document.querySelectorAll('.duration-btn').forEach(button => {
            button.addEventListener('click', (e) => {
                this.handleDurationButtonClick(e.target);
            });
        });
    }

    handleDurationButtonClick(button) {
        const meses = parseInt(button.getAttribute('data-months'));
        const fechaInicio = this.fechaInicioEl.value;
        
        console.log('Botón clickeado, meses:', meses, 'fecha inicio:', fechaInicio);
        
        // Update button states
        this.updateButtonStates(button);
        
        if (fechaInicio) {
            const fechaFin = this.calculateEndDate(fechaInicio, meses);
            if (fechaFin) {
                this.fechaFinEl.value = fechaFin;
                console.log('Nueva fecha de fin:', fechaFin);
                
                // Trigger change event for validation
                this.fechaFinEl.dispatchEvent(new Event('change'));
            }
        } else {
            alert('Por favor, seleccione primero la fecha de inicio');
            this.fechaInicioEl.focus();
        }
    }

    updateButtonStates(activeButton) {
        // Remove active class from all buttons
        document.querySelectorAll('.duration-btn').forEach(btn => {
            btn.classList.remove('btn-primary');
            btn.classList.add('btn-outline-primary');
        });
        
        // Add active class to clicked button
        activeButton.classList.remove('btn-outline-primary');
        activeButton.classList.add('btn-primary');
    }

    getActiveButtonMonths() {
        const activeButton = document.querySelector('.duration-btn.btn-primary');
        if (activeButton) {
            return parseInt(activeButton.getAttribute('data-months'));
        }
        return this.mesesSeleccionados; // Fallback
    }

    calculateEndDate(fechaInicio, meses) {
        if (!fechaInicio || !meses) {
            console.log('Insufficient data to calculate date:', fechaInicio, meses);
            return '';
        }
        
        try {
            const fecha = new Date(fechaInicio + 'T00:00:00');
            if (isNaN(fecha.getTime())) {
                console.log('Invalid start date:', fechaInicio);
                return '';
            }
            
            fecha.setMonth(fecha.getMonth() + parseInt(meses));
            
            // Get last day of target month
            const ultimoDia = this.getLastDayOfMonth(fecha.getFullYear(), fecha.getMonth() + 1);
            fecha.setDate(ultimoDia);
            
            // Format date for input type="date" (YYYY-MM-DD)
            const result = fecha.getFullYear() + '-' + 
                   String(fecha.getMonth() + 1).padStart(2, '0') + '-' + 
                   String(fecha.getDate()).padStart(2, '0');
            
            console.log('Calculated date:', result);
            return result;
        } catch (error) {
            console.error('Error calculating date:', error);
            return '';
        }
    }

    getLastDayOfMonth(year, month) {
        return new Date(year, month, 0).getDate();
    }

    formatPrice(precio) {
        if (!precio) return '';
        return new Intl.NumberFormat('es-AR', {
            style: 'currency',
            currency: 'ARS',
            minimumFractionDigits: 0,
            maximumFractionDigits: 0
        }).format(precio);
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    new ContratoCreateManager();
});
