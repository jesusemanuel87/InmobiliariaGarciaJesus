// Gestión de filtros para tipos de inmueble
const TiposInmuebleFilters = {
    // Inicializar eventos de filtros
    inicializar: function() {
        this.configurarEventos();
    },

    // Configurar eventos de los filtros
    configurarEventos: function() {
        // Filtro de estado
        $('#estadoFilter').on('change', function() {
            const estado = $(this).val();
            TiposInmuebleDatatables.aplicarFiltros({ estado: estado });
        });

        // Filtro de búsqueda con debounce
        let searchTimeout;
        $('#searchFilter').on('input', function() {
            clearTimeout(searchTimeout);
            const busqueda = $(this).val();
            
            searchTimeout = setTimeout(() => {
                TiposInmuebleDatatables.aplicarFiltros({ busqueda: busqueda });
            }, 300); // Esperar 300ms después de que el usuario deje de escribir
        });

        // Limpiar búsqueda con Escape
        $('#searchFilter').on('keydown', function(e) {
            if (e.key === 'Escape') {
                $(this).val('');
                TiposInmuebleDatatables.aplicarFiltros({ busqueda: '' });
            }
        });
    },

    // Obtener valores actuales de los filtros
    obtenerValores: function() {
        return {
            estado: $('#estadoFilter').val(),
            busqueda: $('#searchFilter').val()
        };
    },

    // Establecer valores de filtros
    establecerValores: function(valores) {
        if (valores.estado !== undefined) {
            $('#estadoFilter').val(valores.estado);
        }
        if (valores.busqueda !== undefined) {
            $('#searchFilter').val(valores.busqueda);
        }
    },

    // Limpiar todos los filtros
    limpiar: function() {
        $('#estadoFilter').val('');
        $('#searchFilter').val('');
        TiposInmuebleDatatables.limpiarFiltros();
        
        // Mostrar mensaje de confirmación
        TiposInmuebleUtils.mostrarAlerta('info', 'Filtros limpiados correctamente');
    },

    // Aplicar filtros programáticamente
    aplicar: function(filtros = null) {
        const valores = filtros || this.obtenerValores();
        TiposInmuebleDatatables.aplicarFiltros(valores);
    },

    // Contar registros filtrados
    contarResultados: function() {
        const tabla = TiposInmuebleDatatables.obtenerTabla();
        if (tabla) {
            return {
                total: tabla.data().count(),
                filtrados: tabla.rows({ search: 'applied' }).count()
            };
        }
        return { total: 0, filtrados: 0 };
    },

    // Exportar configuración de filtros
    exportarConfiguracion: function() {
        return {
            filtros: this.obtenerValores(),
            timestamp: new Date().toISOString()
        };
    },

    // Importar configuración de filtros
    importarConfiguracion: function(configuracion) {
        if (configuracion && configuracion.filtros) {
            this.establecerValores(configuracion.filtros);
            this.aplicar(configuracion.filtros);
        }
    },

    // Guardar filtros en localStorage
    guardarEnStorage: function() {
        const configuracion = this.exportarConfiguracion();
        localStorage.setItem('tiposInmueble_filtros', JSON.stringify(configuracion));
    },

    // Cargar filtros desde localStorage
    cargarDesdeStorage: function() {
        try {
            const configuracionStr = localStorage.getItem('tiposInmueble_filtros');
            if (configuracionStr) {
                const configuracion = JSON.parse(configuracionStr);
                this.importarConfiguracion(configuracion);
                return true;
            }
        } catch (error) {
            console.warn('Error al cargar filtros desde localStorage:', error);
        }
        return false;
    },

    // Limpiar storage
    limpiarStorage: function() {
        localStorage.removeItem('tiposInmueble_filtros');
    }
};
