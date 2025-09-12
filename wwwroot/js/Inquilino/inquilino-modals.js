// InquilinoModals - Modal handling functions for Inquilinos
class InquilinoModals {
    
    // Global functions for modal interactions
    static showDetailsModal(id) {
        if (window.inquilinoManager) {
            window.inquilinoManager.showDetailsModal(id);
        }
    }

    static showEditModal(id) {
        if (window.inquilinoManager) {
            window.inquilinoManager.showEditModal(id);
        }
    }

    static showEditFromDetails(id) {
        console.log('showEditFromDetails called with id:', id);
        
        if (!window.inquilinoManager) {
            console.error('inquilinoManager not found');
            return;
        }
        
        // Cerrar el modal de detalles inmediatamente
        $('#detailsInquilinoModal').modal('hide');
        
        // Usar setTimeout para dar tiempo a que se cierre el modal
        setTimeout(function() {
            console.log('Opening edit modal after timeout');
            window.inquilinoManager.showEditModal(id);
        }, 300);
    }

    static showDeleteModal(id) {
        if (window.inquilinoManager) {
            window.inquilinoManager.showDeleteModal(id);
        }
    }

    static submitForm() {
        console.log('Global submitForm called, inquilinoManager:', window.inquilinoManager);
        if (window.inquilinoManager) {
            window.inquilinoManager.submitForm();
        } else {
            console.error('inquilinoManager is not initialized');
        }
    }

    static submitDeleteForm() {
        if (window.inquilinoManager) {
            window.inquilinoManager.submitDeleteForm();
        }
    }

    static cleanupModals() {
        // Remover cualquier backdrop residual
        $('.modal-backdrop').remove();
        
        // Restaurar el scroll del body
        $('body').removeClass('modal-open').css('padding-right', '');
        
        // Limpiar cualquier modal que pueda estar abierto
        $('.modal').modal('hide');
        
        console.log('Modals cleaned up');
    }
}

// Export functions to global scope for onclick handlers
window.showDetailsModal = InquilinoModals.showDetailsModal;
window.showEditModal = InquilinoModals.showEditModal;
window.showEditFromDetails = InquilinoModals.showEditFromDetails;
window.showDeleteModal = InquilinoModals.showDeleteModal;
window.submitForm = InquilinoModals.submitForm;
window.submitDeleteForm = InquilinoModals.submitDeleteForm;
window.cleanupModals = InquilinoModals.cleanupModals;
