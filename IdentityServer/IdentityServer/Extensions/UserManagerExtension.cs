using IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace PrefeituraBrasil.IdentityServer.Extensions
{
    public static class UserManagerExtension
    {
        public static Task<ApplicationUser> FindByCPF(this UserManager<ApplicationUser> userManager, string cpf)
        {
            if (!String.IsNullOrWhiteSpace(cpf))
                return userManager?.Users?.FirstOrDefaultAsync(um => um.CPF == cpf);
            else
                return null;
        }

        public static Task<ApplicationUser> FindByEmail(this UserManager<ApplicationUser> userManager, string email)
        {
            if (!String.IsNullOrWhiteSpace(email))
                return userManager?.Users?.FirstOrDefaultAsync(um => um.Email == email);
            else
                return null;
        }
    }
}
