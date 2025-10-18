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
        <div class="text-center py-4">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Cargando...</span>
            </div>
            <p class="mt-2 text-muted">Cargando propietarios...</p>
        </div>
    `;
    modal.show();

    fetch('/Usuarios/GetPropietariosSinUsuario')
        .then(response => response.json())
        .then(result => {
            if (result.success && result.data.length > 0) {
                let html = `
                    <div class="alert alert-info">
                        <i class="fas fa-info-circle"></i> 
                        <strong>Propietarios activos sin usuario</strong><br>
                        Haga clic en "Crear Usuario" para generar credenciales de acceso.
                    </div>
                    <div class="table-responsive">
                        <table class="table table-hover table-sm">
                            <thead>
                                <tr>
                                    <th>DNI</th>
                                    <th>Nombre Completo</th>
                                    <th>Email</th>
                                    <th class="text-center">Acción</th>
                                </tr>
                            </thead>
                            <tbody>
                `;
                
                result.data.forEach(prop => {
                    html += `
                        <tr>
                            <td>${prop.dni}</td>
                            <td>${prop.nombre} ${prop.apellido}</td>
                            <td><small>${prop.email}</small></td>
                            <td class="text-center">
                                <button class="btn btn-sm btn-success" onclick="crearUsuarioParaPersona(${prop.id}, 'Propietario', '${prop.nombre} ${prop.apellido}')">
                                    <i class="fas fa-user-plus"></i> Crear Usuario
                                </button>
                            </td>
                        </tr>
                    `;
                });
                
                html += `
                            </tbody>
                        </table>
                    </div>
                    <div class="alert alert-warning mt-3 mb-0">
                        <small>
                            <i class="fas fa-key"></i> <strong>Nota:</strong> El username será <code>nombre.apellido</code> 
                            y la contraseña temporal será el <strong>DNI</strong>. El usuario deberá cambiar su contraseña en el primer inicio de sesión.
                        </small>
                    </div>
                `;
                
                document.getElementById('personasSinUsuarioModalBody').innerHTML = html;
            } else if (result.success) {
                document.getElementById('personasSinUsuarioModalBody').innerHTML = `
                    <div class="alert alert-success">
                        <i class="fas fa-check-circle"></i> 
                        <strong>¡Excelente!</strong><br>
                        Todos los propietarios activos ya tienen usuarios asignados.
                    </div>
                `;
            } else {
                document.getElementById('personasSinUsuarioModalBody').innerHTML = `
                    <div class="alert alert-danger">
                        <i class="fas fa-exclamation-triangle"></i> ${result.message}
                    </div>
                `;
            }
        })
        .catch(error => {
            console.error('Error:', error);
            document.getElementById('personasSinUsuarioModalBody').innerHTML = `
                <div class="alert alert-danger">
                    <i class="fas fa-exclamation-triangle"></i> Error al cargar propietarios sin usuario
                </div>
            `;
        });
}

function verInquilinosSinUsuario() {
    const modal = new bootstrap.Modal(document.getElementById('personasSinUsuarioModal'));
    document.getElementById('personasSinUsuarioModalLabel').innerHTML = '<i class="fas fa-user"></i> Inquilinos sin Usuario';
    document.getElementById('personasSinUsuarioModalBody').innerHTML = `
        <div class="text-center py-4">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Cargando...</span>
            </div>
            <p class="mt-2 text-muted">Cargando inquilinos...</p>
        </div>
    `;
    modal.show();

    fetch('/Usuarios/GetInquilinosSinUsuario')
        .then(response => response.json())
        .then(result => {
            if (result.success && result.data.length > 0) {
                let html = `
                    <div class="alert alert-info">
                        <i class="fas fa-info-circle"></i> 
                        <strong>Inquilinos activos sin usuario</strong><br>
                        Haga clic en "Crear Usuario" para generar credenciales de acceso.
                    </div>
                    <div class="table-responsive">
                        <table class="table table-hover table-sm">
                            <thead>
                                <tr>
                                    <th>DNI</th>
                                    <th>Nombre Completo</th>
                                    <th>Email</th>
                                    <th class="text-center">Acción</th>
                                </tr>
                            </thead>
                            <tbody>
                `;
                
                result.data.forEach(inq => {
                    html += `
                        <tr>
                            <td>${inq.dni}</td>
                            <td>${inq.nombre} ${inq.apellido}</td>
                            <td><small>${inq.email}</small></td>
                            <td class="text-center">
                                <button class="btn btn-sm btn-info" onclick="crearUsuarioParaPersona(${inq.id}, 'Inquilino', '${inq.nombre} ${inq.apellido}')">
                                    <i class="fas fa-user-plus"></i> Crear Usuario
                                </button>
                            </td>
                        </tr>
                    `;
                });
                
                html += `
                            </tbody>
                        </table>
                    </div>
                    <div class="alert alert-warning mt-3 mb-0">
                        <small>
                            <i class="fas fa-key"></i> <strong>Nota:</strong> El username será <code>nombre.apellido</code> 
                            y la contraseña temporal será el <strong>DNI</strong>. El usuario deberá cambiar su contraseña en el primer inicio de sesión.
                        </small>
                    </div>
                `;
                
                document.getElementById('personasSinUsuarioModalBody').innerHTML = html;
            } else if (result.success) {
                document.getElementById('personasSinUsuarioModalBody').innerHTML = `
                    <div class="alert alert-success">
                        <i class="fas fa-check-circle"></i> 
                        <strong>¡Excelente!</strong><br>
                        Todos los inquilinos activos ya tienen usuarios asignados.
                    </div>
                `;
            } else {
                document.getElementById('personasSinUsuarioModalBody').innerHTML = `
                    <div class="alert alert-danger">
                        <i class="fas fa-exclamation-triangle"></i> ${result.message}
                    </div>
                `;
            }
        })
        .catch(error => {
            console.error('Error:', error);
            document.getElementById('personasSinUsuarioModalBody').innerHTML = `
                <div class="alert alert-danger">
                    <i class="fas fa-exclamation-triangle"></i> Error al cargar inquilinos sin usuario
                </div>
            `;
        });
}

function crearUsuarioParaPersona(personaId, tipo, nombreCompleto) {
    Swal.fire({
        title: `¿Crear Usuario para ${tipo}?`,
        html: `<p>Se creará un usuario para <strong>${nombreCompleto}</strong></p>
               <p class="text-muted mb-0"><small><i class="fas fa-key"></i> Username: <code>nombre.apellido</code><br>
               <i class="fas fa-lock"></i> Contraseña temporal: <strong>DNI</strong></small></p>`,
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#0d6efd',
        cancelButtonColor: '#6c757d',
        confirmButtonText: 'Sí, crear',
        cancelButtonText: 'Cancelar',
        reverseButtons: true
    }).then((result) => {
        if (result.isConfirmed) {
            Swal.fire({
                title: 'Creando usuario...',
                text: 'Por favor espere',
                allowOutsideClick: false,
                didOpen: () => {
                    Swal.showLoading();
                }
            });

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
                    Swal.fire({
                        title: '¡Usuario Creado!',
                        html: `<p>${data.message}</p>
                               <p class="text-warning mt-3"><i class="fas fa-exclamation-triangle"></i> El usuario está <strong>inactivo</strong>. 
                               Debe activarlo desde la lista de usuarios.</p>`,
                        icon: 'success',
                        confirmButtonColor: '#198754',
                        confirmButtonText: 'Entendido'
                    }).then(() => location.reload());
                } else {
                    Swal.fire('Error', data.message, 'error');
                }
            })
            .catch(error => {
                console.error('Error:', error);
                Swal.fire('Error', 'Error al crear el usuario', 'error');
            });
        }
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
