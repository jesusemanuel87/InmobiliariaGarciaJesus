document.addEventListener('DOMContentLoaded', function() {
    const estadoSelect = document.getElementById('estadoSelect');
    const motivoCancelacionDiv = document.getElementById('motivoCancelacionDiv');
    const fechaInicioInput = document.querySelector('input[name="FechaInicio"]');
    
    // Mostrar/ocultar campo de motivo según el estado
    function toggleMotivoCancelacion() {
        if (estadoSelect.value === '3') { // EstadoContrato.Cancelado = 3
            motivoCancelacionDiv.style.display = 'block';
            document.querySelector('textarea[name="MotivoCancelacion"]').required = true;
        } else {
            motivoCancelacionDiv.style.display = 'none';
            document.querySelector('textarea[name="MotivoCancelacion"]').required = false;
        }
    }
    
    // Verificar estado inicial al cargar la página
    toggleMotivoCancelacion();
    
    // Escuchar cambios en el estado
    estadoSelect.addEventListener('change', toggleMotivoCancelacion);
    
    // Deshabilitar fecha de inicio si el contrato está activo
    if (estadoSelect.value === '1') { // EstadoContrato.Activo = 1
        fechaInicioInput.readOnly = true;
        fechaInicioInput.style.backgroundColor = '#f8f9fa';
    }
    
    // Deshabilitar validaciones automáticas en modo edición
    // Solo validar cuando el usuario cambie campos manualmente
    const isEditMode = document.querySelector('input[name="Id"]') !== null;
    if (isEditMode) {
        // Limpiar mensajes de validación al cargar
        const validationMessages = document.querySelectorAll('.text-danger, .alert-danger');
        validationMessages.forEach(msg => {
            if (msg.textContent.includes('superponen') || msg.textContent.includes('anterior')) {
                msg.style.display = 'none';
            }
        });
    }
});
