// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using IdentityServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
//using PrefeituraBrasil.Domain.Entities.Global.Usuarios;

namespace IdentityServer.Pages.Account.Register;

[AllowAnonymous]
public class ConfirmEmailModel : PageModel
{
    public bool ShowAutoRedirectTimer = true;
    private static string UrlLoginPadrao = "/Account/Login/Index";
    public string UrlLoginRetornar = "/Account/Login/Index";
    public bool IsLocalLogin { get; set; }

    private UserManager<ApplicationUser> _userManager;
    public ConfirmEmailModel(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [TempData]
    public string StatusMessage { get; set; }
    public async Task<IActionResult> OnGetAsync(string userId, string code, string returnUrl = null)
    {
        if (userId == null || code == null)
        {
            return RedirectToPage("/Index");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound($"Não foi possível encontrar o usuário com o ID '{userId}'.");
        }

        code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        var result = await _userManager.ConfirmEmailAsync(user, code);
        StatusMessage = result.Succeeded ? "Obrigado por confirmar seu email." : "Ocorreu um erro ao confirmar seu email.";

        if (String.IsNullOrWhiteSpace(returnUrl) || returnUrl.Trim() == "/")
            this.IsLocalLogin = true;
        else
            this.IsLocalLogin = false;

        this.UrlLoginRetornar = ObterUrlLoginRetornar(returnUrl);

        ///TODO: Depois de confirmado o email, como faço pra saber o "ReturnURL"? Ou seja, o usuário confirmou o email mas ele deve fazer login em qual client?
        return Page();
    }

    private string ObterUrlLoginRetornar(string retUrl = null)
    {
        if (String.IsNullOrWhiteSpace(retUrl))
            retUrl = "/";

        var callbackUrl = Url.Page(UrlLoginPadrao,
                                   pageHandler: null,
                                   values: new { area = "", ReturnUrl = retUrl },
                                   protocol: Request.Scheme);

        return callbackUrl;
    }
}

