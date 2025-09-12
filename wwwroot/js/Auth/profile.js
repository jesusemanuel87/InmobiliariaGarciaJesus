// Auth profile functionality
class AuthProfileManager {
    constructor() {
        this.init();
    }

    init() {
        $(document).ready(() => {
            this.bindPasswordConfirmation();
            console.log('AuthProfileManager initialized');
        });
    }

    bindPasswordConfirmation() {
        $('input[name="ConfirmPassword"]').on('input', (e) => {
            const newPassword = $('input[name="NewPassword"]').val();
            const confirmPassword = $(e.target).val();
            
            if (newPassword !== confirmPassword) {
                e.target.setCustomValidity('Las contraseñas no coinciden');
            } else {
                e.target.setCustomValidity('');
            }
        });
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

function uploadProfilePhoto() {
    if (authProfileManager) {
        authProfileManager.uploadProfilePhoto();
    }
}

// Initialize when DOM is loaded
$(document).ready(function() {
    authProfileManager = new AuthProfileManager();
});
