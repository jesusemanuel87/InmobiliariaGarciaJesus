// Main initialization file for Empleados module
// This file orchestrates all the modules and initializes the application

// Global variable to hold the manager instance
let empleadoManager = null;

// Initialize when DOM is loaded
$(document).ready(function() {
    // Initialize the main manager
    empleadoManager = new EmpleadoIndexManager();
    
    // Make it globally available
    window.empleadoManager = empleadoManager;
    
    console.log('Empleados module initialized');
});
