// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using IdentityServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using PrefeituraBrasil.IdentityServer;
using PrefeituraBrasil.IdentityServer.Extensions;
using PrefeituraBrasil.IdentityServer.Models.Enums;
using PrefeituraBrasil.IdentityServer.Service;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IdentityServer.Pages.Account.Password
{
    [AllowAnonymous]
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ValidadorComplexidadeSenha _passwordValidator;

        public ResetPasswordModel(UserManager<ApplicationUser> userManager, ValidadorComplexidadeSenha passwordValidator)
        {
            _userManager = userManager;
            _passwordValidator = passwordValidator;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Informe o email")]
            [EmailAddress(ErrorMessage = "O email digitado não é válido.")]
            public string Email { get; set; }


            [Required(ErrorMessage = "Informe a senha")]
            //[StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Nova senha")]
            public string Password { get; set; }


            [DataType(DataType.Password)]
            [Display(Name = "Confirmação de nova senha")]
            [Compare("Password", ErrorMessage = "A senha e a confirmação não são iguais.")]
            public string ConfirmPassword { get; set; }

            [Required]
            public string Code { get; set; }

            [Required(ErrorMessage = "Selecione como deseja resetar sua senha.")]
            public TipoLogin TipoLogin { get; set; }

            [DataType(DataType.Text)]
            public string CPF { get; set; }
        }

        public IActionResult OnGet(string code = null)
        {
            if (code == null)
            {
                return BadRequest("O código para recuperação de senha é inválido.");
            }
            else
            {
                Input = new InputModel
                {
                    Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code))
                };
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            if (String.IsNullOrWhiteSpace(returnUrl))
                returnUrl = Url.Content("~/");

            if (!await Validar(Input))
            {
                return Page();
            }
            else
            {
                var user = await ObterUsuarioPorTipoLogin(Input.TipoLogin);
                if (user == null)
                {
                    // Don't reveal that the user does not exist
                    return RedirectToPage("./ResetPasswordConfirmation");
                }

                var result = await _userManager.ResetPasswordAsync(user, Input.Code, Input.Password);
                if (result.Succeeded)
                {
                    // Resetou a senha. Passou "pra frente", toda a returnURL (caso alguma tenha sido recebida por parametro), desta forma, o usuário
                    // pode ser "devolvido" para a aplicação cliente de onde ele começou o processo de reset de senha, seja esta aplicação o cliente Angular ou qualquer outro que venha a ser desenvolvido no futuro.
                    return RedirectToPage("/Account/Password/ResetPasswordConfirmation", new { returnURL = returnUrl });
                }
                else
                {

                    // Se chegou até aqui é porque o redirect não ocorreu e algo deu errado.
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }

                    return Page();
                }
            }
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

            // Caso o usuário não exista, não avise o usuário para que ele não fique "tentando acertar" um email que existe na nossa base de dados.
            //var usuario = await _userManager.FindByNameAsync(Input.Email.Trim());
            //if (usuario == null || String.IsNullOrWhiteSpace(usuario.UserName))
            //{
            //}

            if (String.IsNullOrWhiteSpace(Input.Password))
                ModelState.AddModelError("Input.Password", "Senha inválida.");
            else
            {
                if (Input.Password != Input.ConfirmPassword)
                    ModelState.AddModelError("Input.ConfirmPassword", "Confirmação de senha inválida.");
                else
                {
                    // Validando a complexidade da senha
                    var result = await _passwordValidator.Validar(Input.Password);
                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                    }
                }
            }

            return ModelState.IsValid;
        }
    }
}
