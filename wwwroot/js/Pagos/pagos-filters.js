// Pagos Filters Management
class PagosFilters {
    constructor() {
        this.filtroEstado = document.getElementById('filtroEstado');
        this.filtroEstadoContrato = document.getElementById('filtroEstadoContrato');
        this.filtroNumeroContrato = document.getElementById('filtroNumeroContrato');
        this.filtroMes = document.getElementById('filtroMes');
        this.filtroAnio = document.getElementById('filtroAnio');
        this.filtroMonto = document.getElementById('filtroMonto');
        this.limpiarFiltros = document.getElementById('limpiarFiltros');
        this.resetearFiltros = document.getElementById('resetearFiltros');
        
        this.initializeFilters();
        this.bindEvents();
    }

    initializeFilters() {
        // Populate year dropdown with current and future years (2025-2027)
        const currentYear = new Date().getFullYear();
        const years = [2025, 2026, 2027];
        
        years.forEach(year => {
            const option = document.createElement('option');
            option.value = year;
            option.textContent = year;
            this.filtroAnio.appendChild(option);
        });
        
        // Set default values
        this.setDefaultFilters();
    }

    setDefaultFilters() {
        const now = new Date();
        const currentMonth = now.getMonth() + 1; // getMonth() returns 0-11
        const currentYear = now.getFullYear();
        
        // Set default values
        this.filtroEstado.value = 'Pendiente';
        this.filtroMes.value = currentMonth.toString();
        this.filtroAnio.value = currentYear.toString();
        
        // Apply filters immediately
        setTimeout(() => {
            this.applyFilters();
        }, 100);
    }

    bindEvents() {
        // Bind filter change events
        this.filtroEstado.addEventListener('change', () => this.applyFilters());
        this.filtroEstadoContrato.addEventListener('change', () => this.applyFilters());
        this.filtroNumeroContrato.addEventListener('input', () => this.applyFilters());
        this.filtroMes.addEventListener('change', () => this.applyFilters());
        this.filtroAnio.addEventListener('change', () => this.applyFilters());
        this.filtroMonto.addEventListener('change', () => this.applyFilters());
        
        // Bind clear filters button
        this.limpiarFiltros.addEventListener('click', () => this.clearFilters());
        
        // Bind reset to defaults button
        this.resetearFiltros.addEventListener('click', () => this.resetToDefaults());
    }

    applyFilters() {
        if (window.pagosManager && window.pagosManager.table) {
            // For server-side processing, just reload the table
            // The filters will be sent automatically via the AJAX data function
            window.pagosManager.table.ajax.reload();
        }
    }

    clearFilters() {
        // Reset all filter selects to empty values
        this.filtroEstado.value = '';
        this.filtroEstadoContrato.value = '';
        this.filtroNumeroContrato.value = '';
        this.filtroMes.value = '';
        this.filtroAnio.value = '';
        this.filtroMonto.value = '';
        
        // Reload table with cleared filters
        if (window.pagosManager && window.pagosManager.table) {
            window.pagosManager.table.ajax.reload();
        }
    }

    resetToDefaults() {
        // Reset to default filter values
        this.setDefaultFilters();
    }

    // Method to be called when DataTables is reinitialized
    reinitialize() {
        // No need to clear search functions for server-side processing
        // Just ensure default filters are applied
        this.setDefaultFilters();
    }
}

// Export for global access
window.PagosFilters = PagosFilters;
