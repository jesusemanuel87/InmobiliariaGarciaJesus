// Pagos Modal Handler
class PagosModalHandler {
    static showModal(modalHtml, modalId) {
        // Insert modal HTML into container
        const modalContainer = document.getElementById('modalContainer');
        if (!modalContainer) {
            console.error('Modal container not found');
            return;
        }
        
        modalContainer.innerHTML = modalHtml;
        
        // Wait for DOM to be ready
        setTimeout(() => {
            const modalElement = document.getElementById(modalId);
            if (!modalElement) {
                console.error(`Modal element ${modalId} not found`);
                return;
            }
            
            // Show the modal
            const modal = new bootstrap.Modal(modalElement, {
                backdrop: 'static',
                keyboard: false
            });
            modal.show();
            
            // Clean up when modal is hidden
            modalElement.addEventListener('hidden.bs.modal', function () {
                modalContainer.innerHTML = '';
            }, { once: true });
        }, 100);
    }

    static initializeRegistrarPagoForm() {
        const form = document.getElementById('registrarPagoForm');
        if (!form) return;

        form.addEventListener('submit', function(e) {
            e.preventDefault();
            
            // Validate payment method selection
            const metodoPago = document.getElementById('MetodoPago').value;
            if (!metodoPago) {
                PagosUtilities.showToast('Debe seleccionar un método de pago', 'error');
                return;
            }
            
            const formData = new FormData(this);
            const submitButton = this.querySelector('button[type="submit"]');
            
            // Disable button to prevent double submission
            submitButton.disabled = true;
            submitButton.innerHTML = '<i class="fas fa-spinner fa-spin me-1"></i>Procesando...';
            
            fetch(this.action, {
                method: 'POST',
                body: formData
            })
            .then(response => response.json())
            .then(result => {
                if (result.success) {
                    // Close modal
                    const modal = bootstrap.Modal.getInstance(document.getElementById('registrarPagoModal'));
                    modal.hide();
                    
                    // Show success message
                    let mensaje = `Pago registrado exitosamente!`;
                    if (result.intereses > 0 || result.multas > 0) {
                        mensaje += ` - Total pagado: $${result.totalAPagar.toLocaleString()}`;
                    }
                    
                    PagosUtilities.showToast(mensaje, 'success');
                    
                    // Refresh table
                    setTimeout(() => {
                        pagosManager.refreshTable();
                    }, 1000);
                } else {
                    PagosUtilities.showToast(`Error: ${result.message}`, 'error');
                    submitButton.disabled = false;
                    submitButton.innerHTML = '<i class="fas fa-dollar-sign me-1"></i>Registrar Pago';
                }
            })
            .catch(error => {
                console.error('Error:', error);
                PagosUtilities.showToast('Error al procesar la solicitud. Por favor, intente nuevamente.', 'error');
                submitButton.disabled = false;
                submitButton.innerHTML = '<i class="fas fa-dollar-sign me-1"></i>Registrar Pago';
            });
        });
    }

    static initializeFormModal() {
        const form = document.getElementById('pagoForm');
        if (!form) return;

        form.addEventListener('submit', function(e) {
            e.preventDefault();
            
            const formData = new FormData(this);
            const submitButton = this.querySelector('button[type="submit"]');
            
            // Convert FormData to JSON
            const jsonData = {};
            for (let [key, value] of formData.entries()) {
                jsonData[key] = value;
            }
            
            submitButton.disabled = true;
            submitButton.innerHTML = '<i class="fas fa-spinner fa-spin me-1"></i>Guardando...';
            
            fetch('/Pagos/Save', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(jsonData)
            })
            .then(response => response.json())
            .then(result => {
                if (result.success) {
                    const modal = bootstrap.Modal.getInstance(document.getElementById('pagoFormModal'));
                    modal.hide();
                    PagosUtilities.showToast(result.message, 'success');
                    pagosManager.refreshTable();
                } else {
                    if (result.errors) {
                        PagosUtilities.displayValidationErrors(result.errors);
                    } else {
                        PagosUtilities.showToast(result.message, 'error');
                    }
                }
            })
            .catch(error => {
                console.error('Error:', error);
                PagosUtilities.showToast('Error al procesar la solicitud.', 'error');
            })
            .finally(() => {
                submitButton.disabled = false;
                submitButton.innerHTML = '<i class="fas fa-save me-1"></i>Guardar';
            });
        });
    }

    static initializeDeleteModal() {
        const form = document.getElementById('deleteForm');
        if (!form) return;

        form.addEventListener('submit', function(e) {
            e.preventDefault();
            
            const formData = new FormData(this);
            const submitButton = this.querySelector('button[type="submit"]');
            
            // Convert FormData to JSON
            const jsonData = {};
            for (let [key, value] of formData.entries()) {
                jsonData[key] = value;
            }
            
            submitButton.disabled = true;
            submitButton.innerHTML = '<i class="fas fa-spinner fa-spin me-1"></i>Eliminando...';
            
            fetch('/Pagos/DeleteConfirmed', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                },
                body: new URLSearchParams(formData)
            })
            .then(response => response.json())
            .then(result => {
                if (result.success) {
                    const modal = bootstrap.Modal.getInstance(document.getElementById('pagoDeleteModal'));
                    modal.hide();
                    PagosUtilities.showToast(result.message, 'success');
                    pagosManager.refreshTable();
                } else {
                    PagosUtilities.showToast(result.message, 'error');
                }
            })
            .catch(error => {
                console.error('Error:', error);
                PagosUtilities.showToast('Error al procesar la solicitud.', 'error');
            })
            .finally(() => {
                submitButton.disabled = false;
                submitButton.innerHTML = '<i class="fas fa-trash me-1"></i>Eliminar';
            });
        });
    }
}
