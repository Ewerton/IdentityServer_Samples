using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using IdentityModel;
using IdentityServer.Data;
using IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Security.Claims;

namespace IdentityServer
{
    public static class DatabaseInitializer
    {
        // Esta função foi utilizad apenas para a primeira Migration. É recomendado que as demais (caso seja necessário) sejam feitas manualmente 
        public static void InitializeDatabase(IApplicationBuilder app)
        {
            InitializePersistedGrantDbContext(app);

            InitializeConfigurationDbContext(app);

            InitializeApplicationDbContext(app);

            SeedTestUsers(app); // Adiciona Bob e Alice para testes
        }

        private static void InitializePersistedGrantDbContext(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                Log.Debug("Migrations para PersistedGrantDbContext...");
                // Migration para os dados de Configuração do IdentityServer
                serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();
            }
        }

        private static void InitializeConfigurationDbContext(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                Log.Debug("Migrations para ConfigurationDbContext...");

                // Migration para os dados operacionais do IdentityServer
                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                context.Database.Migrate();

                foreach (var client in Config.Clients)
                {
                    if (!context.Clients.Any(c => c.ClientId == client.ClientId))
                    {
                        Log.Debug($"Adicionando client: {client.ClientId}");
                        context.Clients.Add(client.ToEntity());
                        context.SaveChanges();
                    }
                }

                foreach (var resource in Config.IdentityResources)
                {
                    if (!context.IdentityResources.Any(r => r.Name == resource.Name))
                    {
                        Log.Debug($"Adicionando resource: {resource.Name}");
                        context.IdentityResources.Add(resource.ToEntity());
                        context.SaveChanges();
                    }
                }

                foreach (var scope in Config.ApiScopes)
                {
                    if (!context.ApiScopes.Any(s => s.Name == scope.Name))
                    {
                        Log.Debug($"Adicionando scope: {scope.Name}");
                        context.ApiScopes.Add(scope.ToEntity());
                    }
                    context.SaveChanges();
                }
            }
        }

        private static void InitializeApplicationDbContext(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                Log.Debug("Migrations para ApplicationDbContext...");

                var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
                context.Database.Migrate();
            }
        }

        private static void SeedTestUsers(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                // Garante que as migrations para ApplicationDbContext foram rodadas
                var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
                context.Database.Migrate();

                var userMgr = serviceScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var alice = userMgr.FindByNameAsync("alice").Result;
                if (alice == null)
                {
                    alice = new ApplicationUser
                    {
                        UserName = "alice",
                        Email = "AliceSmith@email.com",
                        EmailConfirmed = true,
                        CPF= "41752105044",
                        NomeCompleto = "Alice Smith"
                    };
                    var result = userMgr.CreateAsync(alice, "Teste!23").Result;
                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }

                    result = userMgr.AddClaimsAsync(alice, new Claim[]{
                            new Claim(JwtClaimTypes.Name, "Alice Smith"),
                            new Claim(JwtClaimTypes.GivenName, "Alice"),
                            new Claim(JwtClaimTypes.FamilyName, "Smith"),
                            new Claim(JwtClaimTypes.WebSite, "http://alice.com"),
                        }).Result;
                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }
                    Log.Debug("Usuario Alice criado.");
                }
                else
                {
                    Log.Debug("Usuário Alice já existe");
                }

                var bob = userMgr.FindByNameAsync("bob").Result;
                if (bob == null)
                {
                    bob = new ApplicationUser
                    {
                        UserName = "bob",
                        Email = "BobSmith@email.com",
                        EmailConfirmed = true,
                        CPF = "43462486080",
                        NomeCompleto = "Bob Smith"
                    };
                    var result = userMgr.CreateAsync(bob, "Teste!23").Result;
                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }

                    result = userMgr.AddClaimsAsync(bob, new Claim[]{
                            new Claim(JwtClaimTypes.Name, "Bob Smith"),
                            new Claim(JwtClaimTypes.GivenName, "Bob"),
                            new Claim(JwtClaimTypes.FamilyName, "Smith"),
                            new Claim(JwtClaimTypes.WebSite, "http://bob.com"),
                            new Claim("location", "somewhere")
                        }).Result;
                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }
                    Log.Debug("Usuário Bob criado");
                }
                else
                {
                    Log.Debug("Usuário Bob já existe");
                }
            }
        }

    }


}

