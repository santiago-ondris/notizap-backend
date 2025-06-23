public interface IPlantillaWhatsAppService
{
    Task<List<PlantillaWhatsAppDto>> GetAllActivasAsync();
    Task<Dictionary<string, List<PlantillaWhatsAppDto>>> GetPorCategoriasAsync();
    Task<PlantillaWhatsAppDto?> GetByIdAsync(int id);
    Task<PlantillaWhatsAppDto> CrearAsync(CrearPlantillaWhatsAppDto dto, string username);
    Task<PlantillaWhatsAppDto?> ActualizarAsync(int id, ActualizarPlantillaWhatsAppDto dto);
    Task<bool> DesactivarAsync(int id);
}