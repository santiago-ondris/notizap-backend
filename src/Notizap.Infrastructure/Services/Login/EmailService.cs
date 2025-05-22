using MailKit.Net.Smtp;
using MimeKit;

public class EmailService : IEmailService
{
    private readonly string _fromEmail = "montellacalzados@gmail.com";
    private readonly string _fromName = "Notizap";
    private readonly string _password = "zunyhyjaxdstmitv";

    public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_fromName, _fromEmail));
        message.To.Add(new MailboxAddress(toEmail, toEmail));
        message.Subject = "Recuperá tu contraseña en Notizap";

        message.Body = new TextPart("plain")
        {
            Text = $"Hola,\n\nRecibimos una solicitud para restablecer tu contraseña. " +
                   $"Hacé click en el siguiente enlace para crear una nueva contraseña:\n\n{resetLink}\n\n" +
                   "Si no solicitaste este cambio, podés ignorar este mensaje.\n\n¡Saludos gringos!"
        };

        using var client = new SmtpClient();
        await client.ConnectAsync("smtp.gmail.com", 587, false);
        await client.AuthenticateAsync(_fromEmail, _password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
