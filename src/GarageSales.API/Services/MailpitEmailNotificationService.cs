using MailKit.Net.Smtp;
using MimeKit;

public class MailpitEmailNotificationService : IEmailNotificationService
{
    private static string host = "localhost";
    private static int port = 1025; // Mailpit default SMTP port is 1025

    public MailpitEmailNotificationService()
    {
    }

    public async Task SendGarageSaleNotificationAsync(
        string recipientEmail, 
        string recipientUserName, 
        string saleAddress, 
        double saleDistance,
        DateOnly saleDate)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("City of Palm Coast", "no-reply@palmcoast.local"));
        message.To.Add(new MailboxAddress(recipientUserName, recipientEmail));
        message.Subject = "New Garage Sale Near You!";

        message.Body = new TextPart("html")
        {
            Text = $"<p>Dear {recipientUserName},</p>"
                + $"<p>A new garage sale has been registered matching your notification preferences.</p>"
                + $"<p>The sale will begin on <strong>{saleDate:MMMM d, yyyy}</strong> at <strong>{saleAddress}</strong>.</p>"
                + $"<p>The sale is <strong>{saleDistance:F2} miles</strong> away from you.</p>"
        };

        using var client = new SmtpClient();
        await client.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.None);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}