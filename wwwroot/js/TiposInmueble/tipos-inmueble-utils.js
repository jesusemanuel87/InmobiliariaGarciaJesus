// Utilidades para gestión de tipos de inmueble
const TiposInmuebleUtils = {
    // Mostrar alertas
    mostrarAlerta: function(tipo, mensaje, titulo = null) {
        const alertContainer = $('#alertContainer');
        const alertId = 'alert-' + Date.now();
        
        let iconClass = 'fas fa-info-circle';
        switch(tipo) {
            case 'success':
                iconClass = 'fas fa-check-circle';
                break;
            case 'error':
            case 'danger':
                iconClass = 'fas fa-exclamation-triangle';
                break;
            case 'warning':
                iconClass = 'fas fa-exclamation-circle';
                break;
        }

        const alert = `
            <div id="${alertId}" class="alert alert-${tipo} alert-dismissible fade show" role="alert">
                <i class="${iconClass} me-2"></i>
                ${titulo ? `<strong>${titulo}</strong> ` : ''}${mensaje}
                <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
            </div>
        `;
        
        alertContainer.append(alert);
        
        // Auto-remover después de 5 segundos
        setTimeout(() => {
            $(`#${alertId}`).fadeOut(() => {
                $(`#${alertId}`).remove();
            });
        }, 5000);
    },

    // Limpiar alertas
    limpiarAlertas: function() {
        $('#alertContainer').empty();
    },

    // Validar formulario
    validarFormulario: function() {
        let esValido = true;
        
        // Validar nombre
        const nombre = $('#tipoNombre').val().trim();
        if (!nombre) {
            this.mostrarErrorCampo('tipoNombre', 'El nombre es obligatorio');
            esValido = false;
        } else if (nombre.length > 50) {
            this.mostrarErrorCampo('tipoNombre', 'El nombre no puede exceder 50 caracteres');
            esValido = false;
        } else {
            this.limpiarErrorCampo('tipoNombre');
        }

        // Validar descripción
        const descripcion = $('#tipoDescripcion').val().trim();
        if (descripcion.length > 200) {
            this.mostrarErrorCampo('tipoDescripcion', 'La descripción no puede exceder 200 caracteres');
            esValido = false;
        } else {
            this.limpiarErrorCampo('tipoDescripcion');
        }

        return esValido;
    },

    // Mostrar error en campo
    mostrarErrorCampo: function(campoId, mensaje) {
        const campo = $(`#${campoId}`);
        campo.addClass('is-invalid');
        campo.siblings('.invalid-feedback').text(mensaje);
    },

    // Limpiar error de campo
    limpiarErrorCampo: function(campoId) {
        const campo = $(`#${campoId}`);
        campo.removeClass('is-invalid');
        campo.siblings('.invalid-feedback').text('');
    },

    // Limpiar todos los errores del formulario
    limpiarErroresFormulario: function() {
        $('.is-invalid').removeClass('is-invalid');
        $('.invalid-feedback').text('');
    },

    // Formatear fecha
    formatearFecha: function(fecha) {
        if (!fecha) return '';
        const date = new Date(fecha);
        return date.toLocaleDateString('es-ES', {
            year: 'numeric',
            month: '2-digit',
            day: '2-digit',
            hour: '2-digit',
            minute: '2-digit'
        });
    },

    // Obtener datos del formulario
    obtenerDatosFormulario: function() {
        return {
            Id: parseInt($('#tipoId').val()) || 0,
            Nombre: $('#tipoNombre').val().trim(),
            Descripcion: $('#tipoDescripcion').val().trim() || null,
            Estado: $('#tipoEstado').val() === 'true'
        };
    },

    // Limpiar formulario
    limpiarFormulario: function() {
        $('#tipoInmuebleForm')[0].reset();
        $('#tipoId').val('0');
        this.limpiarErroresFormulario();
    },

    // Deshabilitar/habilitar botones
    toggleBotones: function(deshabilitado) {
        $('#submitButton').prop('disabled', deshabilitado);
        if (deshabilitado) {
            $('#submitButton').html('<i class="fas fa-spinner fa-spin me-1"></i>Procesando...');
        }
    },

    // Restaurar texto del botón
    restaurarBoton: function(esEdicion) {
        const texto = esEdicion ? 'Actualizar' : 'Crear';
        const icono = esEdicion ? 'save' : 'plus';
        $('#submitButton').html(`<i class="fas fa-${icono} me-1"></i>${texto}`);
    },

    // Confirmar acción
    confirmarAccion: function(mensaje, callback) {
        if (confirm(mensaje)) {
            callback();
        }
    },

    // Generar badge de estado
    generarBadgeEstado: function(estado) {
        if (estado) {
            return '<span class="badge bg-success"><i class="fas fa-check me-1"></i>Activo</span>';
        } else {
            return '<span class="badge bg-danger"><i class="fas fa-times me-1"></i>Inactivo</span>';
        }
    },

    // Generar botones de acción
    generarBotonesAccion: function(id) {
        return `
            <div class="btn-group" role="group">
                <button type="button" class="btn btn-sm btn-outline-info" 
                        onclick="tiposInmuebleManager.mostrarModalDetalles(${id})" 
                        title="Ver detalles">
                    <i class="fas fa-eye"></i>
                </button>
                <button type="button" class="btn btn-sm btn-outline-primary" 
                        onclick="tiposInmuebleManager.mostrarModalEditar(${id})" 
                        title="Editar">
                    <i class="fas fa-edit"></i>
                </button>
                <button type="button" class="btn btn-sm btn-outline-danger" 
                        onclick="tiposInmuebleManager.mostrarModalEliminar(${id})" 
                        title="Eliminar">
                    <i class="fas fa-trash"></i>
                </button>
            </div>
        `;
    },

    // Truncar texto
    truncarTexto: function(texto, longitud = 50) {
        if (!texto) return '';
        if (texto.length <= longitud) return texto;
        return texto.substring(0, longitud) + '...';
    },

    // Función de emergencia para limpiar modales bloqueados
    limpiarModalesBloqueados: function() {
        // Cerrar todos los modales de Bootstrap
        $('.modal').each(function() {
            const modal = bootstrap.Modal.getInstance(this);
            if (modal) {
                modal.hide();
            }
        });
        
        // Limpiar contenedores y backdrops
        setTimeout(() => {
            $('#modalContainer').empty();
            $('.modal-backdrop').remove();
            $('body').removeClass('modal-open');
            $('body').css('padding-right', '');
            $('body').css('overflow', '');
        }, 500);
        
        this.mostrarAlerta('info', 'Modales limpiados correctamente');
    }
};
