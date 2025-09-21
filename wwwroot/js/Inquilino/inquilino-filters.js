// Inquilinos Filters - Sistema de filtrado para inquilinos (patrón Pagos)
class InquilinosFilters {
    constructor() {
        this.filtroEstado = document.getElementById('filtroEstado');
        this.filtroBuscar = document.getElementById('filtroBuscar');
        
        this.initializeFilters();
        this.bindEvents();
    }

    initializeFilters() {
        // Set default values
        this.setDefaultFilters();
    }

    setDefaultFilters() {
        // Estado por defecto: Activo
        if (this.filtroEstado) {
            this.filtroEstado.value = 'Activo';
        }
        if (this.filtroBuscar) {
            this.filtroBuscar.value = '';
        }
        
        // Apply filters immediately after a short delay
        setTimeout(() => {
            this.applyFilters();
        }, 100);
    }

    bindEvents() {
        // Bind filter change events
        if (this.filtroEstado) {
            this.filtroEstado.addEventListener('change', () => this.applyFilters());
        }
        
        if (this.filtroBuscar) {
            this.filtroBuscar.addEventListener('keypress', (e) => {
                if (e.key === 'Enter') {
                    this.applyFilters();
                }
            });
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
            if (window.inquilinoManager && window.inquilinoManager.dataTable) {
                // For server-side processing, just reload the table
                // The filters will be sent automatically via the AJAX data function
                window.inquilinoManager.dataTable.ajax.reload();
            }
        }, 100);
    }

    clearFilters() {
        // Reset all filter selects to default values
        if (this.filtroEstado) {
            this.filtroEstado.value = 'Activo';
        }
        if (this.filtroBuscar) {
            this.filtroBuscar.value = '';
        }
        
        // Reload table with cleared filters
        this.applyFilters();
    }

    resetToDefaults() {
        // Reset to default filter values
        this.setDefaultFilters();
    }

    // Method to be called when DataTables is reinitialized
    reinitialize() {
        // Just ensure default filters are applied
        this.setDefaultFilters();
    }

    getFilterData() {
        return {
            estado: document.getElementById('filtroEstado').value,
            buscar: document.getElementById('filtroBuscar').value.trim()
        };
    }

    setTable(table) {
        this.table = table;
    }
}

// Funciones globales para compatibilidad
function aplicarFiltros() {
    if (window.inquilinosFilters) {
        window.inquilinosFilters.applyFilters();
    }
}

function limpiarFiltros() {
    if (window.inquilinosFilters) {
        window.inquilinosFilters.clearFilters();
    }
}

// Inicializar cuando el DOM esté listo
document.addEventListener('DOMContentLoaded', function() {
    window.inquilinosFilters = new InquilinosFilters();
});
