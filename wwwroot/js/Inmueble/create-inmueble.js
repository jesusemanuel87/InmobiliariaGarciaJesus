// Inmueble creation functionality
class InmuebleCreateManager {
    constructor() {
        this.init();
    }

    init() {
        $(document).ready(() => {
            this.initializeGeorefDropdowns();
            console.log('InmuebleCreateManager initialized');
        });
    }

    initializeGeorefDropdowns() {
        // Initialize province and locality dropdowns with San Luis as default
        if (typeof georefService !== 'undefined') {
            georefService.inicializarDropdowns('provinciaSelect', 'localidadSelect', 'San Luis', 'San Luis');
        } else {
            console.error('georefService not available');
        }
    }
}

// Initialize when DOM is loaded
$(document).ready(function() {
    new InmuebleCreateManager();
});
