// InquilinoDataTablesConfig - DataTables configuration specific for Inquilinos
class InquilinoDataTablesConfig {
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
                data: 'direccion',
                title: 'Dirección',
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
                    // Solo mostrar botón de eliminar para Administradores
                    // Usar el valor del servidor como fuente de verdad
                    const canDelete = row.canDelete || false;
                    
                    const deleteButton = canDelete
                        ? `<button type="button" class="btn btn-outline-danger btn-sm" onclick="showDeleteModal(${row.id})" title="Eliminar">
                               <i class="fas fa-trash"></i>
                           </button>`
                        : '';
                    
                    return `
                        <div class="d-flex justify-content-center gap-1">
                            <button type="button" class="btn btn-outline-info btn-sm" onclick="showDetailsModal(${row.id})" title="Ver detalles">
                                <i class="fas fa-eye"></i>
                            </button>
                            <button type="button" class="btn btn-outline-warning btn-sm" onclick="showEditModal(${row.id})" title="Editar">
                                <i class="fas fa-edit"></i>
                            </button>
                            ${deleteButton}
                        </div>
                    `;
                }
            }
        ];
    }

    static getAjaxConfig() {
        return {
            url: '/Inquilinos/GetInquilinosData',
            type: 'POST',
            contentType: 'application/json',
            data: function(d) {
                // Get filter values from form
                const estadoSelect = document.getElementById('filtroEstado');
                const buscarInput = document.getElementById('filtroBuscar');
                
                return JSON.stringify({
                    draw: d.draw,
                    start: d.start,
                    length: d.length,
                    search: d.search,
                    order: d.order,
                    columns: d.columns,
                    // Custom filters
                    estado: estadoSelect ? estadoSelect.value : 'Activo',
                    buscar: buscarInput ? buscarInput.value : ''
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
        return [[5, 'desc']]; // Order by fecha creacion desc
    }
}
