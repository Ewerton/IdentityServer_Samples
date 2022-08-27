// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.


using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdentityServer.Models;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    [Column(TypeName = "jsonb")]
    public UsuarioConfiguracao[] UsuarioConfiguracoes { get; set; }
    public string CPF { get; set; }

    public string NomeCompleto { get; set; }
}
