// Configuración de DataTables para tipos de inmueble
const TiposInmuebleDatatables = {
    table: null,

    // Inicializar DataTable
    inicializar: function() {
        this.table = $('#tiposInmuebleTable').DataTable({
            ajax: {
                url: '/TiposInmueble/GetTiposInmuebleData',
                type: 'GET',
                dataSrc: 'data'
            },
            columns: [
                { 
                    data: 'id',
                    title: 'ID',
                    width: '60px',
                    className: 'text-center'
                },
                { 
                    data: 'nombre',
                    title: 'Nombre',
                    render: function(data, type, row) {
                        return `<strong class="text-primary">${data}</strong>`;
                    }
                },
                { 
                    data: 'descripcion',
                    title: 'Descripción',
                    render: function(data, type, row) {
                        if (!data || data.trim() === '') {
                            return '<em class="text-muted">Sin descripción</em>';
                        }
                        return TiposInmuebleUtils.truncarTexto(data, 60);
                    }
                },
                { 
                    data: 'estado',
                    title: 'Estado',
                    width: '100px',
                    className: 'text-center',
                    render: function(data, type, row) {
                        return TiposInmuebleUtils.generarBadgeEstado(data);
                    }
                },
                { 
                    data: 'fechaCreacion',
                    title: 'Fecha Creación',
                    width: '140px',
                    className: 'text-center'
                },
                { 
                    data: 'acciones',
                    title: 'Acciones',
                    width: '120px',
                    className: 'text-center',
                    orderable: false,
                    searchable: false,
                    render: function(data, type, row) {
                        return TiposInmuebleUtils.generarBotonesAccion(row.id);
                    }
                }
            ],
            order: [[1, 'asc']], // Ordenar por nombre por defecto
            pageLength: 25,
            lengthMenu: [[10, 25, 50, 100], [10, 25, 50, 100]],
            responsive: true,
            processing: true,
            language: {
                url: '//cdn.datatables.net/plug-ins/1.13.6/i18n/es-ES.json'
            },
            dom: '<"row"<"col-sm-12 col-md-6"l><"col-sm-12 col-md-6"f>>' +
                 '<"row"<"col-sm-12"tr>>' +
                 '<"row"<"col-sm-12 col-md-5"i><"col-sm-12 col-md-7"p>>',
            drawCallback: function(settings) {
                // Reinicializar tooltips después de cada redibujado
                $('[title]').tooltip();
            }
        });

        return this.table;
    },

    // Recargar datos
    recargar: function() {
        if (this.table) {
            this.table.ajax.reload(null, false); // false = mantener paginación
        }
    },

    // Aplicar filtros
    aplicarFiltros: function(filtros) {
        if (!this.table) return;

        // Filtro de búsqueda general
        if (filtros.busqueda !== undefined) {
            this.table.search(filtros.busqueda).draw();
        }

        // Filtro de estado
        if (filtros.estado !== undefined) {
            if (filtros.estado === '') {
                // Mostrar todos
                this.table.column(3).search('').draw();
            } else {
                // Filtrar por estado específico
                const estadoTexto = filtros.estado === 'true' ? 'Activo' : 'Inactivo';
                this.table.column(3).search(estadoTexto).draw();
            }
        }
    },

    // Limpiar filtros
    limpiarFiltros: function() {
        if (this.table) {
            this.table.search('').columns().search('').draw();
        }
    },

    // Obtener instancia de la tabla
    obtenerTabla: function() {
        return this.table;
    },

    // Destruir tabla
    destruir: function() {
        if (this.table) {
            this.table.destroy();
            this.table = null;
        }
    }
};
