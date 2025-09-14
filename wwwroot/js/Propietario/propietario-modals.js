// PropietarioModals - Modal handling functions for Propietarios
class PropietarioModals {
    
    // Global functions for modal interactions
    static showDetailsModal(id) {
        if (window.propietarioManager) {
            window.propietarioManager.showDetailsModal(id);
        }
    }

    static showEditModal(id) {
        if (window.propietarioManager) {
            window.propietarioManager.showEditModal(id);
        }
    }

    static showEditFromDetails(id) {
        console.log('showEditFromDetails called with id:', id);
        
        if (!window.propietarioManager) {
            console.error('propietarioManager not found');
            return;
        }
        
        // Cerrar el modal de detalles inmediatamente
        $('#detailsPropietarioModal').modal('hide');
        
        // Usar setTimeout para dar tiempo a que se cierre el modal
        setTimeout(function() {
            console.log('Opening edit modal after timeout');
            window.propietarioManager.showEditModal(id);
        }, 300);
    }

    static showDeleteModal(id) {
        if (window.propietarioManager) {
            window.propietarioManager.showDeleteModal(id);
        }
    }

    static submitForm() {
        console.log('Global submitForm called, propietarioManager:', window.propietarioManager);
        if (window.propietarioManager) {
            window.propietarioManager.submitForm();
        } else {
            console.error('propietarioManager is not initialized');
        }
    }

    static submitDeleteForm() {
        if (window.propietarioManager) {
            window.propietarioManager.submitDeleteForm();
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
window.showDetailsModal = PropietarioModals.showDetailsModal;
window.showEditModal = PropietarioModals.showEditModal;
window.showEditFromDetails = PropietarioModals.showEditFromDetails;
window.showDeleteModal = PropietarioModals.showDeleteModal;
window.submitForm = PropietarioModals.submitForm;
window.submitDeleteForm = PropietarioModals.submitDeleteForm;
window.cleanupModals = PropietarioModals.cleanupModals;
