namespace PrefeituraBrasil.MailSender.Interfaces
{
    public interface IEnviadorDeEmail
    {
        Task SendAsync(EmailMessage emailMessage);
    }
}