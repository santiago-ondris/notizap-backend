    public class MercadoLibreSettings
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string RedirectUri { get; set; } = "https://b36b-2803-9800-b887-7e77-78a4-40b5-3b93-a04b.ngrok-free.app/api/auth/mercadolibre/callback";
        public string ApiBaseUrl { get; set; } = "https://api.mercadolibre.com";
        public string SellerId { get; set; } = string.Empty;
    }