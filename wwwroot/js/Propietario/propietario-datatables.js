// PropietarioDataTablesConfig - DataTables configuration specific for Propietarios
class PropietarioDataTablesConfig {
    static getColumns() {
        return [
            { 
                data: 'dni',
                title: 'DNI',
                orderable: true
            },
            { 
                data: 'nombreCompleto',
                title: 'Nombre Completo',
                orderable: true
            },
            { 
                data: 'email',
                title: 'Email',
                orderable: true
            },
            { 
                data: 'telefono',
                title: 'Teléfono',
                orderable: true
            },
            { 
                data: 'fechaCreacion',
                title: 'Fecha Creación',
                orderable: true,
                render: function(data) {
                    return DataTablesConfig.formatDate(data);
                }
            },
            { 
                data: 'estado',
                title: 'Estado',
                orderable: true,
                render: function(data) {
                    return data 
                        ? '<span class="badge bg-success"><i class="fas fa-check-circle"></i> Activo</span>'
                        : '<span class="badge bg-secondary"><i class="fas fa-times-circle"></i> Inactivo</span>';
                }
            },
            {
                data: null,
                title: 'Acciones',
                orderable: false,
                render: function(data, type, row) {
                    return `
                        <div class="btn-group" role="group">
                            <button type="button" class="btn btn-sm btn-outline-info" onclick="showDetailsModal(${row.id})" title="Ver detalles">
                                <i class="fas fa-eye"></i>
                            </button>
                            <button type="button" class="btn btn-sm btn-outline-warning" onclick="showEditModal(${row.id})" title="Editar">
                                <i class="fas fa-edit"></i>
                            </button>
                            <button type="button" class="btn btn-sm btn-outline-danger" onclick="showDeleteModal(${row.id})" title="Eliminar">
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
            url: '/Propietarios/GetPropietariosData',
            type: 'POST',
            contentType: 'application/json',
            data: function(d) {
                return JSON.stringify({
                    draw: d.draw,
                    start: d.start,
                    length: d.length,
                    search: d.search,
                    order: d.order,
                    columns: d.columns
                });
            },
            dataSrc: function(json) {
                console.log('DataTables response:', json);
                if (json.error) {
                    console.error('Server error:', json.error);
                    return [];
                }
                return json.data || [];
            },
            error: function(xhr, error, code) {
                console.error('DataTables Ajax error:', {
                    xhr: xhr,
                    error: error,
                    code: code,
                    status: xhr.status,
                    responseText: xhr.responseText
                });
            }
        };
    }

    static getDefaultOrder() {
        return [[4, 'desc']]; // Order by fecha creacion desc
    }
}
