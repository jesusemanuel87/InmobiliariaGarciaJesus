// Main initialization file for Propietarios module
// This file orchestrates all the modules and initializes the application

// Global variable to hold the manager instance
let propietarioManager = null;

// Initialize when DOM is loaded
$(document).ready(function() {
    // Initialize the main manager
    propietarioManager = new PropietarioIndexManager();
    
    // Make it globally available
    window.propietarioManager = propietarioManager;
    
    console.log('Propietarios module initialized');
});
