// EmpleadoIndexManager - Main class for managing empleados CRUD operations
class EmpleadoIndexManager {
    constructor() {
        this.dataTable = null;
        this.filters = null;
        this.init();
    }

    init() {
        $(document).ready(() => {
            this.initializeDataTable();
            this.bindEvents();
            console.log('EmpleadoIndexManager initialized');
        });
    }

    initializeDataTable() {
        const columns = EmpleadoDataTablesConfig.getColumns();
        
        this.dataTable = $('#empleadosTable').DataTable({
            ...DataTablesConfig.getDefaultConfig(),
            ajax: EmpleadoDataTablesConfig.getAjaxConfig(),
            columns: columns,
            order: EmpleadoDataTablesConfig.getDefaultOrder()
        });
        
        // Initialize filters with a small delay to ensure everything is ready
        setTimeout(() => {
            this.initializeFilters();
        }, 500);
    }

    initializeFilters() {
        if (!this.filters) {
            this.filters = new EmpleadosFilters();
            // Make filters globally available
            window.empleadosFilters = this.filters;
        }
        
        // Add event listener for opening and closing details
        $('#empleadosTable tbody').on('click', '.dt-expand-btn', (e) => {
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
                icon.removeClass('fa-minus').addClass('fa-plus');
            } else {
                // Open this row
                const empleadoId = button.data('id');
                this.showRowDetails(row, empleadoId, button);
            }
        });
    }

    showRowDetails(row, empleadoId, button) {
        // Show loading
        const loadingHtml = `
            <div class="text-center p-3">
                <i class="fas fa-spinner fa-spin"></i> Cargando detalles...
            </div>
        `;
        
        row.child(loadingHtml).show();
        row.child().addClass('child-row-details');
        
        // Load details via AJAX
        $.get(`/Empleados/GetEmpleadoDetails/${empleadoId}`)
            .done((data) => {
                const detailsHtml = this.formatRowDetails(data);
                row.child(detailsHtml).show();
                button.find('i').removeClass('fa-plus').addClass('fa-minus');
                row.node().classList.add('shown');
            })
            .fail(() => {
                const errorHtml = `
                    <div class="alert alert-danger m-2">
                        <i class="fas fa-exclamation-triangle"></i> Error al cargar los detalles
                    </div>
                `;
                row.child(errorHtml).show();
            });
    }

    formatRowDetails(empleado) {
        return `
            <div class="row-details-container p-3 bg-light">
                <div class="row">
                    <div class="col-md-6">
                        <h6><i class="fas fa-info-circle text-info"></i> Información Personal</h6>
                        <table class="table table-sm table-borderless">
                            <tr>
                                <td><strong>DNI:</strong></td>
                                <td>${empleado.dni}</td>
                            </tr>
                            <tr>
                                <td><strong>Nombre:</strong></td>
                                <td>${empleado.nombre}</td>
                            </tr>
                            <tr>
                                <td><strong>Apellido:</strong></td>
                                <td>${empleado.apellido}</td>
                            </tr>
                            <tr>
                                <td><strong>Email:</strong></td>
                                <td><a href="mailto:${empleado.email}">${empleado.email}</a></td>
                            </tr>
                            <tr>
                                <td><strong>Teléfono:</strong></td>
                                <td>${empleado.telefono || 'No especificado'}</td>
                            </tr>
                        </table>
                    </div>
                    <div class="col-md-6">
                        <h6><i class="fas fa-briefcase text-primary"></i> Información Laboral</h6>
                        <table class="table table-sm table-borderless">
                            <tr>
                                <td><strong>Rol:</strong></td>
                                <td>
                                    <span class="badge ${empleado.rol === 'Administrador' ? 'bg-danger' : 'bg-primary'}">
                                        ${empleado.rol}
                                    </span>
                                </td>
                            </tr>
                            <tr>
                                <td><strong>Estado:</strong></td>
                                <td>
                                    <span class="badge ${empleado.estado ? 'bg-success' : 'bg-secondary'}">
                                        ${empleado.estado ? 'Activo' : 'Inactivo'}
                                    </span>
                                </td>
                            </tr>
                            <tr>
                                <td><strong>Fecha Ingreso:</strong></td>
                                <td>${EmpleadoUtils.formatDate(empleado.fechaCreacion)}</td>
                            </tr>
                        </table>
                    </div>
                </div>
                <div class="row mt-2">
                    <div class="col-12 text-end">
                        <button type="button" class="btn btn-sm btn-outline-info me-2" 
                                onclick="showDetailsModal(${empleado.id})">
                            <i class="fas fa-eye"></i> Ver Completo
                        </button>
                        <button type="button" class="btn btn-sm btn-outline-warning" 
                                onclick="showEditModal(${empleado.id})">
                            <i class="fas fa-edit"></i> Editar
                        </button>
                    </div>
                </div>
            </div>
        `;
    }

    bindEvents() {
        // Bind create button
        $('#btnCreateEmpleado').on('click', () => {
            this.showCreateModal();
        });

        // Auto-dismiss alerts
        setTimeout(() => {
            $('.alert').alert('close');
        }, 5000);
    }

    showCreateModal() {
        showCreateModal();
    }

    // Method to refresh the DataTable
    refreshTable() {
        if (this.dataTable) {
            this.dataTable.ajax.reload();
        }
    }

    // Method to get selected rows (if needed for future features)
    getSelectedRows() {
        return this.dataTable.rows('.selected').data().toArray();
    }
}
