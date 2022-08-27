// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.


using PrefeituraBrasil.IdentityServer.Models.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Pages.Account.Login;

public class InputModel
{
    //[Required]
    public string Username { get; set; }

    //[EmailAddress(ErrorMessage = "E-mail inválido.")]
    [DataType(DataType.Text)]
    public string Email { get; set; }

    [DataType(DataType.Text)]
    public string CPF { get; set; }

    //[Required]
    [Display(Name = "Senha")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Display(Name = "Manter conectado")]
    public bool RememberLogin { get; set; }

    public string ReturnUrl { get; set; }

    public string Button { get; set; }

    [Required(ErrorMessage = "Selecione como deseja fazer login")]
    public TipoLogin TipoLogin { get; set; }
}

