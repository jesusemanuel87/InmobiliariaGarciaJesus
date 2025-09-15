/**
 * Servicio para integrar con la API de Georef de Argentina
 * https://apis.datos.gob.ar/georef/api/
 */

class GeorefService {
    constructor() {
        this.baseUrl = 'https://apis.datos.gob.ar/georef/api';
        this.cache = {
            provincias: null,
            localidades: {}
        };
    }

    /**
     * Obtiene todas las provincias de Argentina
     */
    async obtenerProvincias() {
        if (this.cache.provincias) {
            return this.cache.provincias;
        }

        try {
            const response = await fetch(`${this.baseUrl}/provincias?campos=id,nombre&max=24`);
            if (!response.ok) {
                throw new Error(`Error HTTP: ${response.status}`);
            }
            
            const data = await response.json();
            this.cache.provincias = data.provincias || [];
            return this.cache.provincias;
        } catch (error) {
            console.error('Error al obtener provincias:', error);
            return this.getProvinciasDefault();
        }
    }

    /**
     * Obtiene las localidades de una provincia específica
     */
    async obtenerLocalidades(provinciaNombre) {
        if (!provinciaNombre) return [];

        // Verificar cache
        if (this.cache.localidades[provinciaNombre]) {
            return this.cache.localidades[provinciaNombre];
        }

        try {
            const provinciaEncoded = encodeURIComponent(provinciaNombre);
            const response = await fetch(`${this.baseUrl}/localidades?provincia=${provinciaEncoded}&campos=id,nombre&max=1000`);
            
            if (!response.ok) {
                throw new Error(`Error HTTP: ${response.status}`);
            }
            
            const data = await response.json();
            const localidades = data.localidades || [];
            
            // Guardar en cache
            this.cache.localidades[provinciaNombre] = localidades;
            return localidades;
        } catch (error) {
            console.error(`Error al obtener localidades de ${provinciaNombre}:`, error);
            return this.getLocalidadesDefault(provinciaNombre);
        }
    }

    /**
     * Provincias por defecto en caso de error en la API
     */
    getProvinciasDefault() {
        return [
            { id: '74', nombre: 'San Luis' },
            { id: '02', nombre: 'Buenos Aires' },
            { id: '06', nombre: 'Buenos Aires' }, // CABA
            { id: '14', nombre: 'Córdoba' },
            { id: '50', nombre: 'Mendoza' },
            { id: '82', nombre: 'Santa Fe' },
            { id: '90', nombre: 'Tucumán' }
        ];
    }

    /**
     * Localidades por defecto para San Luis
     */
    getLocalidadesDefault(provincia) {
        if (provincia === 'San Luis') {
            return [
                { id: '740007', nombre: 'San Luis' },
                { id: '740014', nombre: 'Villa Mercedes' },
                { id: '740021', nombre: 'Merlo' },
                { id: '740028', nombre: 'La Punta' }
            ];
        }
        return [];
    }

    /**
     * Inicializa los dropdowns de provincia y localidad
     */
    async inicializarDropdowns(provinciaSelectId, localidadSelectId, provinciaDefault = 'San Luis', localidadDefault = 'San Luis') {
        const provinciaSelect = document.getElementById(provinciaSelectId);
        const localidadSelect = document.getElementById(localidadSelectId);

        if (!provinciaSelect || !localidadSelect) {
            console.error('No se encontraron los elementos select especificados');
            return;
        }

        // Mostrar loading
        provinciaSelect.innerHTML = '<option value="">Cargando provincias...</option>';
        localidadSelect.innerHTML = '<option value="">Seleccione primero una provincia</option>';

        try {
            // Cargar provincias
            const provincias = await this.obtenerProvincias();
            this.poblarSelectProvincias(provinciaSelect, provincias, provinciaDefault);

            // Configurar evento change para provincia
            provinciaSelect.addEventListener('change', async (e) => {
                await this.cargarLocalidades(e.target.value, localidadSelect);
            });

            // Cargar localidades por defecto si hay provincia seleccionada
            if (provinciaDefault) {
                await this.cargarLocalidades(provinciaDefault, localidadSelect, localidadDefault);
            }

        } catch (error) {
            console.error('Error al inicializar dropdowns:', error);
            provinciaSelect.innerHTML = '<option value="">Error al cargar provincias</option>';
        }
    }

    /**
     * Poblar el select de provincias
     */
    poblarSelectProvincias(select, provincias, defaultValue = null) {
        select.innerHTML = '<option value="">Seleccionar provincia...</option>';
        
        // Ordenar provincias alfabéticamente
        provincias.sort((a, b) => a.nombre.localeCompare(b.nombre));

        provincias.forEach(provincia => {
            const option = document.createElement('option');
            option.value = provincia.nombre;
            option.textContent = provincia.nombre;
            
            if (defaultValue && provincia.nombre === defaultValue) {
                option.selected = true;
            }
            
            select.appendChild(option);
        });
    }

    /**
     * Cargar localidades para una provincia
     */
    async cargarLocalidades(provinciaNombre, localidadSelect, defaultValue = null) {
        if (!provinciaNombre) {
            localidadSelect.innerHTML = '<option value="">Seleccione primero una provincia</option>';
            return;
        }

        // Mostrar loading
        localidadSelect.innerHTML = '<option value="">Cargando localidades...</option>';

        try {
            const localidades = await this.obtenerLocalidades(provinciaNombre);
            this.poblarSelectLocalidades(localidadSelect, localidades, defaultValue);
        } catch (error) {
            console.error('Error al cargar localidades:', error);
            localidadSelect.innerHTML = '<option value="">Error al cargar localidades</option>';
        }
    }

    /**
     * Poblar el select de localidades
     */
    poblarSelectLocalidades(select, localidades, defaultValue = null) {
        select.innerHTML = '<option value="">Seleccionar localidad...</option>';
        
        // Ordenar localidades alfabéticamente
        localidades.sort((a, b) => a.nombre.localeCompare(b.nombre));

        localidades.forEach(localidad => {
            const option = document.createElement('option');
            option.value = localidad.nombre;
            option.textContent = localidad.nombre;
            
            if (defaultValue && localidad.nombre === defaultValue) {
                option.selected = true;
            }
            
            select.appendChild(option);
        });
    }
}

// Crear instancia global
window.georefService = new GeorefService();
