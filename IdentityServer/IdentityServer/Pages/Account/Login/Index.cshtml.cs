using Duende.IdentityServer.Events;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using IdentityServer.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PrefeituraBrasil.IdentityServer;
using PrefeituraBrasil.IdentityServer.Extensions;
using PrefeituraBrasil.IdentityServer.Models.Enums;

namespace IdentityServer.Pages.Account.Login;

[SecurityHeaders]
[AllowAnonymous]
public class Index : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IIdentityServerInteractionService _interaction;
    private readonly IClientStore _clientStore;
    private readonly IEventService _events;
    private readonly IAuthenticationSchemeProvider _schemeProvider;
    private readonly IIdentityProviderStore _identityProviderStore;

    public ViewModel View { get; set; }

    [BindProperty]
    public InputModel Input { get; set; }

    public Index(
        IIdentityServerInteractionService interaction,
        IClientStore clientStore,
        IAuthenticationSchemeProvider schemeProvider,
        IIdentityProviderStore identityProviderStore,
        IEventService events,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _interaction = interaction;
        _clientStore = clientStore;
        _schemeProvider = schemeProvider;
        _identityProviderStore = identityProviderStore;
        _events = events;
    }

    public async Task<IActionResult> OnGet(string returnUrl)
    {
        await BuildModelAsync(returnUrl);

        if (View.IsExternalLoginOnly)
        {
            // we only have one option for logging in and it's an external provider
            return RedirectToPage("/ExternalLogin/Challenge", new { scheme = View.ExternalLoginScheme, returnUrl });
        }

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        // check if we are in the context of an authorization request
        var context = await _interaction.GetAuthorizationContextAsync(Input.ReturnUrl);

        // the user clicked the "cancel" button
        if (Input.Button != "login")
        {
            if (context != null)
            {
                // if the user cancels, send a result back into IdentityServer as if they 
                // denied the consent (even if this client does not require consent).
                // this will send back an access denied OIDC error response to the client.
                await _interaction.DenyAuthorizationAsync(context, AuthorizationError.AccessDenied);

                // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                if (context.IsNativeClient())
                {
                    // The client is native, so this change in how to
                    // return the response is for better UX for the end user.
                    return this.LoadingPage(Input.ReturnUrl);
                }

                return Redirect(Input.ReturnUrl);
            }
            else
            {
                // since we don't have a valid context, then we just go back to the home page
                return Redirect("~/");
            }
        }

        if (!await Validar(Input))
        {
            await BuildModelAsync(Input.ReturnUrl);
            return Page();
        }

        //    if (ModelState.IsValid)
        //{
        ApplicationUser user = await ObterUsuarioPorTipoLogin(Input.TipoLogin);

        var result = await _signInManager.PasswordSignInAsync(user.UserName, Input.Password, Input.RememberLogin, lockoutOnFailure: true);
        if (result.Succeeded)
        {
            await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id, user.UserName, clientId: context?.Client.ClientId));

            if (context != null)
            {
                if (context.IsNativeClient())
                {
                    // The client is native, so this change in how to
                    // return the response is for better UX for the end user.
                    return this.LoadingPage(Input.ReturnUrl);
                }

                // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                return Redirect(Input.ReturnUrl);
            }

            // request for a local page
            if (Url.IsLocalUrl(Input.ReturnUrl))
            {
                return Redirect(Input.ReturnUrl);
            }
            else if (string.IsNullOrEmpty(Input.ReturnUrl))
            {
                return Redirect("~/");
            }
            else
            {
                // user might have clicked on a malicious link - should be logged
                throw new Exception("invalid return URL");
            }
        }

        await _events.RaiseAsync(new UserLoginFailureEvent(user.UserName, "invalid credentials", clientId: context?.Client.ClientId));
        ModelState.AddModelError(string.Empty, LoginOptions.InvalidCredentialsErrorMessage);

        // something went wrong, show form with error
        await BuildModelAsync(Input.ReturnUrl);
        return Page();
    }

    private async Task BuildModelAsync(string returnUrl)
    {
        var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
        if (context?.IdP != null && await _schemeProvider.GetSchemeAsync(context.IdP) != null)
        {
            var local = context.IdP == Duende.IdentityServer.IdentityServerConstants.LocalIdentityProvider;

            // this is meant to short circuit the UI and only trigger the one external IdP
            View = new ViewModel
            {
                EnableLocalLogin = local,
            };



            if (!local)
            {
                View.ExternalProviders = new[] { new ViewModel.ExternalProvider { AuthenticationScheme = context.IdP } };
            }

            return;
        }

        if (Input == null)
        {
            Input = new InputModel();
            Input.TipoLogin = TipoLogin.EMAIL; // por padrão login com email
            Input.Email = context?.LoginHint;
        }

        Input.ReturnUrl = returnUrl;

        var schemes = await _schemeProvider.GetAllSchemesAsync();

        var providers = schemes
            .Where(x => x.DisplayName != null)
            .Select(x => new ViewModel.ExternalProvider
            {
                DisplayName = x.DisplayName ?? x.Name,
                AuthenticationScheme = x.Name
            }).ToList();

        var dyanmicSchemes = (await _identityProviderStore.GetAllSchemeNamesAsync())
            .Where(x => x.Enabled)
            .Select(x => new ViewModel.ExternalProvider
            {
                AuthenticationScheme = x.Scheme,
                DisplayName = x.DisplayName
            });
        providers.AddRange(dyanmicSchemes);


        var allowLocal = true;
        if (context?.Client.ClientId != null)
        {
            var client = await _clientStore.FindEnabledClientByIdAsync(context.Client.ClientId);
            if (client != null)
            {
                allowLocal = client.EnableLocalLogin;

                if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Any())
                {
                    providers = providers.Where(provider => client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme)).ToList();
                }
            }
        }

        View = new ViewModel
        {
            AllowRememberLogin = LoginOptions.AllowRememberLogin,
            EnableLocalLogin = allowLocal && LoginOptions.AllowLocalLogin,
            ExternalProviders = providers.ToArray()
        };
    }


    private async Task<ApplicationUser> ObterUsuarioPorTipoLogin(TipoLogin tipoLogin)
    {
        switch (tipoLogin)
        {
            case TipoLogin.EMAIL:
                return await _userManager.FindByNameAsync(Input.Email.Trim());
            case TipoLogin.CPF:
                return await _userManager.FindByCPF(Input.CPF.SoNumeros().Trim());
        }
        return null;
    }

    private async Task<bool> Validar(InputModel input)
    {
        ModelState.Clear();

        if (input.TipoLogin == TipoLogin.EMAIL)
        {
            if (String.IsNullOrWhiteSpace(input.Email))
                ModelState.AddModelError("Input.Email", "Informe o e-mail.");
            else
            {
                if (!Validador.Email.Valido(input.Email))
                    ModelState.AddModelError("Input.Email", "E-mail inválido.");
            }
        }

        if (input.TipoLogin == TipoLogin.CPF)
        {
            if (String.IsNullOrWhiteSpace(input.CPF))
                ModelState.AddModelError("Input.CPF", "Informe o CPF.");
            else
            {
                if (!Validador.CPF.Valido(input.CPF))
                    ModelState.AddModelError("Input.CPF", "CPF inválido.");
            }
        }

        if (String.IsNullOrWhiteSpace(Input.Password))
            ModelState.AddModelError("Input.Password", "Senha inválida.");

        // Se login ou senha não for informado, nem valido o resto.
        if (!ModelState.IsValid)
            return await Task.FromResult<bool>(false);

        ApplicationUser usuario = await ObterUsuarioPorTipoLogin(Input.TipoLogin);
        if (usuario == null || String.IsNullOrWhiteSpace(usuario.UserName))
        {
            // Faz o bind da mensagem de erro de acordo com o tipo de login.
            string inputParaBind = Input.TipoLogin == TipoLogin.CPF ? "Input.CPF" : "Input.Email";
            ModelState.AddModelError("", "Login Invalido.");
        }

        return ModelState.IsValid;
    }


}