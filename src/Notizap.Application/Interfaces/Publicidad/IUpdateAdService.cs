public interface IUpdateAdService
{
    Task<List<AdCampaignReadDto>> GetAllCampaignsAsync(
        string? unidad = null, 
        string? plataforma = null, 
        int? year = null, 
        int? month = null, 
        int page = 1, 
        int pageSize = 20);
        
    Task<AdCampaignReadDto?> GetCampaignByIdAsync(int id);
    
    Task<AdCampaignReadDto?> UpdateCampaignAsync(int id, UpdateAdCampaignDto dto);
    
    Task<int> GetTotalCampaignsCountAsync(
        string? unidad = null, 
        string? plataforma = null, 
        int? year = null, 
        int? month = null);
}