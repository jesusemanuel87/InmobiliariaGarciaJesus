// PropietarioIndexManager - Main class for managing propietarios CRUD operations
class PropietarioIndexManager {
    constructor() {
        this.dataTable = null;
        this.filters = null;
        this.init();
    }

    init() {
        $(document).ready(() => {
            this.initializeDataTable();
            this.bindEvents();
            console.log('PropietarioIndexManager initialized');
        });
    }

    initializeDataTable() {
        const columns = PropietarioDataTablesConfig.getColumns();
        
        this.dataTable = $('#propietariosTable').DataTable({
            ...DataTablesConfig.getDefaultConfig(),
            ajax: PropietarioDataTablesConfig.getAjaxConfig(),
            columns: columns,
            order: PropietarioDataTablesConfig.getDefaultOrder()
        });
        
        // Initialize filters with a small delay to ensure everything is ready
        setTimeout(() => {
            this.initializeFilters();
        }, 500);
    }

    initializeFilters() {
        if (!this.filters) {
            this.filters = new PropietariosFilters();
            // Make filters globally available
            window.propietariosFilters = this.filters;
        }
        
        // Add event listener for opening and closing details
        $('#propietariosTable tbody').on('click', '.dt-expand-btn', (e) => {
            e.preventDefault();
            e.stopPropagation();
            
            const button = $(e.currentTarget);
            const tr = button.closest('tr');
            const row = this.dataTable.row(tr);
            const icon = button.find('i');
            
            if (row.child.isShown()) {
                // This row is already open - close it
                row.child.hide();
                tr.removeClass('shown');
                icon.removeClass('fa-minus').addClass('fa-home');
                button.removeClass('btn-outline-secondary').addClass('btn-outline-primary');
            } else {
                // Open this row
                const rowData = row.data();
                if (rowData.hasInmuebles) {
                    this.loadInmueblesData(row, tr, button);
                }
            }
        });
    }

    loadInmueblesData(row, tr, button) {
        const rowData = row.data();
        const icon = button.find('i');
        
        // Show loading state
        icon.removeClass('fa-home').addClass('fa-spinner fa-spin');
        button.removeClass('btn-outline-primary').addClass('btn-outline-secondary');
        
        $.get(`/Propietarios/GetInmueblesData/${rowData.id}`)
            .done((response) => {
                if (response.success) {
                    const childContent = PropietarioDataTablesConfig.formatChildRow(response.data);
                    row.child(childContent).show();
                    tr.addClass('shown');
                    icon.removeClass('fa-spinner fa-spin').addClass('fa-minus');
                    button.removeClass('btn-outline-primary').addClass('btn-outline-secondary');
                } else {
                    this.showAlert('danger', 'Error al cargar los inmuebles: ' + response.message);
                    icon.removeClass('fa-spinner fa-spin').addClass('fa-home');
                    button.removeClass('btn-outline-secondary').addClass('btn-outline-primary');
                }
            })
            .fail(() => {
                this.showAlert('danger', 'Error al cargar los inmuebles del propietario');
                icon.removeClass('fa-spinner fa-spin').addClass('fa-home');
                button.removeClass('btn-outline-secondary').addClass('btn-outline-primary');
            });
    }

    bindEvents() {
        // Bind create button
        $('#btnCreatePropietario').on('click', () => {
            this.showCreateModal();
        });

        // Bind toggle inmuebles events (consolidado desde index-propietario.js)
        this.bindToggleInmueblesEvents();

        // Auto-dismiss alerts
        setTimeout(() => {
            $('.alert').fadeOut();
        }, 5000);
    }

    bindToggleInmueblesEvents() {
        $(document).ready(() => {
            $('.toggle-inmuebles').click((e) => {
                this.handleToggleInmuebles(e);
            });
        });
    }

    handleToggleInmuebles(event) {
        const button = $(event.currentTarget);
        const propietarioId = button.data('propietario-id');
        const container = $('.inmuebles-container[data-propietario-id="' + propietarioId + '"]');
        const content = container.find('.inmuebles-content');
        const icon = button.find('.toggle-icon');
        
        if (container.is(':visible')) {
            // Hide inmuebles
            container.slideUp();
            icon.removeClass('fa-chevron-up').addClass('fa-chevron-down');
        } else {
            // Show inmuebles
            if (content.is(':empty')) {
                // Load inmuebles via AJAX
                content.html('<div class="text-center p-3"><i class="fas fa-spinner fa-spin"></i> Cargando inmuebles...</div>');
                
                $.get('/Propietarios/GetInmuebles', { id: propietarioId })
                    .done((data) => {
                        content.html('<table class="table table-sm mb-0">' + data + '</table>');
                        container.slideDown();
                        icon.removeClass('fa-chevron-down').addClass('fa-chevron-up');
                    })
                    .fail(() => {
                        content.html('<div class="text-center p-3 text-danger"><i class="fas fa-exclamation-triangle"></i> Error al cargar inmuebles</div>');
                        container.slideDown();
                        icon.removeClass('fa-chevron-down').addClass('fa-chevron-up');
                    });
            } else {
                container.slideDown();
                icon.removeClass('fa-chevron-down').addClass('fa-chevron-up');
            }
        }
    }

    showCreateModal() {
        $.get('/Propietarios/FormModal')
            .done((data) => {
                $('#modalContainer').html(data);
                this.setupModal(false);
                $('#modalPropietario').modal('show');
            })
            .fail(() => {
                this.showAlert('danger', 'Error al cargar el formulario de creación');
            });
    }

    showEditModal(id) {
        $.get(`/Propietarios/FormModal/${id}`)
            .done((data) => {
                $('#modalContainer').html(data);
                this.setupModal(true, data);
                $('#modalPropietario').modal('show');
            })
            .fail(() => {
                this.showAlert('danger', 'Error al cargar el formulario de edición');
            });
    }

    setupModal(isEdit, data = null) {
        if (isEdit) {
            $('#modalTitle').text('Editar Propietario');
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
            $('#modalTitle').text('Nuevo Propietario');
            $('#btnGuardar').html('<i class="fas fa-save"></i> Crear');
            $('#auditInfo').hide();
            
            // Limpiar formulario
            $('#formPropietario')[0].reset();
            $('#propietarioId').val('');
            $('#estado').prop('checked', true);
        }
    }

    showDetailsModal(id) {
        $.get(`/Propietarios/DetailsModal/${id}`)
            .done((data) => {
                $('#modalContainer').html(data);
                $('#detailsPropietarioModal').modal('show');
            })
            .fail(() => {
                this.showAlert('danger', 'Error al cargar los detalles');
            });
    }

    showDeleteModal(id) {
        $.get(`/Propietarios/DeleteModal/${id}`)
            .done((data) => {
                $('#modalContainer').html(data);
                $('#deletePropietarioModal').modal('show');
            })
            .fail(() => {
                this.showAlert('danger', 'Error al cargar el formulario de eliminación');
            });
    }

    submitForm() {
        console.log('submitForm called');
        
        const formData = {
            Id: $('#propietarioId').val() || 0,
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
            url: '/Propietarios/Save',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: (response) => {
                console.log('Success response:', response);
                if (response.success) {
                    $('#modalPropietario').modal('hide');
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
        const form = $('#deletePropietarioForm');
        const formData = new FormData(form[0]);

        $.ajax({
            url: '/Propietarios/Delete',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: (response) => {
                if (response.success) {
                    $('#deletePropietarioModal').modal('hide');
                    this.dataTable.ajax.reload();
                    this.showAlert('success', response.message);
                } else {
                    this.showAlert('danger', response.message);
                }
            },
            error: () => {
                this.showAlert('danger', 'Error al eliminar el propietario');
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
