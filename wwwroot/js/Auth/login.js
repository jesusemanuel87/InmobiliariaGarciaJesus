// Auth login functionality
class AuthLoginManager {
    constructor() {
        this.init();
    }

    init() {
        $(document).ready(() => {
            this.bindPasswordToggle();
            console.log('AuthLoginManager initialized');
        });
    }

    bindPasswordToggle() {
        document.getElementById('togglePassword').addEventListener('click', () => {
            const passwordField = document.getElementById('Password');
            const toggleIcon = document.getElementById('toggleIcon');
            
            if (passwordField.type === 'password') {
                passwordField.type = 'text';
                toggleIcon.classList.remove('fa-eye');
                toggleIcon.classList.add('fa-eye-slash');
            } else {
                passwordField.type = 'password';
                toggleIcon.classList.remove('fa-eye-slash');
                toggleIcon.classList.add('fa-eye');
            }
        });
    }
}

// Initialize when DOM is loaded
$(document).ready(function() {
    new AuthLoginManager();
});
