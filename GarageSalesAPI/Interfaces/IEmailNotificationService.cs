public interface IEmailNotificationService
{
    Task SendGarageSaleNotificationAsync(
        string recipientEmail, 
        string recipientUserName,
        string saleAddress,
        double saleDistance,
        DateOnly saleDate);
}