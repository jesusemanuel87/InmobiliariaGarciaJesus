// Inquilinos Filters - Sistema de filtrado para inquilinos
class InquilinosFilters {
    constructor() {
        this.table = null;
        this.initializeFilters();
    }

    initializeFilters() {
        // Aplicar filtros por defecto
        this.setDefaultFilters();
        
        // Event listeners
        document.getElementById('filtroEstado').addEventListener('change', () => this.applyFilters());
        document.getElementById('filtroBuscar').addEventListener('keypress', (e) => {
            if (e.key === 'Enter') {
                this.applyFilters();
            }
        });
    }

    setDefaultFilters() {
        // Estado por defecto: Activo
        document.getElementById('filtroEstado').value = 'true';
    }

    applyFilters() {
        if (this.table) {
            this.table.ajax.reload();
        }
    }

    clearFilters() {
        document.getElementById('filtroEstado').value = 'true';
        document.getElementById('filtroBuscar').value = '';
        this.applyFilters();
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
