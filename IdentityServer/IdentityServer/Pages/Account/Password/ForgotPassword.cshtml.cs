// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using IdentityServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using PrefeituraBrasil.IdentityServer;
using PrefeituraBrasil.IdentityServer.Extensions;
using PrefeituraBrasil.IdentityServer.Models.Enums;

namespace IdentityServer.Pages.Account.Password
{
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public ForgotPasswordModel(UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Informe o email")]
            [EmailAddress(ErrorMessage = "O email digitado não é válido.")]
            public string Email { get; set; }

            [DataType(DataType.Text)]
            public string CPF { get; set; }

            [Required(ErrorMessage = "Selecione como deseja recuperar sua senha.")]
            public TipoLogin TipoLogin { get; set; }
        }

        public IActionResult OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl;

            BuildModel();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            ReturnUrl ??= Url.Content("~/");

            if (!await Validar(Input))
            {
                BuildModel();
                return Page();
            }
            else
            {
                //var user = await _userManager.FindByEmailAsync(Input.Email);
                ApplicationUser user = await ObterUsuarioPorTipoLogin(Input.TipoLogin);

                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage("/Account/Password/ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/Password/ResetPassword",
                    pageHandler: null,
                    values: new { area = "", code, returnUrl = returnUrl },
                    protocol: Request.Scheme);

                // Não dê await aqui. Não precisamos esperar o email ser enviado!
                //Obs: caso queira utilizar o padrão Fire and Forget, descartar o retorno
                _ = _emailSender.SendEmailAsync(
                    user.Email,
                    "Recuperação de senha do SPB",
                    $"Por favor, resete sua senha <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicando aqui.</a>.");

                return RedirectToPage("/Account/Password/ForgotPasswordConfirmation");
            }
        }

        private void BuildModel()
        {
            this.Input = new InputModel()
            {
                TipoLogin = TipoLogin.EMAIL,
                CPF = "",
                Email = ""
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

            // Se login ou senha não for informado, nem valido o resto.
            if (!ModelState.IsValid)
                return await Task.FromResult<bool>(false);

            return ModelState.IsValid;
        }
    }
}
