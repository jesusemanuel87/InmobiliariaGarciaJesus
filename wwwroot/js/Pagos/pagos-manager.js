// Pagos Index Manager
class PagosIndexManager {
    constructor() {
        this.dataTable = null;
        this.init();
    }

    init() {
        $(document).ready(() => {
            this.initializeDataTable();
            this.bindEvents();
            console.log('PagosIndexManager initialized');
        });
    }

    initializeDataTable() {
        console.log('Initializing DataTable...');
        
        // Check if table is already initialized and destroy it
        if ($.fn.DataTable.isDataTable('#pagosTable')) {
            console.log('DataTable already exists, destroying...');
            $('#pagosTable').DataTable().destroy();
        }

        // Clear any existing filters before reinitializing
        if (this.filters) {
            this.filters.reinitialize();
        }

        // Detectar si es vista de inquilino (MisPagos)
        const isInquilinoView = window.esMisPagos === true;
        
        this.table = $('#pagosTable').DataTable({
            processing: true,
            serverSide: true,
            responsive: true,
            pageLength: 10,
            lengthMenu: [[10, 25, 50, 100], [10, 25, 50, 100]],
            columns: PagosDataTablesConfig.getColumns(),
            ajax: PagosDataTablesConfig.getAjaxConfig(isInquilinoView),
            order: PagosDataTablesConfig.getDefaultOrder(),
            language: DataTablesConfig.getSpanishLanguage(),
            dom: '<"row"<"col-sm-12 col-md-6"l><"col-sm-12 col-md-6"f>>' +
                 '<"row"<"col-sm-12"tr>>' +
                 '<"row"<"col-sm-12 col-md-5"i><"col-sm-12 col-md-7"p>>'
        });

        // Initialize filters after DataTable is ready
        if (!this.filters) {
            this.filters = new PagosFilters();
        }

        console.log('DataTable initialized successfully');
    }

    bindEvents() {
        // Auto-dismiss alerts
        setTimeout(() => {
            $('.alert').fadeOut();
        }, 5000);
    }

    // Modal methods
    async showDetailsModal(pagoId) {
        try {
            const response = await fetch(`/Pagos/DetailsModal/${pagoId}`);
            if (response.ok) {
                const modalHtml = await response.text();
                PagosModalHandler.showModal(modalHtml, 'pagoDetailsModal');
            } else {
                PagosUtilities.showToast('Error al cargar los detalles del pago.', 'error');
            }
        } catch (error) {
            console.error('Error loading details modal:', error);
            PagosUtilities.showToast('Error al procesar la solicitud.', 'error');
        }
    }

    async showRegistrarPagoModal(pagoId) {
        try {
            const response = await fetch(`/Pagos/RegistrarPago/${pagoId}`);
            if (response.ok) {
                const modalHtml = await response.text();
                
                // Check if response is JSON error
                try {
                    const jsonResult = JSON.parse(modalHtml);
                    if (!jsonResult.success) {
                        PagosUtilities.showToast(`Error: ${jsonResult.message}`, 'error');
                        return;
                    }
                } catch (e) {
                    // Not JSON, continue with HTML
                }
                
                PagosModalHandler.showModal(modalHtml, 'registrarPagoModal');
                PagosModalHandler.initializeRegistrarPagoForm();
            } else {
                PagosUtilities.showToast('Error al cargar el formulario de registro de pago.', 'error');
            }
        } catch (error) {
            console.error('Error loading registrar pago modal:', error);
            PagosUtilities.showToast('Error al procesar la solicitud.', 'error');
        }
    }

    async showEditModal(pagoId) {
        try {
            const response = await fetch(`/Pagos/FormModal/${pagoId}`);
            if (response.ok) {
                const modalHtml = await response.text();
                PagosModalHandler.showModal(modalHtml, 'pagoFormModal');
                PagosModalHandler.initializeFormModal();
            } else {
                PagosUtilities.showToast('Error al cargar el formulario de edición.', 'error');
            }
        } catch (error) {
            console.error('Error loading edit modal:', error);
            PagosUtilities.showToast('Error al procesar la solicitud.', 'error');
        }
    }

    async showDeleteModal(pagoId) {
        try {
            const response = await fetch(`/Pagos/DeleteModal/${pagoId}`);
            if (response.ok) {
                const modalHtml = await response.text();
                PagosModalHandler.showModal(modalHtml, 'pagoDeleteModal');
                PagosModalHandler.initializeDeleteModal();
            } else {
                PagosUtilities.showToast('Error al cargar el formulario de eliminación.', 'error');
            }
        } catch (error) {
            console.error('Error loading delete modal:', error);
            PagosUtilities.showToast('Error al procesar la solicitud.', 'error');
        }
    }

    refreshTable() {
        if (this.table) {
            console.log('Refreshing DataTable...');
            this.table.ajax.reload(null, false); // false = keep current page
        } else {
            console.error('DataTable not initialized');
        }
    }
}

// Make manager globally available
var pagosManager;
