using IdentityServer.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using PrefeituraBrasil.IdentityServer;
using PrefeituraBrasil.IdentityServer.Extensions;
using PrefeituraBrasil.IdentityServer.Models.Enums;
using PrefeituraBrasil.IdentityServer.Service;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;

namespace IdentityServer.Pages.Account.Register;

[AllowAnonymous]
public class RegisterModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<RegisterModel> _logger;
    private readonly IEmailSender _emailSender;
    private readonly ValidadorComplexidadeSenha _passwordValidator;

    public RegisterModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<RegisterModel> logger,
        IEmailSender emailSender,
        ValidadorComplexidadeSenha passwordValidator)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
        _emailSender = emailSender;
        _passwordValidator = passwordValidator;
    }

    [BindProperty]
    public InputModel Input { get; set; }

    public string ReturnUrl { get; set; }

    public IList<AuthenticationScheme> ExternalLogins { get; set; }

    public class InputModel
    {
        //[Required]
        [DataType(DataType.Text)]
        [Display(Name = "Nome Completo:")]
        public string NomeCompleto { get; set; }

        //[Required]
        [DataType(DataType.Text)]
        [Display(Name = "CPF:")]
        public string CPF { get; set; }

        //[Required]
        [DataType(DataType.Text)]
        [Display(Name = "Celular:")]
        public string Celular { get; set; }

        //[Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        //[Required]
        //[StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmação da Senha")]
        //[Compare("Password", ErrorMessage = "A senha e a confirmação de senha são diferentes.")]
        public string ConfirmPassword { get; set; }

        //[Display(Name = "Confirmação da Senha")]
        //[Compare("Password", ErrorMessage = "A senha e a confirmação de senha são diferentes.")]
        public bool TermosECondicoesAceitos { get; set; }

        [Required(ErrorMessage = "Selecione como deseja fazer login")]
        public TipoConfirmacaoCadastro TipoConfirmacaoCadastro { get; set; }
    }


    public async Task OnGetAsync(string returnUrl = null)
    {
        await BuildModelAsync(returnUrl);

        ReturnUrl = returnUrl;
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
    }

    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        if (!await Validar(Input))
        {
            return Page();
        }
        else
        {
            var user = new ApplicationUser
            {
                UserName = Input.Email.Trim(),
                Email = Input.Email.Trim(),
                CPF = Input.CPF.SoNumeros(),
                UsuarioConfiguracoes = null,
                NomeCompleto = Input.NomeCompleto.Trim(),
            };

            var result = await _userManager.CreateAsync(user, Input.Password);
            if (result.Succeeded)
            {
                _logger.LogInformation("Usuário criou uma nova conta com senha.");

                if (Input.TipoConfirmacaoCadastro == TipoConfirmacaoCadastro.EMAIL)
                {
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/Register/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "", userId = user.Id, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    // Não dê await aqui. Não precisamos esperar o email ser enviado!
                    //Obs: caso queira utilizar o padrão Fire and Forget, descartar o retorno
                    _ = _emailSender.SendEmailAsync(Input.Email, "Confirme seu email para Prefeitura Brasil",
                          $"Você solicitou acesso ao sistema Prefeitura Brasil. <br/><br/> " +
                          $"Por favor, confirme sua solicitação <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicando aqui</a>.");

#if DEBUG
                    Console.WriteLine("### Link Para Confirmação de Cadastro: " + callbackUrl);
#endif

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                else
                {
                    ///TODO: Implementar futuramente
                    /// Gerar um código simples, de preferencia numerico e com uns 6 digitos.
                    /// Salvar em uma na tabela com a colunas (userID, codigo, data_expiraçao)
                    /// - userId: O Id do usuário que tentou se registrar
                    /// - codigo: um código simples de 6 digitos 
                    /// - data_expiraçao: uma data de expiração bem curta para aquele código (poucos minutos)
                    // Enviar o SMS para o usuário e redirecioná-lo para uma página onde dele terá XX segundos para
                    // inserir o código recebido por SMS. Nesta página validar se o código digitado esta correto para o usuário e se não esta expirado
                }
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        // If we got this far, something failed, redisplay form
        return Page();
    }

    private async Task<bool> Validar(InputModel input)
    {
        ModelState.Clear();

        if (String.IsNullOrWhiteSpace(input.NomeCompleto))
            ModelState.AddModelError("Input.NomeCompleto", "Informe o seu nome completo.");
        else
        {
            if (Input.NomeCompleto.Length < 3)
                ModelState.AddModelError("Input.NomeCompleto", "O seu nome deve ter mais de 3 caracteres.");
        }

        if (String.IsNullOrWhiteSpace(input.CPF))
            ModelState.AddModelError("Input.CPF", "Informe o CPF.");
        else
        {
            if (!Validador.CPF.Valido(input.CPF))
                ModelState.AddModelError("Input.CPF", "CPF inválido.");
        }

        if (String.IsNullOrWhiteSpace(input.Celular))
            ModelState.AddModelError("Input.Celular", "Informe o seu celular.");
        else
        {
            string celularSemMascara = Input.Celular.Replace("(", "")
                                                    .Replace(")", "")
                                                    .Replace("+", "")
                                                    .Replace("_", "")
                                                    .Replace("-", "");
            if (celularSemMascara.Length < 11)
                ModelState.AddModelError("Input.Celular", "Celular inválido.");
        }

        if (String.IsNullOrWhiteSpace(input.Email))
            ModelState.AddModelError("Input.Email", "Informe o e-mail.");
        else
        {
            if (!Validador.Email.Valido(input.Email))
                ModelState.AddModelError("Input.Email", "E-mail inválido.");
        }

        if (input.TermosECondicoesAceitos == false)
            ModelState.AddModelError("Input.TermosECondicoesAceitos", "Você precisa ler e aceitar os termos e condições.");


        if (String.IsNullOrWhiteSpace(Input.Password))
            ModelState.AddModelError("Input.Password", "Senha inválida.");
        else
        {
            if (Input.Password != Input.ConfirmPassword)
                ModelState.AddModelError("Input.ConfirmPassword", "Confirmação de senha inválida.");
            else
                await ValidaComplexidadeSenha();
        }

        if (input.TipoConfirmacaoCadastro == TipoConfirmacaoCadastro.SMS)
            ModelState.AddModelError("Input.TipoConfirmacaoCadastro", "A confirmação por SMS não está disponível.");

        return ModelState.IsValid;
    }

    private async Task ValidaComplexidadeSenha()
    {
        var result = await _passwordValidator.Validar(Input.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }
    }

    private async Task BuildModelAsync(string returnUrl)
    {
        if (Input == null)
        {
            Input = new InputModel();
            Input.TipoConfirmacaoCadastro = TipoConfirmacaoCadastro.EMAIL; // por padrão a confirmação será por email até implementarmos por SMS
        }

        this.ReturnUrl = returnUrl;
    }

}

