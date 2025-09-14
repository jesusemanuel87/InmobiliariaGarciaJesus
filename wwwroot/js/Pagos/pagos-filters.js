// Pagos Filters Management
class PagosFilters {
    constructor() {
        this.filtroEstado = document.getElementById('filtroEstado');
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
            const table = window.pagosManager.table;
            
            // Apply filters to DataTables
            table.columns().search(''); // Clear previous searches
            
            // Estado filter - column index 8 (Estado)
            if (this.filtroEstado.value) {
                table.column(8).search(this.filtroEstado.value);
            }
            
            // Custom search function for date and amount filters
            $.fn.dataTable.ext.search.push((settings, data, dataIndex) => {
                if (settings.nTable.id !== 'pagosTable') return true;
                
                // Month/Year filter on Vencimiento column (index 3)
                const fechaVencimiento = data[3];
                if (this.filtroMes.value || this.filtroAnio.value) {
                    // Extract date from badge HTML
                    const fechaMatch = fechaVencimiento.match(/(\d{1,2})\/(\d{1,2})\/(\d{4})/);
                    if (fechaMatch) {
                        const [, day, month, year] = fechaMatch;
                        
                        if (this.filtroMes.value && parseInt(month) !== parseInt(this.filtroMes.value)) {
                            return false;
                        }
                        
                        if (this.filtroAnio.value && parseInt(year) !== parseInt(this.filtroAnio.value)) {
                            return false;
                        }
                    }
                }
                
                // Amount range filter on Monto column (index 4)
                if (this.filtroMonto.value) {
                    const montoText = data[4];
                    // Extract amount from formatted HTML
                    const montoMatch = montoText.match(/\$([\d,.]+)/);
                    if (montoMatch) {
                        const monto = parseFloat(montoMatch[1].replace(/[,.]/g, ''));
                        const [min, max] = this.filtroMonto.value.split('-').map(v => parseFloat(v));
                        
                        if (monto < min || monto > max) {
                            return false;
                        }
                    }
                }
                
                return true;
            });
            
            table.draw();
        }
    }

    clearFilters() {
        // Reset all filter selects to empty values
        this.filtroEstado.value = '';
        this.filtroMes.value = '';
        this.filtroAnio.value = '';
        this.filtroMonto.value = '';
        
        // Clear DataTables filters
        if (window.pagosManager && window.pagosManager.table) {
            // Remove custom search functions
            $.fn.dataTable.ext.search = $.fn.dataTable.ext.search.filter(fn => 
                !fn.toString().includes('pagosTable')
            );
            
            // Clear column searches and redraw
            window.pagosManager.table.columns().search('').draw();
        }
    }

    resetToDefaults() {
        // Reset to default filter values
        this.setDefaultFilters();
    }

    // Method to be called when DataTables is reinitialized
    reinitialize() {
        // Clear any existing custom search functions for this table
        $.fn.dataTable.ext.search = $.fn.dataTable.ext.search.filter(fn => 
            !fn.toString().includes('pagosTable')
        );
    }
}

// Export for global access
window.PagosFilters = PagosFilters;
