using System;
using System.Threading.Tasks;
using Duende.IdentityServer;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServer.Pages.Account.Logout;

[SecurityHeaders]
[AllowAnonymous]
public class LoggedOut : PageModel
{
    private readonly IIdentityServerInteractionService _interactionService;

    public LoggedOutViewModel View { get; set; }

    public LoggedOut(IIdentityServerInteractionService interactionService)
    {
        _interactionService = interactionService;
    }

    public async Task OnGet(string logoutId)
    {
        // get context information (client name, post logout redirect URI and iframe for federated signout)
        var logout = await _interactionService.GetLogoutContextAsync(logoutId);
                
        // Por padrão redireciona para Login
        string postLogoutURL = "/Account/Login/Index";
        if (!String.IsNullOrWhiteSpace(logout?.PostLogoutRedirectUri))
        {
            postLogoutURL = logout?.PostLogoutRedirectUri; // Se tem uma URL de retorno (para uma aplicação client, por exemplo) redireciona para ela
        }

        View = new LoggedOutViewModel
        {
            AutomaticRedirectAfterSignOut = LogoutOptions.AutomaticRedirectAfterSignOut,
            ShowAutoRedirectTimer = LogoutOptions.ShowAutoRedirectTimer,
            PostLogoutRedirectUri = postLogoutURL,
            ClientName = String.IsNullOrEmpty(logout?.ClientName) ? logout?.ClientId : logout?.ClientName,
            SignOutIframeUrl = logout?.SignOutIFrameUrl
        };
}
}