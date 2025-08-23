using System.ComponentModel.DataAnnotations;

namespace InmobiliariaGarciaJesus.Models
{
    public class Propietario
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El DNI es obligatorio")]
        [StringLength(20, ErrorMessage = "El DNI no puede exceder 20 caracteres")]
        [Display(Name = "DNI")]
        public string DNI { get; set; } = "";

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = "";

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(100, ErrorMessage = "El apellido no puede exceder 100 caracteres")]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; } = "";

        [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        [StringLength(100, ErrorMessage = "El email no puede exceder 100 caracteres")]
        [Display(Name = "Email")]
        public string Email { get; set; } = "";

        [StringLength(200, ErrorMessage = "La dirección no puede exceder 200 caracteres")]
        [Display(Name = "Dirección")]
        public string? Direccion { get; set; }

        [Display(Name = "Fecha de Creación")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [Display(Name = "Estado")]
        public bool Estado { get; set; } = true;

        // Propiedad calculada para mostrar nombre completo
        [Display(Name = "Nombre Completo")]
        public string NombreCompleto => $"{Nombre} {Apellido}";

    }
}
