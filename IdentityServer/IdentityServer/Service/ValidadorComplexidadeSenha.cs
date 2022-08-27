using IdentityServer.Models;
using Microsoft.AspNetCore.Identity;

namespace PrefeituraBrasil.IdentityServer.Service
{
    public class ValidadorComplexidadeSenha
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ValidadorComplexidadeSenha(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }


        public async Task<IdentityResult> Validar(string senha)
        {
            IdentityResult result = new IdentityResult();
            foreach (var validator in _userManager.PasswordValidators)
            {
               result = await validator.ValidateAsync(_userManager, null, senha);

                // Para exibir os resultados na tela!
                //if (!result.Succeeded)
                //{
                //    foreach (var error in result.Errors)
                //    {
                //        ModelState.AddModelError("", error.Description);
                //    }
                //}
            }

            return result;
        }
    }
}
