
using System;
using System.Text;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;


namespace IdentityServer.Pages.Account.Register;

[AllowAnonymous]
public class RegisterConfirmationModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _sender;

    public RegisterConfirmationModel(UserManager<ApplicationUser> userManager, IEmailSender sender)
    {
        _userManager = userManager;
        _sender = sender;
    }

    public string Email { get; set; }
    public bool IsLocalLogin { get; set; }
    public string EmailConfirmationUrl { get; set; }

    private static string UrlLoginPadrao = "/Account/Login/Index";
    public string UrlLoginRetornar = "/Account/Login/Index";

    public async Task<IActionResult> OnGetAsync(string email, string returnUrl = null)
    {
        if (email == null)
        {
            return RedirectToPage("/Index");
        }

        if (String.IsNullOrWhiteSpace(returnUrl) || returnUrl.Trim() =="/")
            this.IsLocalLogin = true;
        else
            this.IsLocalLogin = false;

        returnUrl = returnUrl ?? Url.Content("~/");      

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            ///TODO Mostrar isso usando uma pagina com layout (atualmente dá um erro em branco)
            return NotFound($"Não foi possível encontrar o usuário com o email '{email}'.");
        }

        // se tem algo no returnUrl, tem que adicionar esse conteúdo como query string para "progapar" a intenção inicial do usuário.
        this.UrlLoginRetornar = ObterUrlLoginRetornar(returnUrl);

        Email = email;

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

