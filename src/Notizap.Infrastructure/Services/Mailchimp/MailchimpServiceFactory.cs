using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class MailchimpServiceFactory : IMailchimpServiceFactory
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly MailchimpSettings _settings;
    private readonly IServiceProvider _serviceProvider;

    public MailchimpServiceFactory(
        IHttpClientFactory factory, 
        IOptions<MailchimpSettings> settings,
        IServiceProvider serviceProvider)
    {
        _httpFactory = factory;
        _settings = settings.Value;
        _serviceProvider = serviceProvider;
    }

    public IMailchimpService Create(string cuenta)
    {
        if (!_settings.Accounts.TryGetValue(cuenta, out var config))
            throw new ArgumentException($"No existe configuración para la cuenta: {cuenta}");

        var client = _httpFactory.CreateClient();
        var logger = _serviceProvider.GetRequiredService<ILogger<MailchimpService>>();
        
        return new MailchimpService(client, config, logger);
    }
}