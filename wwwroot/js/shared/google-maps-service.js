/**
 * Google Maps Service - Maneja la carga dinámica de Google Maps API
 * Este servicio centraliza la carga de Google Maps y evita cargas múltiples
 */
class GoogleMapsService {
    constructor() {
        this.isLoaded = false;
        this.isLoading = false;
        this.loadPromise = null;
        this.apiKey = null;
        this.callbacks = [];
    }

    /**
     * Configura la API Key de Google Maps
     * @param {string} apiKey - La API Key de Google Maps
     */
    setApiKey(apiKey) {
        this.apiKey = apiKey;
    }

    /**
     * Verifica si Google Maps ya está cargado
     * @returns {boolean} - True si Google Maps está disponible
     */
    isGoogleMapsLoaded() {
        return typeof google !== 'undefined' && google.maps;
    }

    /**
     * Carga Google Maps API de forma asíncrona
     * @param {function} callback - Función callback a ejecutar cuando Maps esté listo
     * @returns {Promise} - Promise que se resuelve cuando Maps está cargado
     */
    loadGoogleMaps(callback = null) {
        // Si ya está cargado, ejecutar callback inmediatamente
        if (this.isGoogleMapsLoaded()) {
            this.isLoaded = true;
            if (callback) callback();
            return Promise.resolve();
        }

        // Si ya hay una carga en progreso, agregar callback a la cola
        if (this.isLoading && this.loadPromise) {
            if (callback) this.callbacks.push(callback);
            return this.loadPromise;
        }

        // Validar que se haya configurado la API Key
        if (!this.apiKey) {
            const error = 'Google Maps API Key no configurada. Use setApiKey() primero.';
            console.error(error);
            return Promise.reject(new Error(error));
        }

        // Iniciar carga
        this.isLoading = true;
        if (callback) this.callbacks.push(callback);

        this.loadPromise = new Promise((resolve, reject) => {
            try {
                // Crear función callback global única
                const callbackName = 'googleMapsCallback_' + Date.now();
                
                window[callbackName] = () => {
                    this.isLoaded = true;
                    this.isLoading = false;
                    
                    // Ejecutar todos los callbacks pendientes
                    this.callbacks.forEach(cb => {
                        try {
                            cb();
                        } catch (error) {
                            console.error('Error en callback de Google Maps:', error);
                        }
                    });
                    this.callbacks = [];
                    
                    // Limpiar callback global
                    delete window[callbackName];
                    
                    resolve();
                };

                // Crear y agregar script
                const script = document.createElement('script');
                script.src = `https://maps.googleapis.com/maps/api/js?key=${this.apiKey}&callback=${callbackName}`;
                script.async = true;
                script.defer = true;
                
                script.onerror = () => {
                    this.isLoading = false;
                    const error = 'Error al cargar Google Maps API';
                    console.error(error);
                    delete window[callbackName];
                    reject(new Error(error));
                };

                document.head.appendChild(script);

            } catch (error) {
                this.isLoading = false;
                console.error('Error al inicializar carga de Google Maps:', error);
                reject(error);
            }
        });

        return this.loadPromise;
    }

    /**
     * Método de conveniencia para cargar Maps y ejecutar callback
     * @param {function} initMapFunction - Función que inicializa el mapa
     * @param {string} apiKey - API Key (opcional si ya se configuró)
     */
    async initializeMap(initMapFunction, apiKey = null) {
        try {
            if (apiKey) {
                this.setApiKey(apiKey);
            }

            await this.loadGoogleMaps();
            
            if (typeof initMapFunction === 'function') {
                initMapFunction();
            }
        } catch (error) {
            console.error('Error al inicializar Google Maps:', error);
        }
    }

    /**
     * Resetea el estado del servicio (útil para testing)
     */
    reset() {
        this.isLoaded = false;
        this.isLoading = false;
        this.loadPromise = null;
        this.callbacks = [];
    }
}

// Crear instancia global del servicio
window.googleMapsService = new GoogleMapsService();

// Exportar para uso en módulos (si es necesario)
if (typeof module !== 'undefined' && module.exports) {
    module.exports = GoogleMapsService;
}
