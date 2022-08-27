namespace PrefeituraBrasil.MailSender
{
    public class EmailAddress
    {
        public string Nome { get; set; }
        public string Endereco { get; set; }

        public EmailAddress()
        {

        }

        public EmailAddress(string nome, string enderecoDeEmail)
        {
            Nome = nome;
            Endereco = enderecoDeEmail;
        }
    }
}
