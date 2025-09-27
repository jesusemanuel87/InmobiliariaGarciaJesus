// Manager principal para gestión de tipos de inmueble
class TiposInmuebleManager {
    constructor() {
        this.tabla = null;
        this.inicializado = false;
    }

    // Inicializar el manager
    inicializar() {
        if (this.inicializado) {
            console.warn('TiposInmuebleManager ya está inicializado');
            return;
        }

        try {
            // Inicializar componentes
            this.tabla = TiposInmuebleDatatables.inicializar();
            TiposInmuebleFilters.inicializar();
            
            // Cargar filtros guardados
            TiposInmuebleFilters.cargarDesdeStorage();
            
            // Configurar eventos globales
            this.configurarEventosGlobales();
            
            this.inicializado = true;
            console.log('TiposInmuebleManager inicializado correctamente');
            
        } catch (error) {
            console.error('Error al inicializar TiposInmuebleManager:', error);
            TiposInmuebleUtils.mostrarAlerta('error', 'Error al inicializar la página. Por favor, recarga la página.');
        }
    }

    // Configurar eventos globales
    configurarEventosGlobales() {
        // Guardar filtros al cambiar
        $(document).on('change', '#estadoFilter, #searchFilter', () => {
            TiposInmuebleFilters.guardarEnStorage();
        });

        // Manejar errores AJAX globales
        $(document).ajaxError((event, xhr, settings) => {
            if (settings.url && settings.url.includes('/TiposInmueble/')) {
                console.error('Error AJAX en TiposInmueble:', xhr);
            }
        });

        // Limpiar modales al cerrar
        $(document).on('hidden.bs.modal', '.modal', () => {
            TiposInmuebleUtils.limpiarErroresFormulario();
        });
    }

    // Mostrar modal de creación
    mostrarModalCrear() {
        TiposInmuebleModals.mostrarCrear();
    }

    // Mostrar modal de edición
    mostrarModalEditar(id) {
        if (!id || id <= 0) {
            TiposInmuebleUtils.mostrarAlerta('error', 'ID de tipo inválido');
            return;
        }
        TiposInmuebleModals.mostrarEditar(id);
    }

    // Mostrar modal de detalles
    mostrarModalDetalles(id) {
        if (!id || id <= 0) {
            TiposInmuebleUtils.mostrarAlerta('error', 'ID de tipo inválido');
            return;
        }
        TiposInmuebleModals.mostrarDetalles(id);
    }

    // Mostrar modal de eliminación
    mostrarModalEliminar(id) {
        if (!id || id <= 0) {
            TiposInmuebleUtils.mostrarAlerta('error', 'ID de tipo inválido');
            return;
        }
        TiposInmuebleModals.mostrarEliminar(id);
    }

    // Confirmar eliminación
    confirmarEliminacion(id) {
        TiposInmuebleModals.confirmarEliminacion(id);
    }

    // Recargar datos
    recargarDatos() {
        if (this.tabla) {
            TiposInmuebleDatatables.recargar();
            TiposInmuebleUtils.mostrarAlerta('info', 'Datos recargados correctamente');
        }
    }

    // Limpiar filtros
    limpiarFiltros() {
        TiposInmuebleFilters.limpiar();
    }

    // Exportar datos (funcionalidad futura)
    exportarDatos(formato = 'excel') {
        TiposInmuebleUtils.mostrarAlerta('info', 'Funcionalidad de exportación en desarrollo');
    }

    // Obtener estadísticas
    obtenerEstadisticas() {
        const contadores = TiposInmuebleFilters.contarResultados();
        return {
            total: contadores.total,
            filtrados: contadores.filtrados,
            activos: this.contarPorEstado(true),
            inactivos: this.contarPorEstado(false)
        };
    }

    // Contar por estado
    contarPorEstado(estado) {
        if (!this.tabla) return 0;
        
        let contador = 0;
        this.tabla.rows({ search: 'applied' }).data().each(function(row) {
            if (row.estado === estado) {
                contador++;
            }
        });
        return contador;
    }

    // Buscar tipo por nombre
    buscarPorNombre(nombre) {
        if (!nombre) {
            TiposInmuebleUtils.mostrarAlerta('warning', 'Ingrese un nombre para buscar');
            return;
        }
        
        $('#searchFilter').val(nombre);
        TiposInmuebleFilters.aplicar();
    }

    // Filtrar por estado
    filtrarPorEstado(estado) {
        $('#estadoFilter').val(estado);
        TiposInmuebleFilters.aplicar();
    }

    // Destruir manager
    destruir() {
        if (this.tabla) {
            TiposInmuebleDatatables.destruir();
        }
        
        TiposInmuebleFilters.limpiarStorage();
        $(document).off('.tiposInmueble'); // Remover eventos con namespace
        
        this.inicializado = false;
        console.log('TiposInmuebleManager destruido');
    }

    // Verificar si está inicializado
    estaInicializado() {
        return this.inicializado;
    }

    // Obtener instancia de la tabla
    obtenerTabla() {
        return this.tabla;
    }

    // Método de utilidad para debug
    debug() {
        return {
            inicializado: this.inicializado,
            tabla: this.tabla,
            estadisticas: this.obtenerEstadisticas(),
            filtros: TiposInmuebleFilters.obtenerValores()
        };
    }
}

// Crear instancia global
const tiposInmuebleManager = new TiposInmuebleManager();
