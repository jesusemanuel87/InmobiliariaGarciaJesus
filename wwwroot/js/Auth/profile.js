// Auth profile functionality
class AuthProfileManager {
    constructor() {
        this.init();
    }

    init() {
        // Inicializar inmediatamente si el DOM está listo, sino esperar
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', () => {
                this.bindPasswordConfirmation();
                console.log('AuthProfileManager initialized (DOM ready)');
            });
        } else {
            this.bindPasswordConfirmation();
            console.log('AuthProfileManager initialized (immediate)');
        }
    }

    bindPasswordConfirmation() {
        // Usar vanilla JavaScript si jQuery no está disponible
        const confirmPasswordInput = document.querySelector('input[name="ConfirmPassword"]');
        if (confirmPasswordInput) {
            confirmPasswordInput.addEventListener('input', (e) => {
                const newPasswordInput = document.querySelector('input[name="NewPassword"]');
                const newPassword = newPasswordInput ? newPasswordInput.value : '';
                const confirmPassword = e.target.value;
                
                if (newPassword !== confirmPassword) {
                    e.target.setCustomValidity('Las contraseñas no coinciden');
                } else {
                    e.target.setCustomValidity('');
                }
            });
        }
    }

    uploadProfilePhoto() {
        const fileInput = document.getElementById('profilePhotoInput');
        const file = fileInput.files[0];
        
        if (!file) return;

        // Validar tipo de archivo
        const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp'];
        if (!allowedTypes.includes(file.type)) {
            alert('Por favor seleccione un archivo de imagen válido (JPG, PNG, GIF, WEBP)');
            return;
        }

        // Validar tamaño (5MB máximo)
        if (file.size > 5 * 1024 * 1024) {
            alert('El archivo es demasiado grande. Máximo 5MB permitido.');
            return;
        }

        const formData = new FormData();
        formData.append('profilePhoto', file);

        // Mostrar indicador de carga
        const profileImage = document.getElementById('profileImage');
        const originalContent = profileImage.outerHTML;
        profileImage.outerHTML = '<div id="profileImage" class="bg-light rounded-circle d-flex align-items-center justify-content-center" style="width: 120px; height: 120px; margin: 0 auto;"><div class="spinner-border text-primary" role="status"></div></div>';

        fetch('/Auth/UploadProfilePhoto', {
            method: 'POST',
            body: formData,
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            }
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                // Actualizar imagen de perfil
                document.getElementById('profileImage').outerHTML = 
                    '<img id="profileImage" src="' + data.photoUrl + '?t=' + new Date().getTime() + '" alt="Foto de perfil" class="rounded-circle" style="width: 120px; height: 120px; object-fit: cover;">';
                
                // Agregar botón de eliminar si no existe y no es la imagen por defecto
                if (!data.photoUrl.includes('user_default.png') && !document.querySelector('button[onclick="deleteProfilePhoto()"]')) {
                    this.addDeleteButton();
                }
                
                // Mostrar mensaje de éxito
                this.showAlert('success', 'Foto de perfil actualizada exitosamente');
            } else {
                // Restaurar imagen original
                document.getElementById('profileImage').outerHTML = originalContent;
                this.showAlert('danger', data.message || 'Error al subir la foto');
            }
        })
        .catch(error => {
            // Restaurar imagen original
            document.getElementById('profileImage').outerHTML = originalContent;
            this.showAlert('danger', 'Error de conexión al subir la foto');
        });

        // Limpiar input
        fileInput.value = '';
    }

    addDeleteButton() {
        // Buscar el contenedor de la imagen
        const profileImageContainer = document.getElementById('profileImage').parentElement;
        
        // Verificar si ya existe el botón
        if (profileImageContainer.querySelector('button[onclick="deleteProfilePhoto()"]')) {
            return; // Ya existe, no agregar duplicado
        }
        
        // Crear el botón de eliminar
        const deleteButton = document.createElement('button');
        deleteButton.type = 'button';
        deleteButton.className = 'btn btn-sm btn-danger position-absolute';
        deleteButton.style.cssText = 'bottom: 5px; right: 5px;';
        deleteButton.setAttribute('onclick', 'deleteProfilePhoto()');
        deleteButton.setAttribute('title', 'Eliminar foto');
        deleteButton.innerHTML = '<i class="fas fa-trash"></i>';
        
        // Agregar el botón al contenedor
        profileImageContainer.appendChild(deleteButton);
        
        console.log('Botón de eliminar agregado dinámicamente');
    }

    deleteProfilePhoto() {
        // Mostrar modal de confirmación en lugar de confirm() simple
        this.showDeletePhotoModal();
    }

    showDeletePhotoModal() {
        // Resetear el checkbox
        const checkbox = document.getElementById('confirmDeletePhoto');
        const confirmBtn = document.getElementById('confirmDeletePhotoBtn');
        
        checkbox.checked = false;
        confirmBtn.disabled = true;
        
        // Configurar event listeners
        this.setupDeleteModalListeners();
        
        // Mostrar modal
        const modal = new bootstrap.Modal(document.getElementById('deletePhotoModal'));
        modal.show();
    }

    setupDeleteModalListeners() {
        const checkbox = document.getElementById('confirmDeletePhoto');
        const confirmBtn = document.getElementById('confirmDeletePhotoBtn');
        
        // Remover listeners previos para evitar duplicados
        checkbox.removeEventListener('change', this.handleCheckboxChange);
        confirmBtn.removeEventListener('click', this.handleConfirmDelete);
        
        // Agregar nuevos listeners
        this.handleCheckboxChange = () => {
            confirmBtn.disabled = !checkbox.checked;
        };
        
        this.handleConfirmDelete = () => {
            // Cerrar modal
            const modal = bootstrap.Modal.getInstance(document.getElementById('deletePhotoModal'));
            modal.hide();
            
            // Proceder con la eliminación
            this.executeDeleteProfilePhoto();
        };
        
        checkbox.addEventListener('change', this.handleCheckboxChange);
        confirmBtn.addEventListener('click', this.handleConfirmDelete);
    }

    executeDeleteProfilePhoto() {
        console.log('Iniciando eliminación de foto de perfil...');

        // Mostrar indicador de carga
        const profileImage = document.getElementById('profileImage');
        const originalContent = profileImage.outerHTML;
        profileImage.outerHTML = '<div id="profileImage" class="bg-light rounded-circle d-flex align-items-center justify-content-center" style="width: 120px; height: 120px; margin: 0 auto;"><div class="spinner-border text-primary" role="status"></div></div>';

        // Crear FormData con el token antiforgery
        const formData = new FormData();
        const token = $('input[name="__RequestVerificationToken"]').val();
        console.log('Token encontrado:', token ? 'Sí' : 'No');
        formData.append('__RequestVerificationToken', token);

        fetch('/Auth/DeleteProfilePhoto', {
            method: 'POST',
            body: formData
        })
        .then(response => {
            console.log('Respuesta del servidor:', response.status);
            return response.json();
        })
        .then(data => {
            console.log('Datos recibidos:', data);
            if (data.success) {
                // Actualizar imagen de perfil con la imagen por defecto
                document.getElementById('profileImage').outerHTML = 
                    '<img id="profileImage" src="' + data.photoUrl + '?t=' + new Date().getTime() + '" alt="Foto de perfil" class="rounded-circle" style="width: 120px; height: 120px; object-fit: cover;">';
                
                // Remover el botón de eliminar si existe
                const deleteButton = document.querySelector('button[onclick="deleteProfilePhoto()"]');
                if (deleteButton) {
                    deleteButton.remove();
                }
                
                // Mostrar mensaje de éxito
                this.showAlert('success', 'Foto de perfil eliminada exitosamente');
                
                // Recargar la página después de 2 segundos para actualizar la vista
                setTimeout(() => {
                    window.location.reload();
                }, 2000);
            } else {
                // Restaurar imagen original
                document.getElementById('profileImage').outerHTML = originalContent;
                this.showAlert('danger', data.message || 'Error al eliminar la foto');
            }
        })
        .catch(error => {
            console.error('Error en la petición:', error);
            // Restaurar imagen original
            document.getElementById('profileImage').outerHTML = originalContent;
            this.showAlert('danger', 'Error de conexión al eliminar la foto');
        });
    }

    showAlert(type, message) {
        const alertHtml = `
            <div class="alert alert-${type} alert-dismissible fade show" role="alert">
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        `;
        $('.card-body').first().prepend(alertHtml);
        
        // Auto-dismiss después de 5 segundos
        setTimeout(() => {
            $('.alert').fadeOut();
        }, 5000);
    }
}

// Global functions for backward compatibility
let authProfileManager;

// Inicializar inmediatamente
try {
    authProfileManager = new AuthProfileManager();
    console.log('AuthProfileManager inicializado inmediatamente');
} catch (error) {
    console.log('Error al inicializar inmediatamente, esperando DOM:', error);
}

function uploadProfilePhoto() {
    if (authProfileManager) {
        authProfileManager.uploadProfilePhoto();
    }
}

function deleteProfilePhoto() {
    console.log('deleteProfilePhoto called, authProfileManager:', authProfileManager);
    if (authProfileManager) {
        authProfileManager.deleteProfilePhoto();
    } else {
        console.error('authProfileManager no está inicializado');
        alert('Error: El sistema no está listo. Por favor, recargue la página.');
    }
}

// Initialize when DOM is loaded
$(document).ready(function() {
    console.log('Inicializando AuthProfileManager...');
    authProfileManager = new AuthProfileManager();
    console.log('AuthProfileManager inicializado:', authProfileManager);
});

// También inicializar con vanilla JavaScript por si jQuery no está listo
document.addEventListener('DOMContentLoaded', function() {
    if (!authProfileManager) {
        console.log('Inicializando AuthProfileManager con vanilla JS...');
        authProfileManager = new AuthProfileManager();
        console.log('AuthProfileManager inicializado con vanilla JS:', authProfileManager);
    }
});
