// Pagos DataTables Configuration
class PagosDataTablesConfig {
    static getColumns() {
        return [
            {
                data: 'contratoId',
                title: 'Contrato',
                className: 'text-end',
                width: '80px',
                render: function(data, type, row) {
                    return `<span class="badge bg-primary">#${data}</span>`;
                }
            },
            {
                data: 'inquilinoNombre',
                title: 'Inquilino',
                render: function(data, type, row) {
                    return `<span>${data}</span>`;
                }
            },
            {
                data: 'inmuebleDireccion',
                title: 'Inmueble',
                render: function(data, type, row) {
                    return `<span>${data}</span>`;
                }
            },
            {
                data: 'fechaVencimiento',
                title: 'Vencimiento',
                render: function(data, type, row) {
                    const fecha = new Date(data);
                    const fechaFormateada = fecha.toLocaleDateString('es-AR');
                    const hoy = new Date();
                    hoy.setHours(0, 0, 0, 0); // Reset time to compare only dates
                    fecha.setHours(0, 0, 0, 0); // Reset time to compare only dates
                    const esVencido = fecha < hoy;
                    
                    return `<span class="badge ${esVencido ? 'bg-danger' : 'bg-info'}">
                        <i class="fas fa-calendar me-1"></i>
                        ${fechaFormateada}
                    </span>`;
                }
            },
            {
                data: 'monto',
                title: 'Monto',
                className: 'text-end',
                render: function(data, type, row) {
                    return `<span class="fw-bold text-success">$${parseFloat(data).toLocaleString('es-AR', {maximumFractionDigits: 0})}</span>`;
                }
            },
            {
                data: 'multas',
                title: 'Multas',
                className: 'text-end',
                render: function(data, type, row) {
                    const multa = parseFloat(data) || 0;
                    if (multa > 0) {
                        return `<span class="fw-bold text-danger">$${multa.toLocaleString('es-AR', {maximumFractionDigits: 0})}</span>`;
                    }
                    return '<span class="text-muted">-</span>';
                }
            },
            {
                data: 'intereses',
                title: 'Intereses',
                className: 'text-end',
                render: function(data, type, row) {
                    const interes = parseFloat(data) || 0;
                    if (interes > 0) {
                        return `<span class="fw-bold text-warning">$${interes.toLocaleString('es-AR', {maximumFractionDigits: 0})}</span>`;
                    }
                    return '<span class="text-muted">-</span>';
                }
            },
            {
                data: null,
                title: 'Total a Pagar',
                className: 'text-end',
                render: function(data, type, row) {
                    const monto = parseFloat(row.monto) || 0;
                    const multas = parseFloat(row.multas) || 0;
                    const intereses = parseFloat(row.intereses) || 0;
                    const total = monto + multas + intereses;
                    return `<span class="fw-bold text-primary">$${total.toLocaleString('es-AR', {maximumFractionDigits: 0})}</span>`;
                }
            },
            {
                data: 'estado',
                title: 'Estado',
                render: function(data, type, row) {
                    // Use the actual state from database (backend handles state updates)
                    const badgeClass = data === 'Pagado' ? 'bg-success' : 
                                     data === 'Pendiente' ? 'bg-warning' : 'bg-danger';
                    const icon = data === 'Pagado' ? 'fa-check-circle' : 
                                data === 'Pendiente' ? 'fa-clock' : 'fa-exclamation-triangle';
                    
                    return `<span class="badge ${badgeClass}">
                        <i class="fas ${icon} me-1"></i>
                        ${data}
                    </span>`;
                }
            },
            {
                data: null,
                title: 'Acciones',
                orderable: false,
                className: 'text-end',
                render: function(data, type, row) {
                    let actions = `<div class="d-flex justify-content-end gap-1">
                            <button type="button" class="btn btn-sm btn-outline-info" 
                                    onclick="window.pagosManager.showDetailsModal(${row.id})" 
                                    title="Ver detalles">
                                <i class="fas fa-eye"></i>
                            </button>`;
                    
                    if (row.estado !== 'Pagado') {
                        actions += `
                            <button type="button" class="btn btn-sm btn-outline-success" 
                                    onclick="window.pagosManager.showRegistrarPagoModal(${row.id})" 
                                    title="Registrar pago">
                                <i class="fas fa-dollar-sign"></i>
                            </button>`;
                    }
                    
                    // Botón Editar - disponible para todos los estados
                    actions += `
                        <button type="button" class="btn btn-sm btn-outline-primary" 
                                onclick="window.pagosManager.showEditModal(${row.id})" 
                                title="Editar">
                            <i class="fas fa-edit"></i>
                        </button>`;
                    
                    // Botón Eliminar - solo para pagos pagados
                    if (row.estado === 'Pagado') {
                        actions += `
                            <button type="button" class="btn btn-sm btn-outline-danger" 
                                    onclick="window.pagosManager.showDeleteModal(${row.id})" 
                                    title="Eliminar">
                                <i class="fas fa-trash"></i>
                            </button>`;
                    }
                    
                    actions += `</div>`;
                    return actions;
                }
            }
        ];
    }

    static getAjaxConfig() {
        return {
            url: '/Pagos/GetPagosData',
            type: 'POST',
            contentType: 'application/json',
            data: function(d) {
                // Get filter values
                const filtroEstado = document.getElementById('filtroEstado')?.value || '';
                const filtroEstadoContrato = document.getElementById('filtroEstadoContrato')?.value || '';
                const filtroNumeroContrato = document.getElementById('filtroNumeroContrato')?.value || '';
                const filtroMes = document.getElementById('filtroMes')?.value || '';
                const filtroAnio = document.getElementById('filtroAnio')?.value || '';
                const filtroMonto = document.getElementById('filtroMonto')?.value || '';
                
                return JSON.stringify({
                    draw: d.draw,
                    start: d.start,
                    length: d.length,
                    search: d.search,
                    order: d.order,
                    columns: d.columns,
                    // Custom filters
                    filtroEstado: filtroEstado,
                    filtroEstadoContrato: filtroEstadoContrato,
                    filtroNumeroContrato: filtroNumeroContrato,
                    filtroMes: filtroMes,
                    filtroAnio: filtroAnio,
                    filtroMonto: filtroMonto
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
        return [[3, 'asc']]; // Order by fecha vencimiento asc
    }
}
