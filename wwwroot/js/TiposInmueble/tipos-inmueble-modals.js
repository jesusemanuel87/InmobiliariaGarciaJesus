// Gestión de modales para tipos de inmueble
const TiposInmuebleModals = {
    // Mostrar modal de creación
    mostrarCrear: function() {
        this.cargarModal('/TiposInmueble/FormModal', 'Crear nuevo tipo');
    },

    // Mostrar modal de edición
    mostrarEditar: function(id) {
        this.cargarModal(`/TiposInmueble/FormModal?id=${id}`, 'Editar tipo');
    },

    // Mostrar modal de detalles
    mostrarDetalles: function(id) {
        this.cargarModal(`/TiposInmueble/DetailsModal?id=${id}`, 'Detalles del tipo');
    },

    // Mostrar modal de eliminación
    mostrarEliminar: function(id) {
        this.cargarModal(`/TiposInmueble/DeleteModal?id=${id}`, 'Eliminar tipo');
    },

    // Cargar modal genérico
    cargarModal: function(url, titulo) {
        TiposInmuebleUtils.limpiarAlertas();
        
        // Mostrar loading
        $('#modalContainer').html(`
            <div class="modal fade show" style="display: block; background: rgba(0,0,0,0.5);">
                <div class="modal-dialog modal-dialog-centered">
                    <div class="modal-content">
                        <div class="modal-body text-center py-5">
                            <div class="spinner-border text-primary mb-3" role="status">
                                <span class="visually-hidden">Cargando...</span>
                            </div>
                            <h5>Cargando ${titulo.toLowerCase()}...</h5>
                        </div>
                    </div>
                </div>
            </div>
        `);

        // Cargar contenido del modal
        $.get(url)
            .done((data) => {
                $('#modalContainer').html(data);
                this.mostrarModal();
            })
            .fail((xhr) => {
                let mensaje = 'Error al cargar el modal.';
                if (xhr.status === 404) {
                    mensaje = 'El tipo solicitado no fue encontrado.';
                } else if (xhr.status === 500) {
                    mensaje = 'Error interno del servidor.';
                }
                
                this.mostrarModalError(mensaje);
            });
    },

    // Mostrar modal (ya cargado)
    mostrarModal: function() {
        const modalElement = $('#modalContainer .modal').first();
        if (modalElement.length) {
            const modal = new bootstrap.Modal(modalElement[0]);
            modal.show();
            
            // Limpiar contenedor al cerrar
            modalElement.on('hidden.bs.modal', () => {
                $('#modalContainer').empty();
            });
        }
    },

    // Mostrar modal de error
    mostrarModalError: function(mensaje) {
        $('#modalContainer').html(`
            <div class="modal fade show" style="display: block; background: rgba(0,0,0,0.5);">
                <div class="modal-dialog modal-dialog-centered">
                    <div class="modal-content">
                        <div class="modal-header bg-danger text-white">
                            <h5 class="modal-title">Error</h5>
                            <button type="button" class="btn-close btn-close-white" onclick="tiposInmuebleModals.cerrarModal()"></button>
                        </div>
                        <div class="modal-body text-center">
                            <i class="fas fa-exclamation-triangle fa-3x text-danger mb-3"></i>
                            <h5>${mensaje}</h5>
                            <p class="text-muted">Por favor, intenta nuevamente.</p>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" onclick="tiposInmuebleModals.cerrarModal()">Cerrar</button>
                        </div>
                    </div>
                </div>
            </div>
        `);
    },

    // Cerrar modal
    cerrarModal: function() {
        // Cerrar modal de Bootstrap si existe
        const modalElement = $('#modalContainer .modal').first();
        if (modalElement.length) {
            const modal = bootstrap.Modal.getInstance(modalElement[0]);
            if (modal) {
                modal.hide();
            }
        }
        
        // Limpiar contenedor después de un pequeño delay
        setTimeout(() => {
            $('#modalContainer').empty();
            // Asegurar que se remueva el backdrop
            $('.modal-backdrop').remove();
            $('body').removeClass('modal-open');
            $('body').css('padding-right', '');
        }, 300);
    },

    // Configurar formulario (llamado desde el modal)
    configurarFormulario: function() {
        const form = $('#tipoInmuebleForm');
        if (form.length) {
            form.on('submit', (e) => {
                e.preventDefault();
                this.enviarFormulario();
            });

            // Validación en tiempo real
            $('#tipoNombre').on('blur', () => {
                const nombre = $('#tipoNombre').val().trim();
                if (nombre && nombre.length > 50) {
                    TiposInmuebleUtils.mostrarErrorCampo('tipoNombre', 'El nombre no puede exceder 50 caracteres');
                } else {
                    TiposInmuebleUtils.limpiarErrorCampo('tipoNombre');
                }
            });

            $('#tipoDescripcion').on('input', () => {
                const descripcion = $('#tipoDescripcion').val();
                if (descripcion.length > 200) {
                    TiposInmuebleUtils.mostrarErrorCampo('tipoDescripcion', 'La descripción no puede exceder 200 caracteres');
                } else {
                    TiposInmuebleUtils.limpiarErrorCampo('tipoDescripcion');
                }
            });
        }
    },

    // Enviar formulario
    enviarFormulario: function() {
        if (!TiposInmuebleUtils.validarFormulario()) {
            return;
        }

        const datos = TiposInmuebleUtils.obtenerDatosFormulario();
        const esEdicion = datos.Id > 0;

        TiposInmuebleUtils.toggleBotones(true);

        $.ajax({
            url: '/TiposInmueble/Save',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(datos),
            success: (response) => {
                if (response.success) {
                    TiposInmuebleUtils.mostrarAlerta('success', response.message);
                    this.cerrarModal();
                    TiposInmuebleDatatables.recargar();
                } else {
                    TiposInmuebleUtils.mostrarAlerta('error', response.message);
                }
            },
            error: (xhr) => {
                let mensaje = 'Error al guardar el tipo de inmueble.';
                if (xhr.responseJSON && xhr.responseJSON.message) {
                    mensaje = xhr.responseJSON.message;
                }
                TiposInmuebleUtils.mostrarAlerta('error', mensaje);
            },
            complete: () => {
                TiposInmuebleUtils.toggleBotones(false);
                TiposInmuebleUtils.restaurarBoton(esEdicion);
            }
        });
    },

    // Confirmar eliminación
    confirmarEliminacion: function(id) {
        const datos = { Id: id };

        // Deshabilitar botón
        $('#confirmDeleteButton').prop('disabled', true).html('<i class="fas fa-spinner fa-spin me-1"></i>Eliminando...');

        $.ajax({
            url: '/TiposInmueble/Delete',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(datos),
            success: (response) => {
                if (response.success) {
                    TiposInmuebleUtils.mostrarAlerta('success', response.message);
                    this.cerrarModal();
                    TiposInmuebleDatatables.recargar();
                } else {
                    TiposInmuebleUtils.mostrarAlerta('error', response.message);
                }
            },
            error: (xhr) => {
                let mensaje = 'Error al eliminar el tipo de inmueble.';
                if (xhr.responseJSON && xhr.responseJSON.message) {
                    mensaje = xhr.responseJSON.message;
                }
                TiposInmuebleUtils.mostrarAlerta('error', mensaje);
            },
            complete: () => {
                $('#confirmDeleteButton').prop('disabled', false).html('<i class="fas fa-trash me-1"></i>Eliminar');
            }
        });
    }
};

// Alias para compatibilidad
const tiposInmuebleModals = TiposInmuebleModals;
