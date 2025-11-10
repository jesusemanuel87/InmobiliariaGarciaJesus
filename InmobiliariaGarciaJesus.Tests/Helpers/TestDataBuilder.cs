using InmobiliariaGarciaJesus.Models.DTOs;

namespace InmobiliariaGarciaJesus.Tests.Helpers
{
    /// <summary>
    /// Builder para crear CrearInmuebleDto con valores por defecto válidos
    /// Facilita la creación de DTOs en tests usando patrón fluent
    /// </summary>
    public class CrearInmuebleDtoBuilder
    {
        private readonly CrearInmuebleDto _dto;

        public CrearInmuebleDtoBuilder()
        {
            // Valores por defecto válidos
            _dto = new CrearInmuebleDto
            {
                Direccion = "Av. Test 123",
                Localidad = "San Luis",
                Provincia = "San Luis",
                TipoId = 1, // Casa (seeded en CustomWebApplicationFactory)
                Ambientes = 3,
                Superficie = 100m,
                Precio = 150000m,
                Uso = 0, // Residencial
                Latitud = -33.3017m, // Coordenadas de San Luis
                Longitud = -66.3378m
            };
        }

        public CrearInmuebleDtoBuilder ConDireccion(string direccion)
        {
            _dto.Direccion = direccion;
            return this;
        }

        public CrearInmuebleDtoBuilder ConLocalidad(string localidad)
        {
            _dto.Localidad = localidad;
            return this;
        }

        public CrearInmuebleDtoBuilder ConProvincia(string provincia)
        {
            _dto.Provincia = provincia;
            return this;
        }

        public CrearInmuebleDtoBuilder ConTipoId(int tipoId)
        {
            _dto.TipoId = tipoId;
            return this;
        }

        public CrearInmuebleDtoBuilder ConAmbientes(int ambientes)
        {
            _dto.Ambientes = ambientes;
            return this;
        }

        public CrearInmuebleDtoBuilder ConSuperficie(decimal superficie)
        {
            _dto.Superficie = superficie;
            return this;
        }

        public CrearInmuebleDtoBuilder ConPrecio(decimal precio)
        {
            _dto.Precio = precio;
            return this;
        }

        public CrearInmuebleDtoBuilder ConUso(int uso)
        {
            _dto.Uso = uso;
            return this;
        }

        public CrearInmuebleDtoBuilder ConCoordenadas(decimal latitud, decimal longitud)
        {
            _dto.Latitud = latitud;
            _dto.Longitud = longitud;
            return this;
        }

        public CrearInmuebleDtoBuilder ConImagen(string base64, string nombre)
        {
            _dto.ImagenBase64 = base64;
            _dto.ImagenNombre = nombre;
            return this;
        }

        public CrearInmuebleDto Build() => _dto;
    }

    /// <summary>
    /// Builder para crear ActualizarEstadoInmuebleDto
    /// </summary>
    public class ActualizarEstadoInmuebleDtoBuilder
    {
        private readonly ActualizarEstadoInmuebleDto _dto;

        public ActualizarEstadoInmuebleDtoBuilder()
        {
            _dto = new ActualizarEstadoInmuebleDto();
        }

        public ActualizarEstadoInmuebleDtoBuilder ConEstado(string estado)
        {
            _dto.Estado = estado;
            return this;
        }

        public ActualizarEstadoInmuebleDtoBuilder Activo()
        {
            _dto.Estado = "Activo";
            return this;
        }

        public ActualizarEstadoInmuebleDtoBuilder Inactivo()
        {
            _dto.Estado = "Inactivo";
            return this;
        }

        public ActualizarEstadoInmuebleDto Build() => _dto;
    }
}
