namespace PrefeituraBrasil.MailSender.Interfaces
{
    public interface IEmailSmtpConfig
    {
        string SmtpServer { get; }
        int SmtpPort { get; }
        string SmtpUsername { get; set; }
        string SmtpPassword { get; set; }
    }
}