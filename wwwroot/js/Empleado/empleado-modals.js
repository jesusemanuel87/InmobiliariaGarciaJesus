// EmpleadoModals - Modal management functions for Empleados module

// Show create modal
function showCreateModal() {
    $.get('/Empleados/FormModal')
        .done(function(data) {
            $('#modalContainer').html(data);
            const modal = new bootstrap.Modal(document.getElementById('empleadoFormModal'));
            modal.show();
        })
        .fail(function() {
            EmpleadoUtils.showAlert('Error al cargar el formulario', 'error');
        });
}

// Show edit modal
function showEditModal(id) {
    $.get(`/Empleados/FormModal/${id}`)
        .done(function(data) {
            $('#modalContainer').html(data);
            const modal = new bootstrap.Modal(document.getElementById('empleadoFormModal'));
            modal.show();
        })
        .fail(function() {
            EmpleadoUtils.showAlert('Error al cargar el formulario de edición', 'error');
        });
}

// Show details modal
function showDetailsModal(id) {
    $.get(`/Empleados/DetailsModal/${id}`)
        .done(function(data) {
            $('#modalContainer').html(data);
            const modal = new bootstrap.Modal(document.getElementById('empleadoDetailsModal'));
            modal.show();
        })
        .fail(function() {
            EmpleadoUtils.showAlert('Error al cargar los detalles', 'error');
        });
}

// Show delete modal
function showDeleteModal(id) {
    $.get(`/Empleados/DeleteModal/${id}`)
        .done(function(data) {
            $('#modalContainer').html(data);
            const modal = new bootstrap.Modal(document.getElementById('empleadoDeleteModal'));
            modal.show();
        })
        .fail(function() {
            EmpleadoUtils.showAlert('Error al cargar la confirmación de eliminación', 'error');
        });
}

// Save empleado (create or update)
function saveEmpleado() {
    console.log('saveEmpleado() called');
    const formData = EmpleadoUtils.getFormData('#empleadoForm');
    console.log('Form data:', formData);
    
    // Client-side validation
    const validation = EmpleadoUtils.validateEmpleadoForm(formData);
    console.log('Validation result:', validation);
    if (!validation.isValid) {
        EmpleadoUtils.displayValidationErrors(validation.errors);
        return;
    }

    // Clear previous validation errors
    EmpleadoUtils.clearValidationErrors();

    // Show loading state
    const submitBtn = $('#empleadoFormModal .btn-primary');
    const originalText = submitBtn.html();
    submitBtn.html('<i class="fas fa-spinner fa-spin"></i> Guardando...').prop('disabled', true);

    // Prepare data in the format expected by the controller
    const requestData = {
        Empleado: {
            Id: parseInt(formData.Id) || 0,
            Dni: formData.Dni || '',
            Nombre: formData.Nombre || '',
            Apellido: formData.Apellido || '',
            Email: formData.Email || '',
            Telefono: formData.Telefono || '',
            Rol: parseInt(formData.Rol === 'Empleado' ? 1 : 2), // Convert to enum value (Empleado=1, Administrador=2)
            Estado: formData.Estado === 'true' || formData.Estado === true || formData.Estado === 'on',
            FechaCreacion: formData.FechaCreacion || new Date().toISOString(),
            FechaIngreso: formData.FechaCreacion || new Date().toISOString()
        },
        NombreUsuario: formData.NombreUsuario || null,
        Password: formData.Password || null
    };
    
    console.log('Request data:', requestData);

    $.ajax({
        url: '/Empleados/Save',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(requestData),
        success: function(response) {
            console.log('AJAX Success - Response:', response);
            if (response.success) {
                // Close modal
                const modal = bootstrap.Modal.getInstance(document.getElementById('empleadoFormModal'));
                modal.hide();
                
                // Show success message
                EmpleadoUtils.showAlert(response.message, 'success');
                
                // Reload DataTable
                if (window.empleadoManager && window.empleadoManager.dataTable) {
                    window.empleadoManager.dataTable.ajax.reload();
                }
            } else {
                // Show error message
                console.log('Save failed:', response.message);
                EmpleadoUtils.showAlert(response.message || 'Error al guardar el empleado', 'error');
            }
        },
        error: function(xhr, status, error) {
            console.error('AJAX Error:', {xhr, status, error});
            console.error('Response Text:', xhr.responseText);
            EmpleadoUtils.showAlert('Error de conexión al guardar el empleado', 'error');
        },
        complete: function() {
            console.log('AJAX Complete');
            // Restore button state
            submitBtn.html(originalText).prop('disabled', false);
        }
    });
}

// Delete empleado
function deleteEmpleado(id) {
    // Check confirmation checkbox
    const confirmCheckbox = $('#confirmDelete');
    if (!confirmCheckbox.is(':checked')) {
        EmpleadoUtils.showAlert('Debe confirmar la eliminación marcando la casilla', 'error', '#deleteModalAlerts');
        return;
    }

    // Show loading state
    const deleteBtn = $('#confirmDeleteBtn');
    const originalText = deleteBtn.html();
    deleteBtn.html('<i class="fas fa-spinner fa-spin"></i> Eliminando...').prop('disabled', true);

    $.ajax({
        url: '/Empleados/Delete',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ id: id }),
        success: function(response) {
            if (response.success) {
                // Close modal
                const modal = bootstrap.Modal.getInstance(document.getElementById('empleadoDeleteModal'));
                modal.hide();
                
                // Show success message
                EmpleadoUtils.showAlert(response.message, 'success');
                
                // Reload DataTable
                if (window.empleadoManager && window.empleadoManager.dataTable) {
                    window.empleadoManager.dataTable.ajax.reload();
                }
            } else {
                EmpleadoUtils.showAlert(response.message || 'Error al eliminar el empleado', 'error', '#deleteModalAlerts');
            }
        },
        error: function() {
            EmpleadoUtils.showAlert('Error de conexión al eliminar el empleado', 'error', '#deleteModalAlerts');
        },
        complete: function() {
            // Restore button state
            deleteBtn.html(originalText).prop('disabled', false);
        }
    });
}
