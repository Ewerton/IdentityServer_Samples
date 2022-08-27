using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MyApp.Namespace
{
    public class SignoutModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Limapa o cookie local e redirect to the IdentityServer.
            // O IdentityServer vai limpar seus cookies e redirecionar o usuário para um link que o devolve para a aplicação, desta vez deslogado.
            return SignOut("Cookies", "oidc");
        }
    }
}
