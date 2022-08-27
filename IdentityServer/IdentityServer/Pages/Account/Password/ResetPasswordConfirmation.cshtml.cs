// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServer.Pages.Account.Password
{

    [AllowAnonymous]
    public class ResetPasswordConfirmationModel : PageModel
    {
        public string ReturnUrl { get; set; }

        public IActionResult OnGet(string returnURL = null)
        {
            // No cshtml´será composta uma URL onde a URL principal é a URL da tela de login e a returnURL é a returnURL recebida por parametro aqui.
            // Isso faz com que, se o usuário começou o processo de reset de senha a partir de uma página do cliente (Angular, por exemplo), depois de resetar
            // ela será redirecionado para o local de onde ele iniciou o processo de reset de senha.
            ReturnUrl = returnURL;
            return Page();
        }
    }
}
