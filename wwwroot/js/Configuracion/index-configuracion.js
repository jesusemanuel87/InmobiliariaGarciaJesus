// Configuracion index functionality
class ConfiguracionIndexManager {
    constructor() {
        this.init();
    }

    init() {
        $(document).ready(() => {
            this.bindAutoDismissAlerts();
            console.log('ConfiguracionIndexManager initialized');
        });
    }

    bindAutoDismissAlerts() {
        // Auto-dismiss alerts after 5 seconds
        setTimeout(() => {
            $('.alert').fadeOut();
        }, 5000);
    }
}

// Initialize when DOM is loaded
$(document).ready(function() {
    new ConfiguracionIndexManager();
});
