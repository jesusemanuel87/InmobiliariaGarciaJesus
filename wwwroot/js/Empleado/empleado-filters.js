// Empleados Filters - Sistema de filtrado para empleados (patrón Propietarios)
class EmpleadosFilters {
    constructor() {
        this.filtroEstado = document.getElementById('filtroEstado');
        this.filtroRol = document.getElementById('filtroRol');
        this.filtroBuscar = document.getElementById('filtroBuscar');
        
        this.initializeFilters();
        this.bindEvents();
    }

    initializeFilters() {
        // Set default values
        this.setDefaultFilters();
    }

    setDefaultFilters() {
        // Default: Show active employees
        if (this.filtroEstado && !this.filtroEstado.value) {
            this.filtroEstado.value = 'true';
        }
    }

    bindEvents() {
        // Estado filter change
        if (this.filtroEstado) {
            this.filtroEstado.addEventListener('change', () => {
                this.applyFilters();
            });
        }

        // Rol filter change
        if (this.filtroRol) {
            this.filtroRol.addEventListener('change', () => {
                this.applyFilters();
            });
        }

        // Search input with debounce
        if (this.filtroBuscar) {
            this.filtroBuscar.addEventListener('input', () => {
                // Auto-apply after typing (with debounce)
                clearTimeout(this.searchTimeout);
                this.searchTimeout = setTimeout(() => {
                    this.applyFilters();
                }, 500);
            });
        }
    }

    applyFilters() {
        // Clear any pending filter requests to avoid multiple simultaneous calls
        clearTimeout(this.filterTimeout);
        
        this.filterTimeout = setTimeout(() => {
            if (window.empleadoManager && window.empleadoManager.dataTable) {
                // For server-side processing, just reload the table
                // The filters will be sent automatically via the AJAX data function
                window.empleadoManager.dataTable.ajax.reload();
            }
        }, 100);
    }

    clearFilters() {
        // Reset all filter selects to default values
        if (this.filtroEstado) {
            this.filtroEstado.value = 'true';
        }
        if (this.filtroRol) {
            this.filtroRol.value = '';
        }
        if (this.filtroBuscar) {
            this.filtroBuscar.value = '';
        }
        
        // Reload table with cleared filters
        this.applyFilters();
    }

    resetToDefaults() {
        this.setDefaultFilters();
        this.applyFilters();
    }

    getFilterData() {
        return {
            estado: this.filtroEstado?.value || '',
            rol: this.filtroRol?.value || '',
            buscar: this.filtroBuscar?.value || ''
        };
    }

    setTable(table) {
        this.table = table;
    }

    // Method to be called when DataTables is reinitialized
    reinitialize() {
        // Just ensure default filters are applied
        this.setDefaultFilters();
    }
}

// Funciones globales para compatibilidad
function aplicarFiltros() {
    if (window.empleadosFilters) {
        window.empleadosFilters.applyFilters();
    }
}

function limpiarFiltros() {
    if (window.empleadosFilters) {
        window.empleadosFilters.clearFilters();
    }
}

// La inicialización se hace desde el manager
