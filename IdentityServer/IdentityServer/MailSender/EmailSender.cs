using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;
using MimeKit.Text;
using PrefeituraBrasil.MailSender.Interfaces;
using Serilog;

namespace PrefeituraBrasil.MailSender
{
    public class EmailSender : IEnviadorDeEmail, IEmailSender
    {
        private readonly IEmailSmtpConfig _emailConfiguration;

        private MailboxAddress EnderecoEnviadorPadrao
        {
            get
            {
                // Um endereço padrão para o "enviador" do email, caso nenhum seja específicado
                return new MailboxAddress("Prefeitura Brasil", "suporte@prefeiturabrasil.com.br");
            }
        }
        public EmailSender(IEmailSmtpConfig emailConfiguration)
        {
            _emailConfiguration = emailConfiguration;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            EmailMessage emailMessage = new EmailMessage();
            emailMessage.Subject = subject;
            emailMessage.ToAddresses = new List<EmailAddress>() { new EmailAddress(email, email) };
            emailMessage.Content = htmlMessage;

            return SendAsync(emailMessage);
        }

        public async Task SendAsync(EmailMessage emailMessage)
        {
            try
            {
                var message = new MimeMessage();

                if (emailMessage.ToAddresses.Count <= 0)
                    throw new ArgumentException("O email precisa ter pelo menos um destinatário");

                message.To.AddRange(emailMessage.ToAddresses.Select(x => new MailboxAddress(x.Nome, x.Endereco)));

                if (emailMessage.FromAddresses.Count <= 0)
                    message.From.Add(EnderecoEnviadorPadrao);
                else
                    message.From.AddRange(emailMessage.FromAddresses.Select(x => new MailboxAddress(x.Nome, x.Endereco)));

                message.Subject = emailMessage.Subject;

                message.Body = new TextPart(TextFormat.Html)
                {
                    Text = emailMessage.Content
                };

                using (var emailClient = new SmtpClient())
                {
                    await emailClient.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTlsWhenAvailable);

                    //Remove qualquer funcionalidade OAuth já que não usamos. 
                    emailClient.AuthenticationMechanisms.Remove("XOAUTH2");
                    await emailClient.AuthenticateAsync(_emailConfiguration.SmtpUsername, _emailConfiguration.SmtpPassword);
                    await emailClient.SendAsync(message);
                    await emailClient.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ocorreu um erro ao enviar o email.");
                throw;
            }
        }
    }
}
