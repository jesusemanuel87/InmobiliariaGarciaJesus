// EmpleadoDataTablesConfig - DataTables configuration for Empleados module
class EmpleadoDataTablesConfig {
    static getColumns() {
        return [
            {
                data: null,
                title: '',
                orderable: false,
                searchable: false,
                width: '40px',
                className: 'text-center',
                render: function(data, type, row) {
                    return `
                        <button class="btn btn-sm btn-outline-info dt-expand-btn" 
                                title="Ver detalles" 
                                data-id="${row.id}">
                            <i class="fas fa-plus"></i>
                        </button>
                    `;
                }
            },
            {
                data: 'dni',
                title: 'DNI',
                width: '100px',
                className: 'text-center'
            },
            {
                data: 'nombreCompleto',
                title: 'Nombre Completo',
                render: function(data, type, row) {
                    return `<strong>${data}</strong>`;
                }
            },
            {
                data: 'email',
                title: 'Email',
                render: function(data, type, row) {
                    return `<a href="mailto:${data}" class="text-decoration-none">${data}</a>`;
                }
            },
            {
                data: 'rol',
                title: 'Rol',
                width: '120px',
                className: 'text-center',
                render: function(data, type, row) {
                    const badgeClass = data === 'Administrador' ? 'bg-danger' : 'bg-primary';
                    return `<span class="badge ${badgeClass}">${data}</span>`;
                }
            },
            {
                data: 'estado',
                title: 'Estado',
                width: '100px',
                className: 'text-center',
                render: function(data, type, row) {
                    const badgeClass = data ? 'bg-success' : 'bg-secondary';
                    const text = data ? 'Activo' : 'Inactivo';
                    return `<span class="badge ${badgeClass}">${text}</span>`;
                }
            },
            {
                data: null,
                title: 'Acciones',
                orderable: false,
                searchable: false,
                width: '120px',
                className: 'text-center',
                render: function(data, type, row) {
                    return `
                        <div class="btn-group" role="group">
                            <button type="button" class="btn btn-sm btn-outline-info" 
                                    onclick="showDetailsModal(${row.id})" 
                                    title="Ver detalles">
                                <i class="fas fa-eye"></i>
                            </button>
                            <button type="button" class="btn btn-sm btn-outline-warning" 
                                    onclick="showEditModal(${row.id})" 
                                    title="Editar">
                                <i class="fas fa-edit"></i>
                            </button>
                            <button type="button" class="btn btn-sm btn-outline-danger" 
                                    onclick="showDeleteModal(${row.id})" 
                                    title="Eliminar">
                                <i class="fas fa-trash"></i>
                            </button>
                        </div>
                    `;
                }
            }
        ];
    }

    static getAjaxConfig() {
        return {
            url: '/Empleados/GetEmpleadosData',
            type: 'POST',
            contentType: 'application/json',
            data: function(d) {
                // Add custom filters
                d.estado = $('#filtroEstado').val();
                d.rol = $('#filtroRol').val();
                d.buscar = $('#filtroBuscar').val();
                
                return JSON.stringify(d);
            },
            error: function(xhr, error, code) {
                console.log('DataTables Ajax error:', {xhr, error, code});
            },
            complete: function(xhr, status) {
                if (xhr.responseJSON) {
                    console.log('DataTables response:', xhr.responseJSON);
                }
            }
        };
    }

    static getDefaultOrder() {
        return [[2, 'asc']]; // Order by nombre completo
    }
}
