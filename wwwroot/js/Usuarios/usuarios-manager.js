// Gestión de Usuarios - Manager

function activarUsuario(id) {
    Swal.fire({
        title: '¿Activar Usuario?',
        text: 'El usuario podrá iniciar sesión en el sistema.',
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#198754',
        cancelButtonColor: '#6c757d',
        confirmButtonText: 'Sí, activar',
        cancelButtonText: 'Cancelar',
        reverseButtons: true
    }).then((result) => {
        if (result.isConfirmed) {
            fetch(`/Usuarios/ActivarUsuario`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                },
                body: JSON.stringify({ id: id })
            })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    Swal.fire('¡Activado!', data.message, 'success')
                        .then(() => location.reload());
                } else {
                    Swal.fire('Error', data.message, 'error');
                }
            })
            .catch(error => {
                console.error('Error:', error);
                Swal.fire('Error', 'Error al activar el usuario', 'error');
            });
        }
    });
}

function desactivarUsuario(id) {
    Swal.fire({
        title: '¿Desactivar Usuario?',
        text: 'El usuario no podrá iniciar sesión en el sistema.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#6c757d',
        confirmButtonText: 'Sí, desactivar',
        cancelButtonText: 'Cancelar',
        reverseButtons: true
    }).then((result) => {
        if (result.isConfirmed) {
            fetch(`/Usuarios/DesactivarUsuario`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                },
                body: JSON.stringify({ id: id })
            })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    Swal.fire('¡Desactivado!', data.message, 'success')
                        .then(() => location.reload());
                } else {
                    Swal.fire('Error', data.message, 'error');
                }
            })
            .catch(error => {
                console.error('Error:', error);
                Swal.fire('Error', 'Error al desactivar el usuario', 'error');
            });
        }
    });
}

function restablecerContrasena(id) {
    Swal.fire({
        title: '¿Restablecer Contraseña?',
        html: '<p>La nueva contraseña será el <strong>DNI</strong> del usuario.</p><p class="text-warning mb-0"><i class="fas fa-exclamation-triangle"></i> El usuario deberá cambiar su contraseña en el próximo inicio de sesión.</p>',
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#0d6efd',
        cancelButtonColor: '#6c757d',
        confirmButtonText: 'Sí, restablecer',
        cancelButtonText: 'Cancelar',
        reverseButtons: true
    }).then((result) => {
        if (result.isConfirmed) {
            fetch(`/Usuarios/RestablecerContrasena`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                },
                body: JSON.stringify({ id: id })
            })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    Swal.fire('¡Restablecida!', data.message, 'success');
                } else {
                    Swal.fire('Error', data.message, 'error');
                }
            })
            .catch(error => {
                console.error('Error:', error);
                Swal.fire('Error', 'Error al restablecer la contraseña', 'error');
            });
        }
    });
}

function verPropietariosSinUsuario() {
    const modal = new bootstrap.Modal(document.getElementById('personasSinUsuarioModal'));
    document.getElementById('personasSinUsuarioModalLabel').innerHTML = '<i class="fas fa-home"></i> Propietarios sin Usuario';
    document.getElementById('personasSinUsuarioModalBody').innerHTML = `
        <div class="text-center">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Cargando...</span>
            </div>
        </div>
    `;
    modal.show();

    fetch('/Propietarios/Index')
        .then(response => response.text())
        .then(html => {
            // Aquí deberías hacer una petición a un endpoint que devuelva propietarios sin usuario
            // Por ahora mostramos un mensaje
            document.getElementById('personasSinUsuarioModalBody').innerHTML = `
                <div class="alert alert-info">
                    <i class="fas fa-info-circle"></i> 
                    <strong>Funcionalidad en desarrollo</strong><br>
                    Para crear usuarios para propietarios existentes:<br><br>
                    <ol>
                        <li>Vaya al módulo <strong>Propietarios</strong></li>
                        <li>Identifique propietarios sin usuario (aparecen sin badge de usuario)</li>
                        <li>Use el botón "Crear Usuario" en la sección de detalles</li>
                    </ol>
                    <p class="mb-0 mt-3">
                        <strong>Nota:</strong> El username será generado automáticamente como <code>nombre.apellido</code> 
                        y la contraseña temporal será el <strong>DNI</strong> del propietario.
                    </p>
                </div>
            `;
        })
        .catch(error => {
            console.error('Error:', error);
            document.getElementById('personasSinUsuarioModalBody').innerHTML = `
                <div class="alert alert-danger">
                    <i class="fas fa-exclamation-triangle"></i> Error al cargar propietarios
                </div>
            `;
        });
}

function verInquilinosSinUsuario() {
    const modal = new bootstrap.Modal(document.getElementById('personasSinUsuarioModal'));
    document.getElementById('personasSinUsuarioModalLabel').innerHTML = '<i class="fas fa-user"></i> Inquilinos sin Usuario';
    document.getElementById('personasSinUsuarioModalBody').innerHTML = `
        <div class="text-center">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Cargando...</span>
            </div>
        </div>
    `;
    modal.show();

    // Similar al anterior
    setTimeout(() => {
        document.getElementById('personasSinUsuarioModalBody').innerHTML = `
            <div class="alert alert-info">
                <i class="fas fa-info-circle"></i> 
                <strong>Funcionalidad en desarrollo</strong><br>
                Para crear usuarios para inquilinos existentes:<br><br>
                <ol>
                    <li>Vaya al módulo <strong>Inquilinos</strong></li>
                    <li>Identifique inquilinos sin usuario (aparecen sin badge de usuario)</li>
                    <li>Use el botón "Crear Usuario" en la sección de detalles</li>
                </ol>
                <p class="mb-0 mt-3">
                    <strong>Nota:</strong> El username será generado automáticamente como <code>nombre.apellido</code> 
                    y la contraseña temporal será el <strong>DNI</strong> del inquilino.
                </p>
            </div>
        `;
    }, 500);
}

function crearUsuarioParaPersona(personaId, tipo) {
    if (!confirm(`¿Está seguro que desea crear un usuario para este ${tipo}?\n\nSe generará un username automático y la contraseña será el DNI.`)) {
        return;
    }

    fetch(`/Usuarios/CrearUsuarioParaPersona`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
        },
        body: JSON.stringify({ 
            personaId: personaId,
            tipo: tipo
        })
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            showAlert('success', data.message);
            setTimeout(() => location.reload(), 2000);
        } else {
            showAlert('danger', data.message);
        }
    })
    .catch(error => {
        console.error('Error:', error);
        showAlert('danger', 'Error al crear el usuario');
    });
}

function showAlert(type, message) {
    const alertHtml = `
        <div class="alert alert-${type} alert-dismissible fade show" role="alert">
            <i class="fas fa-${type === 'success' ? 'check-circle' : type === 'warning' ? 'exclamation-triangle' : 'times-circle'}"></i> 
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;
    
    const container = document.querySelector('.container-fluid .col-12');
    const existingAlert = container.querySelector('.alert');
    
    if (existingAlert) {
        existingAlert.remove();
    }
    
    const h2 = container.querySelector('h2');
    h2.insertAdjacentHTML('afterend', alertHtml);
    
    setTimeout(() => {
        const alert = container.querySelector('.alert');
        if (alert) {
            alert.classList.remove('show');
            setTimeout(() => alert.remove(), 150);
        }
    }, 5000);
}
