// ContratosFilters - Sistema de filtros para Contratos
class ContratosFilters {
    constructor() {
        this.initializeFilters();
        this.bindEvents();
    }

    initializeFilters() {
        // Inicializar dropdown multiselect para Estados
        this.initializeEstadoDropdown();
        
        // Aplicar valores por defecto si es la primera carga
        this.applyDefaultFilters();
    }

    initializeEstadoDropdown() {
        const checkboxes = document.querySelectorAll('input[name="estado"]');
        const todosCheckbox = document.getElementById('estadoTodos');
        const placeholder = document.getElementById('estadoPlaceholder');
        const dropdown = document.getElementById('estadoDropdown');
        
        if (checkboxes.length > 0 && placeholder) {
            // Escuchar cambios en los checkboxes individuales
            checkboxes.forEach(checkbox => {
                checkbox.addEventListener('change', () => {
                    this.updateTodosCheckbox();
                    this.updatePlaceholder();
                    
                    // No auto-submit para permitir selección múltiple
                    // El usuario debe hacer clic en "Buscar" o presionar Enter
                });
            });

            // Escuchar cambios en el checkbox "Todos"
            if (todosCheckbox) {
                todosCheckbox.addEventListener('change', () => {
                    const isChecked = todosCheckbox.checked;
                    
                    // Marcar/desmarcar todos los checkboxes
                    checkboxes.forEach(checkbox => {
                        checkbox.checked = isChecked;
                    });
                    
                    this.updatePlaceholder();
                    
                    // No auto-submit para permitir selección múltiple
                    // El usuario debe hacer clic en "Buscar" o presionar Enter
                });
            }
            
            // Prevenir que el dropdown se cierre al hacer clic en los checkboxes
            const dropdownMenu = document.getElementById('estadoDropdownMenu');
            if (dropdownMenu) {
                dropdownMenu.addEventListener('click', (e) => {
                    e.stopPropagation();
                });
            }
        }
    }

    updateTodosCheckbox() {
        const checkboxes = document.querySelectorAll('input[name="estado"]');
        const todosCheckbox = document.getElementById('estadoTodos');
        
        if (todosCheckbox) {
            const allChecked = Array.from(checkboxes).every(cb => cb.checked);
            const noneChecked = Array.from(checkboxes).every(cb => !cb.checked);
            
            if (allChecked) {
                todosCheckbox.checked = true;
                todosCheckbox.indeterminate = false;
            } else if (noneChecked) {
                todosCheckbox.checked = false;
                todosCheckbox.indeterminate = false;
            } else {
                todosCheckbox.checked = false;
                todosCheckbox.indeterminate = true;
            }
        }
    }

    updatePlaceholder() {
        const checkboxes = document.querySelectorAll('input[name="estado"]:checked');
        const placeholder = document.getElementById('estadoPlaceholder');
        
        if (!placeholder) return;
        
        if (checkboxes.length === 0) {
            placeholder.textContent = 'Seleccionar estados...';
            placeholder.style.color = '#6c757d';
        } else if (checkboxes.length === 1) {
            placeholder.textContent = checkboxes[0].value;
            placeholder.style.color = '#212529';
        } else {
            placeholder.textContent = `${checkboxes.length} estados seleccionados`;
            placeholder.style.color = '#212529';
        }
    }


    applyDefaultFilters() {
        // Restaurar valores desde el servidor o aplicar defaults
        this.restoreServerValues();
        
        // Solo aplicar defaults si no hay parámetros en la URL
        const urlParams = new URLSearchParams(window.location.search);
        const hasFilters = Array.from(urlParams.keys()).some(key => 
            ['estado', 'inquilino', 'inmueble', 'precioMin', 'precioMax', 
             'fechaDesde', 'fechaHasta', 'fechaInicioDesde', 'fechaInicioHasta',
             'fechaFinDesde', 'fechaFinHasta'].includes(key)
        );

        if (!hasFilters) {
            // Aplicar filtros por defecto: Activo y Reservado
            const activoCheckbox = document.getElementById('estadoActivo');
            const reservadoCheckbox = document.getElementById('estadoReservado');
            
            if (activoCheckbox) activoCheckbox.checked = true;
            if (reservadoCheckbox) reservadoCheckbox.checked = true;
            
            this.updateTodosCheckbox();
            this.updatePlaceholder();
        }
    }

    restoreServerValues() {
        // Restaurar valores de estado desde ViewBag
        const estadosFromServer = window.estadosSeleccionados || [];
        
        if (estadosFromServer.length > 0) {
            estadosFromServer.forEach(estado => {
                const checkbox = document.querySelector(`input[name="estado"][value="${estado}"]`);
                if (checkbox) {
                    checkbox.checked = true;
                }
            });
            this.updateTodosCheckbox();
            this.updatePlaceholder();
        }
    }

    bindEvents() {
        // Toggle del panel de filtros
        const toggleButton = document.getElementById('toggleFilters');
        const filtersPanel = document.getElementById('filtersPanel');
        
        if (toggleButton && filtersPanel) {
            toggleButton.addEventListener('click', () => {
                const isVisible = filtersPanel.style.display !== 'none';
                filtersPanel.style.display = isVisible ? 'none' : 'block';
                
                const icon = toggleButton.querySelector('i');
                if (icon) {
                    icon.className = isVisible ? 'fas fa-chevron-down' : 'fas fa-chevron-up';
                }
            });
        }

        // Limpiar filtros
        const clearButton = document.getElementById('clearFilters');
        const clearButtonEmpty = document.getElementById('clearFiltersEmpty');
        
        if (clearButton) {
            clearButton.addEventListener('click', () => this.clearAllFilters());
        }
        
        if (clearButtonEmpty) {
            clearButtonEmpty.addEventListener('click', () => this.clearAllFilters());
        }

        // Auto-submit en cambios de fecha
        const dateInputs = document.querySelectorAll('input[type="date"]');
        dateInputs.forEach(input => {
            input.addEventListener('change', () => {
                // Pequeño delay para mejor UX
                setTimeout(() => {
                    document.getElementById('filtersForm').submit();
                }, 300);
            });
        });

        // Enter key en campos de texto
        const textInputs = document.querySelectorAll('#inquilino, #inmueble');
        textInputs.forEach(input => {
            input.addEventListener('keypress', (e) => {
                if (e.key === 'Enter') {
                    e.preventDefault();
                    document.getElementById('filtersForm').submit();
                }
            });
        });

        // Auto-submit ya está configurado en initializeChoicesJS()
        // No necesitamos duplicar el event listener aquí
    }

    clearAllFilters() {
        // Redirigir a la página sin parámetros para aplicar filtros por defecto
        window.location.href = window.location.pathname;
    }

    // Método para obtener los filtros activos
    getActiveFilters() {
        const form = document.getElementById('filtersForm');
        if (!form) return {};

        const formData = new FormData(form);
        const filters = {};

        for (let [key, value] of formData.entries()) {
            if (value && value.trim() !== '') {
                if (filters[key]) {
                    // Si ya existe, convertir a array
                    if (!Array.isArray(filters[key])) {
                        filters[key] = [filters[key]];
                    }
                    filters[key].push(value);
                } else {
                    filters[key] = value;
                }
            }
        }

        return filters;
    }

    // Método para aplicar filtros programáticamente
    applyFilters(filters) {
        Object.keys(filters).forEach(key => {
            if (key === 'estado') {
                // Para el multiselect de estados usando checkboxes
                const values = Array.isArray(filters[key]) ? filters[key] : [filters[key]];
                
                // Limpiar checkboxes primero
                document.querySelectorAll('input[name="estado"]').forEach(cb => cb.checked = false);
                
                // Marcar los valores seleccionados
                values.forEach(value => {
                    const checkbox = document.querySelector(`input[name="estado"][value="${value}"]`);
                    if (checkbox) checkbox.checked = true;
                });
                
                this.updatePlaceholder();
            } else {
                const element = document.getElementById(key);
                if (element) {
                    element.value = filters[key];
                }
            }
        });
    }
}

// Inicializar cuando el DOM esté listo
document.addEventListener('DOMContentLoaded', function() {
    new ContratosFilters();
});

// Exportar para uso global si es necesario
window.ContratosFilters = ContratosFilters;
