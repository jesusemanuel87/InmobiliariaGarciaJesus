/**
 * Inmuebles Filters Manager
 * Maneja los filtros en la vista de índice de inmuebles
 */
class InmueblesFilters {
    constructor() {
        this.initializeElements();
        this.bindEvents();
        this.loadProvincias();
        this.setCurrentValues();
        this.setDefaultFilters();
    }

    initializeElements() {
        // Filtros
        this.filtroEstado = document.getElementById('filtroEstado');
        this.filtroTipo = document.getElementById('filtroTipo');
        this.filtroUso = document.getElementById('filtroUso');
        this.filtroProvincia = document.getElementById('filtroProvincia');
        this.filtroLocalidad = document.getElementById('filtroLocalidad');
        this.filtroDisponibilidad = document.getElementById('filtroDisponibilidad');
        this.precioMin = document.getElementById('precioMin');
        this.precioMax = document.getElementById('precioMax');
        this.fechaDesde = document.getElementById('fechaDesde');
        this.fechaHasta = document.getElementById('fechaHasta');

        // Botones
        this.limpiarFiltros = document.getElementById('limpiarFiltros');
        this.limpiarFiltrosEmpty = document.getElementById('limpiarFiltrosEmpty');
        this.filtrosForm = document.getElementById('filtrosForm');

        // Hidden inputs
        this.estadoActual = document.getElementById('estadoActual')?.value || '';
        this.tipoActual = document.getElementById('tipoActual')?.value || '';
        this.usoActual = document.getElementById('usoActual')?.value || '';
        this.provinciaActual = document.getElementById('provinciaActual')?.value || '';
        this.localidadActual = document.getElementById('localidadActual')?.value || '';
        this.disponibilidadActual = document.getElementById('disponibilidadActual')?.value || '';
        this.userRole = document.getElementById('userRole')?.value || '';
        this.canViewAllStates = document.getElementById('canViewAllStates')?.value === 'True';

        // Georef service
        this.georefService = window.georefService || new GeorefService();
    }

    bindEvents() {
        // Cambio de provincia
        if (this.filtroProvincia) {
            this.filtroProvincia.addEventListener('change', () => {
                this.onProvinciaChange();
            });
        }

        // Limpiar filtros
        if (this.limpiarFiltros) {
            this.limpiarFiltros.addEventListener('click', () => {
                this.clearFilters();
            });
        }

        // Limpiar filtros desde mensaje vacío
        if (this.limpiarFiltrosEmpty) {
            this.limpiarFiltrosEmpty.addEventListener('click', () => {
                this.clearFilters();
                // Redirigir a la página sin parámetros
                window.location.href = window.location.pathname;
            });
        }

        // Evento para calcular fecha hasta automáticamente
        if (this.fechaDesde) {
            this.fechaDesde.addEventListener('change', () => {
                this.calcularFechaHasta();
            });
        }

        // Auto-submit en cambios de filtros (opcional)
        const autoSubmitElements = [
            this.filtroEstado, this.filtroTipo, this.filtroUso, 
            this.filtroDisponibilidad, this.filtroLocalidad
        ];

        autoSubmitElements.forEach(element => {
            if (element) {
                element.addEventListener('change', () => {
                    // Opcional: auto-submit del formulario
                    // this.filtrosForm.submit();
                });
            }
        });
    }

    async loadProvincias() {
        if (!this.filtroProvincia) return;

        try {
            const provincias = await this.georefService.obtenerProvincias();
            
            // Limpiar opciones existentes
            this.filtroProvincia.innerHTML = '<option value="">Todas las provincias</option>';
            
            // Verificar si es primera carga para establecer valores por defecto
            const urlParams = new URLSearchParams(window.location.search);
            const isFirstLoad = urlParams.toString() === '';
            
            // Agregar provincias
            provincias.forEach(provincia => {
                const option = document.createElement('option');
                option.value = provincia.nombre;
                option.textContent = provincia.nombre;
                
                // Seleccionar si coincide con la provincia actual o es primera carga con San Luis
                if (provincia.nombre === this.provinciaActual || 
                    (isFirstLoad && !this.provinciaActual && provincia.nombre === 'San Luis')) {
                    option.selected = true;
                    this.provinciaActual = provincia.nombre; // Actualizar para usar en localidades
                }
                
                this.filtroProvincia.appendChild(option);
            });

            // Si hay una provincia seleccionada, cargar sus localidades
            if (this.provinciaActual) {
                await this.loadLocalidades(this.provinciaActual);
            }

        } catch (error) {
            console.error('Error loading provincias:', error);
            this.filtroProvincia.innerHTML = '<option value="">Error cargando provincias</option>';
        }
    }

    async loadLocalidades(provinciaNombre) {
        if (!this.filtroLocalidad || !provinciaNombre) return;

        try {
            // Mostrar loading
            this.filtroLocalidad.innerHTML = '<option value="">Cargando localidades...</option>';
            
            const localidades = await this.georefService.obtenerLocalidades(provinciaNombre);
            
            // Limpiar y agregar opciones
            this.filtroLocalidad.innerHTML = '<option value="">Todas las localidades</option>';
            
            // Verificar si es primera carga para establecer valores por defecto
            const urlParams = new URLSearchParams(window.location.search);
            const isFirstLoad = urlParams.toString() === '';
            
            localidades.forEach(localidad => {
                const option = document.createElement('option');
                option.value = localidad.nombre;
                option.textContent = localidad.nombre;
                
                // Seleccionar si coincide con la localidad actual o es primera carga con San Luis
                if (localidad.nombre === this.localidadActual || 
                    (isFirstLoad && !this.localidadActual && localidad.nombre === 'San Luis')) {
                    option.selected = true;
                }
                
                this.filtroLocalidad.appendChild(option);
            });

        } catch (error) {
            console.error('Error loading localidades:', error);
            this.filtroLocalidad.innerHTML = '<option value="">Error cargando localidades</option>';
        }
    }

    async onProvinciaChange() {
        const selectedProvincia = this.filtroProvincia.value;
        
        if (selectedProvincia) {
            await this.loadLocalidades(selectedProvincia);
        } else {
            // Si no hay provincia seleccionada, limpiar localidades
            this.filtroLocalidad.innerHTML = '<option value="">Seleccione primero una provincia</option>';
        }
    }

    setCurrentValues() {
        // Establecer valores actuales desde el servidor
        if (this.filtroEstado && this.estadoActual) {
            this.filtroEstado.value = this.estadoActual;
        }
        
        if (this.filtroTipo && this.tipoActual) {
            this.filtroTipo.value = this.tipoActual;
        }
        
        if (this.filtroUso && this.usoActual) {
            this.filtroUso.value = this.usoActual;
        }
        
        if (this.filtroDisponibilidad && this.disponibilidadActual) {
            // Para multi-select, necesitamos manejar múltiples valores
            const valoresSeleccionados = this.disponibilidadActual.split(',');
            Array.from(this.filtroDisponibilidad.options).forEach(option => {
                option.selected = valoresSeleccionados.includes(option.value);
            });
        }
        
        // Debug logs
        console.log('DEBUG - Valores actuales:', {
            estado: this.estadoActual,
            tipo: this.tipoActual,
            uso: this.usoActual,
            provincia: this.provinciaActual,
            localidad: this.localidadActual,
            disponibilidad: this.disponibilidadActual,
            userRole: this.userRole,
            canViewAllStates: this.canViewAllStates
        });
    }

    setDefaultFilters() {
        // Verificar si es la primera carga (sin parámetros en la URL)
        const urlParams = new URLSearchParams(window.location.search);
        const isFirstLoad = urlParams.toString() === '';
        
        if (isFirstLoad) {
            // Establecer valores por defecto para la primera carga
            
            // Disponibilidad por defecto: Disponible y Reservado
            if (this.filtroDisponibilidad && !this.disponibilidadActual) {
                // Seleccionar múltiples opciones por defecto
                Array.from(this.filtroDisponibilidad.options).forEach(option => {
                    if (option.value === 'Disponible' || option.value === 'Reservado') {
                        option.selected = true;
                    }
                });
            }
            
            // Estado por defecto para Administradores y Empleados: Activo
            if ((this.userRole === 'Administrador' || this.userRole === 'Empleado') && 
                this.filtroEstado && !this.estadoActual) {
                this.filtroEstado.value = 'Activo';
            }
            
            // Provincia y Localidad se establecerán cuando se carguen desde la API
            // (San Luis por defecto)
        }
    }

    calcularFechaHasta() {
        if (!this.fechaDesde || !this.fechaDesde.value || this.fechaHasta.value) {
            return; // No calcular si no hay fecha desde o ya hay fecha hasta
        }

        const fechaDesde = new Date(this.fechaDesde.value);
        
        // Agregar un año
        const fechaHasta = new Date(fechaDesde);
        fechaHasta.setFullYear(fechaHasta.getFullYear() + 1);
        
        // Ir al último día del mes
        fechaHasta.setMonth(fechaHasta.getMonth() + 1, 0);
        
        // Formatear fecha para input date (YYYY-MM-DD)
        const fechaFormateada = fechaHasta.toISOString().split('T')[0];
        
        this.fechaHasta.value = fechaFormateada;
        
        console.log(`Fecha calculada automáticamente: Desde ${this.fechaDesde.value} hasta ${fechaFormateada}`);
    }

    clearFilters() {
        // Limpiar todos los filtros
        if (this.filtroEstado) this.filtroEstado.value = '';
        if (this.filtroTipo) this.filtroTipo.value = '';
        if (this.filtroUso) this.filtroUso.value = '';
        if (this.filtroProvincia) this.filtroProvincia.value = '';
        if (this.filtroLocalidad) this.filtroLocalidad.innerHTML = '<option value="">Seleccione primero una provincia</option>';
        if (this.filtroDisponibilidad) {
            Array.from(this.filtroDisponibilidad.options).forEach(option => {
                option.selected = false;
            });
        }
        if (this.precioMin) this.precioMin.value = '';
        if (this.precioMax) this.precioMax.value = '';
        if (this.fechaDesde) this.fechaDesde.value = '';
        if (this.fechaHasta) this.fechaHasta.value = '';

        // Aplicar filtros por defecto después de limpiar
        this.setDefaultFilters();
    }

    // Método para obtener los valores actuales de los filtros
    getCurrentFilters() {
        return {
            estado: this.filtroEstado?.value || '',
            tipo: this.filtroTipo?.value || '',
            uso: this.filtroUso?.value || '',
            provincia: this.filtroProvincia?.value || '',
            localidad: this.filtroLocalidad?.value || '',
            disponibilidad: this.filtroDisponibilidad ? Array.from(this.filtroDisponibilidad.selectedOptions).map(o => o.value) : [],
            precioMin: this.precioMin?.value || '',
            precioMax: this.precioMax?.value || '',
            fechaDesde: this.fechaDesde?.value || '',
            fechaHasta: this.fechaHasta?.value || ''
        };
    }

    // Método para aplicar filtros programáticamente
    applyFilters(filters) {
        if (filters.estado && this.filtroEstado) this.filtroEstado.value = filters.estado;
        if (filters.tipo && this.filtroTipo) this.filtroTipo.value = filters.tipo;
        if (filters.uso && this.filtroUso) this.filtroUso.value = filters.uso;
        if (filters.provincia && this.filtroProvincia) this.filtroProvincia.value = filters.provincia;
        if (filters.localidad && this.filtroLocalidad) this.filtroLocalidad.value = filters.localidad;
        if (filters.disponibilidad && this.filtroDisponibilidad) {
            Array.from(this.filtroDisponibilidad.options).forEach(option => {
                option.selected = filters.disponibilidad.includes(option.value);
            });
        }
        if (filters.precioMin && this.precioMin) this.precioMin.value = filters.precioMin;
        if (filters.precioMax && this.precioMax) this.precioMax.value = filters.precioMax;
        if (filters.fechaDesde && this.fechaDesde) this.fechaDesde.value = filters.fechaDesde;
        if (filters.fechaHasta && this.fechaHasta) this.fechaHasta.value = filters.fechaHasta;

        // Si se cambió la provincia, cargar localidades
        if (filters.provincia) {
            this.loadLocalidades(filters.provincia);
        }
    }
}

// Inicializar cuando el DOM esté listo
document.addEventListener('DOMContentLoaded', function() {
    console.log('Inicializando filtros de inmuebles...');
    
    // Crear instancia global
    window.inmueblesFilters = new InmueblesFilters();
    
    console.log('Filtros de inmuebles inicializados correctamente');
});
