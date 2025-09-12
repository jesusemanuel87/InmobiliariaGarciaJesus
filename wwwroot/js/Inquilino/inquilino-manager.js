// InquilinoIndexManager - Main class for managing inquilinos CRUD operations
class InquilinoIndexManager {
    constructor() {
        this.dataTable = null;
        this.init();
    }

    init() {
        $(document).ready(() => {
            this.initializeDataTable();
            this.bindEvents();
            console.log('InquilinoIndexManager initialized');
        });
    }

    initializeDataTable() {
        const columns = InquilinoDataTablesConfig.getColumns();
        
        this.dataTable = $('#inquilinosTable').DataTable({
            ...DataTablesConfig.getDefaultConfig(),
            ajax: InquilinoDataTablesConfig.getAjaxConfig(),
            columns: columns,
            order: InquilinoDataTablesConfig.getDefaultOrder()
        });
    }

    bindEvents() {
        // Bind create button
        $('#btnCreateInquilino').on('click', () => {
            this.showCreateModal();
        });

        // Auto-dismiss alerts
        setTimeout(() => {
            $('.alert').fadeOut();
        }, 5000);
    }

    showCreateModal() {
        $.get('/Inquilinos/FormModal')
            .done((data) => {
                $('#modalContainer').html(data);
                this.setupModal(false);
                $('#modalInquilino').modal('show');
            })
            .fail(() => {
                this.showAlert('danger', 'Error al cargar el formulario de creación');
            });
    }

    showEditModal(id) {
        $.get(`/Inquilinos/FormModal/${id}`)
            .done((data) => {
                $('#modalContainer').html(data);
                this.setupModal(true, data);
                $('#modalInquilino').modal('show');
            })
            .fail(() => {
                this.showAlert('danger', 'Error al cargar el formulario de edición');
            });
    }

    setupModal(isEdit, data = null) {
        if (isEdit) {
            $('#modalTitle').text('Editar Inquilino');
            $('#btnGuardar').html('<i class="fas fa-save"></i> Actualizar');
            $('#auditInfo').show();
            
            // Poblar campos si hay data del modelo
            if (data) {
                // Los campos ya vienen poblados desde el servidor
                const fechaCreacion = $('#fechaCreacion').val();
                if (fechaCreacion) {
                    const fecha = new Date(fechaCreacion).toLocaleDateString('es-ES', {
                        year: 'numeric',
                        month: 'long',
                        day: 'numeric',
                        hour: '2-digit',
                        minute: '2-digit'
                    });
                    $('#fechaCreacionText').text(`Creado el ${fecha}`);
                }
            }
        } else {
            $('#modalTitle').text('Nuevo Inquilino');
            $('#btnGuardar').html('<i class="fas fa-save"></i> Crear');
            $('#auditInfo').hide();
            
            // Limpiar formulario
            $('#formInquilino')[0].reset();
            $('#inquilinoId').val('');
            $('#estado').prop('checked', true);
        }
    }

    showDetailsModal(id) {
        $.get(`/Inquilinos/DetailsModal/${id}`)
            .done((data) => {
                $('#modalContainer').html(data);
                $('#detailsInquilinoModal').modal('show');
            })
            .fail(() => {
                this.showAlert('danger', 'Error al cargar los detalles');
            });
    }

    showDeleteModal(id) {
        $.get(`/Inquilinos/DeleteModal/${id}`)
            .done((data) => {
                $('#modalContainer').html(data);
                $('#deleteInquilinoModal').modal('show');
            })
            .fail(() => {
                this.showAlert('danger', 'Error al cargar el formulario de eliminación');
            });
    }

    submitForm() {
        console.log('submitForm called');
        
        const formData = {
            Id: $('#inquilinoId').val() || 0,
            Nombre: $('#nombre').val(),
            Apellido: $('#apellido').val(),
            Dni: $('#dni').val(),
            Telefono: $('#telefono').val(),
            Email: $('#email').val(),
            Direccion: $('#direccion').val(),
            Estado: $('#estado').is(':checked'),
            FechaCreacion: $('#fechaCreacion').val() || new Date().toISOString()
        };

        console.log('Form data:', formData);

        $.ajax({
            url: '/Inquilinos/Save',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: (response) => {
                console.log('Success response:', response);
                if (response.success) {
                    $('#modalInquilino').modal('hide');
                    this.dataTable.ajax.reload();
                    this.showAlert('success', response.message);
                } else {
                    this.displayValidationErrors(response.errors);
                }
            },
            error: (xhr, status, error) => {
                console.log('Error response:', xhr, status, error);
                this.showAlert('danger', 'Error al procesar el formulario');
            }
        });
    }

    submitDeleteForm() {
        const form = $('#deleteInquilinoForm');
        const formData = new FormData(form[0]);

        $.ajax({
            url: '/Inquilinos/Delete',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: (response) => {
                if (response.success) {
                    $('#deleteInquilinoModal').modal('hide');
                    this.dataTable.ajax.reload();
                    this.showAlert('success', response.message);
                } else {
                    this.showAlert('danger', response.message);
                }
            },
            error: () => {
                this.showAlert('danger', 'Error al eliminar el inquilino');
            }
        });
    }

    displayValidationErrors(errors) {
        // Clear previous errors
        $('.is-invalid').removeClass('is-invalid');
        $('.text-danger').remove();

        // Display new errors
        for (const [field, messages] of Object.entries(errors)) {
            const input = $(`#${field.toLowerCase()}`);
            if (input.length) {
                input.addClass('is-invalid');
                const errorDiv = $('<div class="text-danger"></div>').text(messages[0]);
                input.after(errorDiv);
            }
        }
    }

    showAlert(type, message) {
        const alertHtml = `
            <div class="alert alert-${type} alert-dismissible fade show" role="alert">
                <i class="fas fa-${type === 'success' ? 'check-circle' : 'exclamation-triangle'}"></i>
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        `;
        
        $('#alertContainer').html(alertHtml);
        
        // Auto dismiss after 5 seconds
        setTimeout(() => {
            $('.alert').fadeOut();
        }, 5000);
    }
}
