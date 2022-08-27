using System;

namespace IdentityServer.Pages.Account.Login;

public class LoginOptions
{
    /// TODO: Colocar como false em Produção.
    public static bool AllowLocalLogin = true;
    public static bool AllowRememberLogin = true;
    public static TimeSpan RememberMeLoginDuration = TimeSpan.FromDays(30);
    public static string InvalidCredentialsErrorMessage = "Usuário e/ou senha inválidos";
}