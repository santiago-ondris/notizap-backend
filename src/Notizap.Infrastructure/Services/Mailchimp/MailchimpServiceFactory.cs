using Microsoft.Extensions.Options;

public class MailchimpServiceFactory : IMailchimpServiceFactory
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly MailchimpSettings _settings;

    public MailchimpServiceFactory(IHttpClientFactory factory, IOptions<MailchimpSettings> settings)
    {
        _httpFactory = factory;
        _settings = settings.Value;
    }

    public IMailchimpService Create(string cuenta)
    {
        if (!_settings.Accounts.TryGetValue(cuenta, out var config))
            throw new ArgumentException($"No existe configuración para la cuenta: {cuenta}");

        var client = _httpFactory.CreateClient();
        return new MailchimpService(client, config);
    }
}
