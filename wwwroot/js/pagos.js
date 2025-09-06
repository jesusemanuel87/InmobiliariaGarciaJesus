// Pagos JavaScript functionality

// Función para manejar el click del botón registrar pago
function handleRegistrarPagoClick(button) {
    const pagoId = button.getAttribute('data-pago-id');
    console.log('Clicked registrar pago for ID:', pagoId);
    registrarPago(pagoId);
}

// Inicializar cuando el DOM esté listo
document.addEventListener('DOMContentLoaded', function() {
    // Agregar event listeners a todos los botones
    const buttons = document.querySelectorAll('.registrar-pago-btn');
    buttons.forEach(function(button) {
        button.addEventListener('click', function(e) {
            e.preventDefault();
            e.stopPropagation();
            handleRegistrarPagoClick(this);
        });
    });
    
    console.log('Initialized', buttons.length, 'registrar pago buttons');
});

// Función para cargar y mostrar el modal de registro de pago
async function registrarPago(pagoId) {
    try {
        // Cargar el modal del servidor
        const response = await fetch(`/Pagos/RegistrarPago/${pagoId}`, {
            method: 'GET',
            headers: {
                'Accept': 'text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8'
            }
        });

        if (response.ok) {
            const modalHtml = await response.text();
            
            // Si la respuesta es JSON (error), mostrar el mensaje
            try {
                const jsonResult = JSON.parse(modalHtml);
                if (!jsonResult.success) {
                    showToast(`Error: ${jsonResult.message}`, 'error');
                    return;
                }
            } catch (e) {
                // No es JSON, continuar con el HTML del modal
            }
            
            // Insertar el modal en el contenedor
            document.getElementById('modalContainer').innerHTML = modalHtml;
            
            // Mostrar el modal
            const modal = new bootstrap.Modal(document.getElementById('registrarPagoModal'));
            modal.show();
            
            // Inicializar el formulario del modal
            initializeRegistrarPagoForm();
            
            // Limpiar el contenedor cuando se cierre el modal
            document.getElementById('registrarPagoModal').addEventListener('hidden.bs.modal', function () {
                document.getElementById('modalContainer').innerHTML = '';
            });
            
        } else {
            showToast('Error al cargar el formulario de registro de pago.', 'error');
        }
    } catch (error) {
        console.error('Error al cargar el modal:', error);
        showToast('Error al procesar la solicitud. Por favor, intente nuevamente.', 'error');
    }
}

// Función para mostrar notificaciones toast
function showToast(message, type = 'success') {
    const toast = document.getElementById('successToast');
    const toastHeader = toast.querySelector('.toast-header');
    const toastMessage = document.getElementById('toastMessage');
    
    // Configurar el tipo de toast
    if (type === 'error') {
        toastHeader.className = 'toast-header bg-danger text-white';
        toastHeader.querySelector('i').className = 'fas fa-exclamation-circle me-2';
        toastHeader.querySelector('strong').textContent = 'Error';
    } else {
        toastHeader.className = 'toast-header bg-success text-white';
        toastHeader.querySelector('i').className = 'fas fa-check-circle me-2';
        toastHeader.querySelector('strong').textContent = 'Éxito';
    }
    
    toastMessage.textContent = message;
    
    const bsToast = new bootstrap.Toast(toast);
    bsToast.show();
}

// Función para inicializar el formulario del modal (se llama cuando se carga el modal)
function initializeRegistrarPagoForm() {
    const form = document.getElementById('registrarPagoForm');
    if (!form) return;

    form.addEventListener('submit', function(e) {
        e.preventDefault();
        
        // Validar que se haya seleccionado un método de pago
        const metodoPago = document.getElementById('MetodoPago').value;
        if (!metodoPago) {
            alert('Debe seleccionar un método de pago');
            return;
        }
        
        const formData = new FormData(this);
        const submitButton = this.querySelector('button[type="submit"]');
        
        // Deshabilitar el botón para evitar doble envío
        submitButton.disabled = true;
        submitButton.innerHTML = '<i class="fas fa-spinner fa-spin me-1"></i>Procesando...';
        
        fetch(this.action, {
            method: 'POST',
            body: formData
        })
        .then(response => response.json())
        .then(result => {
            if (result.success) {
                // Cerrar el modal
                const modal = bootstrap.Modal.getInstance(document.getElementById('registrarPagoModal'));
                modal.hide();
                
                // Mostrar toast de éxito
                let mensaje = `Pago registrado exitosamente!`;
                if (result.intereses > 0 || result.multas > 0) {
                    mensaje += ` - Total pagado: $${result.totalAPagar.toLocaleString()}`;
                }
                
                showToast(mensaje, 'success');
                
                // Recargar la página después de un breve delay
                setTimeout(() => {
                    location.reload();
                }, 1500);
            } else {
                alert(`Error: ${result.message}`);
                submitButton.disabled = false;
                submitButton.innerHTML = '<i class="fas fa-dollar-sign me-1"></i>Registrar Pago';
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('Error al procesar la solicitud. Por favor, intente nuevamente.');
            submitButton.disabled = false;
            submitButton.innerHTML = '<i class="fas fa-dollar-sign me-1"></i>Registrar Pago';
        });
    });
}
