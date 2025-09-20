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
                className: 'text-end',
                render: function(data, type, row) {
                    const expandButton = row.hasInmuebles 
                        ? `<button type="button" class="btn btn-outline-primary btn-sm dt-expand-btn" data-id="${row.id}" title="Ver inmuebles">
                               <i class="fas fa-home"></i>
                           </button>`
                        : '';
                    
                    // Solo mostrar botón de eliminar para Administradores
                    // Usar el valor del servidor como fuente de verdad
                    const canDelete = row.canDelete || false;
                    
                    const deleteButton = canDelete
                        ? `<button type="button" class="btn btn-outline-danger btn-sm" onclick="showDeleteModal(${row.id})" title="Eliminar">
                               <i class="fas fa-trash"></i>
                           </button>`
                        : '';
                    
                    return `
                        <div class="d-flex justify-content-end gap-1">
                            ${expandButton}
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

    static formatChildRow(data) {
        if (!data || data.length === 0) {
            return '<div class="p-2 text-muted text-center bg-light">No hay inmuebles registrados para este propietario</div>';
        }

        let html = `
            <div class="child-row-content">
                <div class="bg-primary text-white px-3 py-2">
                    <h6 class="mb-0"><i class="fas fa-home me-2"></i>Inmuebles del Propietario</h6>
                </div>
                <div class="p-3">
                    <div class="table-responsive">
                        <table class="table table-sm table-striped mb-0">
                            <thead class="table-light">
                                <tr>
                                    <th>Dirección</th>
                                    <th>Tipo</th>
                                    <th>Uso</th>
                                    <th>Estado</th>
                                    <th>Estado Contrato</th>
                                    <th class="text-end">Precio</th>
                                </tr>
                            </thead>
                            <tbody>
        `;

        data.forEach(inmueble => {
            html += `
                <tr>
                    <td>${inmueble.direccion}</td>
                    <td><span class="badge bg-info">${inmueble.tipo}</span></td>
                    <td><span class="badge bg-secondary">${inmueble.uso}</span></td>
                    <td><span class="badge bg-success">${inmueble.estado}</span></td>
                    <td><span class="badge ${inmueble.estadoContratoCss}">${inmueble.estadoContrato}</span></td>
                    <td class="text-end">$${inmueble.precio.toLocaleString('es-AR')}</td>
                </tr>
            `;
        });

        html += `
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        `;

        return html;
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
