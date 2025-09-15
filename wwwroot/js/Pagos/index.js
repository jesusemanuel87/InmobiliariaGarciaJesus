// Declare global variable
var pagosManager;

// Pagos module entry point
$(document).ready(function() {
    console.log('Pagos module initialized');
    
    // Initialize the Pagos Index Manager and make it globally available
    if (!pagosManager) {
        pagosManager = new PagosIndexManager();
        window.pagosManager = pagosManager;
    }
});
