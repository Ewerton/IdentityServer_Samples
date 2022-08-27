using System.ComponentModel;

namespace PrefeituraBrasil.IdentityServer.Models.Enums
{
    public enum TipoLogin
    {
        [Description("E-mail")]
        EMAIL = 0,

        [Description("CPF")]
        CPF = 1
    }


    public enum TipoConfirmacaoCadastro
    {
        [Description("E-mail")]
        EMAIL = 0,

        [Description("SMS")]
        SMS = 1
    }
}
