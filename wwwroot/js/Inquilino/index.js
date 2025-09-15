// Main initialization file for Inquilinos module
// This file orchestrates all the modules and initializes the application

// Global variable to hold the manager instance
let inquilinoManager = null;

// Initialize when DOM is loaded
$(document).ready(function() {
    // Initialize the main manager
    inquilinoManager = new InquilinoIndexManager();
    
    // Make it globally available
    window.inquilinoManager = inquilinoManager;
    
    console.log('Inquilinos module initialized');
});
