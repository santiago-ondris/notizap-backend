using System.ComponentModel.DataAnnotations;

public class ActualizarTelefonoDto
{
    [Required(ErrorMessage = "El teléfono es requerido")]
    [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
    public string Telefono { get; set; } = string.Empty;
}