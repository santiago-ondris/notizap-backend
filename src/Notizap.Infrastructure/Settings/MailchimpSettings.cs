public class MailchimpAccountSettings
{
    public string ApiKey { get; set; } = default!;
    public string ServerPrefix { get; set; } = default!;
}

public class MailchimpSettings
{
    public Dictionary<string, MailchimpAccountSettings> Accounts { get; set; } = new();
}
