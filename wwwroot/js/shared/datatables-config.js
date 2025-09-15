// Configuración reutilizable de DataTables
class DataTablesConfig {
    static getDefaultConfig() {
        return {
            processing: true,
            serverSide: false,
            responsive: true,
            pageLength: 10,
            lengthMenu: [[10, 25, 50, 100], [10, 25, 50, 100]],
            language: {
                url: 'https://cdn.datatables.net/plug-ins/1.13.7/i18n/es-ES.json'
            },
            dom: '<"row"<"col-sm-12 col-md-6"l><"col-sm-12 col-md-6"f>>' +
                 '<"row"<"col-sm-12"tr>>' +
                 '<"row"<"col-sm-12 col-md-5"i><"col-sm-12 col-md-7"p>>',
            columnDefs: [
                {
                    targets: 'no-sort',
                    orderable: false
                },
                {
                    targets: 'text-center',
                    className: 'text-center'
                }
            ]
        };
    }

    static getAjaxConfig(ajaxUrl, columns, customConfig = {}) {
        const defaultConfig = this.getDefaultConfig();
        
        return {
            ...defaultConfig,
            serverSide: true,
            ajax: {
                url: ajaxUrl,
                type: 'POST',
                data: function(d) {
                    // Transformar parámetros de DataTables para el servidor
                    return {
                        draw: d.draw,
                        start: d.start,
                        length: d.length,
                        search: d.search.value,
                        orderColumn: d.order[0]?.column || 0,
                        orderDirection: d.order[0]?.dir || 'asc'
                    };
                },
                error: function(xhr, error, thrown) {
                    console.error('Error en DataTables AJAX:', error);
                    alert('Error al cargar los datos. Por favor, recargue la página.');
                }
            },
            columns: columns,
            ...customConfig
        };
    }

    static initializeTable(tableSelector, config) {
        // Destruir tabla existente si existe
        if ($.fn.DataTable.isDataTable(tableSelector)) {
            $(tableSelector).DataTable().destroy();
        }

        // Inicializar nueva tabla
        return $(tableSelector).DataTable(config);
    }

    static getActionButtons(id, actions = ['details', 'edit', 'delete']) {
        const buttons = [];
        
        if (actions.includes('details')) {
            buttons.push(`
                <button type="button" class="btn btn-outline-info btn-sm me-1" 
                        onclick="showDetailsModal(${id})" title="Ver detalles">
                    <i class="fas fa-eye"></i>
                </button>
            `);
        }
        
        if (actions.includes('edit')) {
            buttons.push(`
                <button type="button" class="btn btn-outline-warning btn-sm me-1" 
                        onclick="showEditModal(${id})" title="Editar">
                    <i class="fas fa-edit"></i>
                </button>
            `);
        }
        
        if (actions.includes('delete')) {
            buttons.push(`
                <button type="button" class="btn btn-outline-danger btn-sm" 
                        onclick="showDeleteModal(${id})" title="Eliminar">
                    <i class="fas fa-trash"></i>
                </button>
            `);
        }
        
        return `<div class="btn-group" role="group">${buttons.join('')}</div>`;
    }

    static formatDate(dateString) {
        if (!dateString) return '';
        const date = new Date(dateString);
        return date.toLocaleDateString('es-ES');
    }

    static formatBadge(value, type = 'success', falseType = 'secondary') {
        const badgeType = value ? type : falseType;
        const text = value ? 'Activo' : 'Inactivo';
        return `<span class="badge bg-${badgeType}">${text}</span>`;
    }

    static getSpanishLanguage() {
        return {
            processing: "Procesando...",
            search: "Buscar:",
            lengthMenu: "Mostrar _MENU_ registros",
            info: "Mostrando registros del _START_ al _END_ de un total de _TOTAL_ registros",
            infoEmpty: "Mostrando registros del 0 al 0 de un total de 0 registros",
            infoFiltered: "(filtrado de un total de _MAX_ registros)",
            infoPostFix: "",
            loadingRecords: "Cargando...",
            zeroRecords: "No se encontraron resultados",
            emptyTable: "Ningún dato disponible en esta tabla",
            paginate: {
                first: "Primero",
                previous: "Anterior",
                next: "Siguiente",
                last: "Último"
            },
            aria: {
                sortAscending: ": Activar para ordenar la columna de manera ascendente",
                sortDescending: ": Activar para ordenar la columna de manera descendente"
            }
        };
    }
}

// Hacer disponible globalmente
window.DataTablesConfig = DataTablesConfig;
