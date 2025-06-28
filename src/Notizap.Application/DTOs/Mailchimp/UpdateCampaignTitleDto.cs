using System.ComponentModel.DataAnnotations;

public class UpdateCampaignTitleDto
{
    [Required(ErrorMessage = "El título es requerido")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "El título debe tener entre 1 y 200 caracteres")]
    public string Title { get; set; } = default!;
}