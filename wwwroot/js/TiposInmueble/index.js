// Inicialización de la página de tipos de inmueble
$(document).ready(function() {
    // Verificar que todas las dependencias estén disponibles
    if (typeof TiposInmuebleUtils === 'undefined' ||
        typeof TiposInmuebleDatatables === 'undefined' ||
        typeof TiposInmuebleFilters === 'undefined' ||
        typeof TiposInmuebleModals === 'undefined' ||
        typeof TiposInmuebleManager === 'undefined') {
        
        console.error('Error: No se pudieron cargar todos los módulos necesarios');
        alert('Error al cargar la página. Por favor, recarga la página.');
        return;
    }

    // Inicializar el manager principal
    try {
        tiposInmuebleManager.inicializar();
        
        // Mostrar mensaje de bienvenida (opcional)
        console.log('Página de Tipos de Inmueble cargada correctamente');
        
        // Configurar tooltips
        $('[data-bs-toggle="tooltip"]').tooltip();
        
        // Configurar eventos adicionales específicos de esta página
        configurarEventosEspecificos();
        
    } catch (error) {
        console.error('Error al inicializar la página:', error);
        TiposInmuebleUtils.mostrarAlerta('error', 'Error al cargar la página. Por favor, recarga la página.');
    }
});

// Configurar eventos específicos de la página
function configurarEventosEspecificos() {
    // Atajos de teclado
    $(document).on('keydown', function(e) {
        // Ctrl + N = Nuevo tipo
        if (e.ctrlKey && e.key === 'n') {
            e.preventDefault();
            tiposInmuebleManager.mostrarModalCrear();
        }
        
        // Ctrl + R = Recargar datos
        if (e.ctrlKey && e.key === 'r') {
            e.preventDefault();
            tiposInmuebleManager.recargarDatos();
        }
        
        // Ctrl + L = Limpiar filtros
        if (e.ctrlKey && e.key === 'l') {
            e.preventDefault();
            tiposInmuebleManager.limpiarFiltros();
        }
        
        // Escape = Limpiar modales bloqueados
        if (e.key === 'Escape') {
            TiposInmuebleUtils.limpiarModalesBloqueados();
        }
    });

    // Doble clic en fila para ver detalles
    $(document).on('dblclick', '#tiposInmuebleTable tbody tr', function() {
        const tabla = tiposInmuebleManager.obtenerTabla();
        if (tabla) {
            const data = tabla.row(this).data();
            if (data && data.id) {
                tiposInmuebleManager.mostrarModalDetalles(data.id);
            }
        }
    });

    // Confirmar antes de salir si hay cambios sin guardar
    let haycambiosSinGuardar = false;
    
    $(document).on('input change', '#tipoInmuebleForm input, #tipoInmuebleForm textarea, #tipoInmuebleForm select', function() {
        hayChangiosSinGuardar = true;
    });

    $(document).on('submit', '#tipoInmuebleForm', function() {
        hayChangiosSinGuardar = false;
    });

    $(window).on('beforeunload', function(e) {
        if (hayChangiosSinGuardar) {
            e.preventDefault();
            return 'Tienes cambios sin guardar. ¿Estás seguro de que quieres salir?';
        }
    });

    // Limpiar flag al cerrar modal
    $(document).on('hidden.bs.modal', '.modal', function() {
        hayChangiosSinGuardar = false;
    });
}

// Función global para limpiar filtros (llamada desde HTML)
function limpiarFiltros() {
    tiposInmuebleManager.limpiarFiltros();
}

// Función global para recargar datos
function recargarDatos() {
    tiposInmuebleManager.recargarDatos();
}

// Funciones globales para compatibilidad con botones HTML
window.tiposInmuebleManager = tiposInmuebleManager;
window.limpiarFiltros = limpiarFiltros;
window.recargarDatos = recargarDatos;

// Debug: Exponer manager en consola para desarrollo
if (typeof window !== 'undefined') {
    window.debugTiposInmueble = function() {
        return tiposInmuebleManager.debug();
    };
}
