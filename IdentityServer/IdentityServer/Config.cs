using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using IdentityModel;

namespace IdentityServer;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            // https://docs.duendesoftware.com/identityserver/v6/quickstarts/2_interactive/#configure-oidc-scopes
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email(),
            new IdentityResource()
            {
                Name = "verification",
                UserClaims = new List<string>
                {
                    JwtClaimTypes.Email,
                    JwtClaimTypes.EmailVerified,
                }
            },
            // Nao aparece no WebCLient, porque?
            new IdentityResource()
            {
                Name = "CPF",
                UserClaims = new List<string>
                {
                    "CPF"
                }
            }
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
            {
                new ApiScope(name: "PrefeituraBrasilAPI", displayName: "PrefeituraBrasil.API") // Full access, apenas para testar
            };

    public static IEnumerable<Client> Clients =>
        new Client[]
        {
            // Utilizada pela aplicação de teste \Clients Teste\ConsoleClient (machine to machine client (from quickstart 1))
            new Client
            {
                ClientId = "client", // O cliente (app) autorizado a fazer login
                    
                // no interactive user, use the clientid/secret for authentication
                AllowedGrantTypes = GrantTypes.ClientCredentials,

                // secret for authentication, uma especie de senha para o client
                ClientSecrets = { new Secret("secret".Sha256()) },

                // scopes that client has access to
                AllowedScopes = { "PrefeituraBrasilAPI" }
            },

            // Utilizada pela aplicação de teste \Clients Teste\WebCLient (Interactive ASP.NET Core Web App)
            new Client
            {
                ClientId = "web",
                ClientSecrets = { new Secret("secret".Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,
            
                // where to redirect to after login
                RedirectUris = { "https://localhost:5002/signin-oidc" }, // Endereço do meu IdentityServer

                // where to redirect to after logout
                PostLogoutRedirectUris = { "https://localhost:5002/signout-callback-oidc" }, // Endereço do meu IdentityServer

                // Enable support for refresh tokens 
                AllowOfflineAccess = true,

                AllowedScopes = new List<string>
                {
                    IdentityServerConstants.StandardScopes.OpenId, // Permite que este cliente requisite se autentique usando um fluxo OIDC (pkce , por exemplo)
                    IdentityServerConstants.StandardScopes.Profile, // Permite que este cliente requisite o profile
                    IdentityServerConstants.StandardScopes.Email, // Permite que este cliente requisite o email,
                    "PrefeituraBrasilAPI", // Além do identity token (para login), habilita o access token (para acesso à API do SPB)
                    "verification", // Uma claim adicional para dizer se o usuário já confirmou o email.
                    "MinhaClaimCustomizada" // uma claim qualquer inventada (não funcionou!)
                }
            },

            // Utilizado pela aplicação de \Clients Teste\JSClient_BFF (Javascript com Backenf for Frontend)
            new Client
            {
                ClientId = "bff",
                ClientSecrets = { new Secret("secret".Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,
    
                // where to redirect to after login
                RedirectUris = { "https://localhost:5003/signin-oidc" },

                // where to redirect to after logout
                PostLogoutRedirectUris = { "https://localhost:5003/signout-callback-oidc" },

                AllowedScopes = new List<string>
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    "PrefeituraBrasilAPI"
                }
            },

            // Utilizado pela aplicação de \Clients Teste\JSClient (Javascript puro, sem backend)
            new Client
            {
                ClientId = "js",
                ClientName = "JavaScript Client",
                RequireClientSecret = false,
                //ClientSecrets = { new Secret("secret".Sha256()) },
                AllowedGrantTypes = GrantTypes.Code,
    
                // where to redirect to after login
                RedirectUris =           { "https://localhost:5003/callback.html" },
                PostLogoutRedirectUris = { "https://localhost:5003/index.html" },
                AllowedCorsOrigins =     { "https://localhost:5003" },

                AllowedScopes = new List<string>
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    "PrefeituraBrasilAPI"
                }
            },

            // Utilizado pela aplicação de \Clients Teste\BlaworWASM (Javascript puro, sem backend)
            new Client
            {
                ClientId = "blazor_wasm",
                ClientName = "Brazor Web Assembly Client",
                //RequireClientSecret = false,
                ClientSecrets = { new Secret("secret".Sha256()) },
                AllowedGrantTypes = GrantTypes.Code,
    
                // where to redirect to after login
                RedirectUris = { "https://localhost:5003/signin-oidc" },

                // where to redirect to after logout
                PostLogoutRedirectUris = { "https://localhost:5003/signout-callback-oidc" },

                 // Enable support for refresh tokens 
                AllowOfflineAccess = true,

                AllowedScopes = new List<string>
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Email,
                    "PrefeituraBrasilAPI"
                }
            },

            // Cliente do SPB Angular
            new Client
            {
                ClientId = "PrefeituraBrasilSPAClient",
                ClientName = "Prefeitura Brasil Angular SPA",
                RequireClientSecret = false,
                AllowedGrantTypes = GrantTypes.Code,

                // Enable support for refresh tokens 
                AllowOfflineAccess = true,

                // where to redirect to after login
                RedirectUris = new List<string>
                {
                    "https://localhost:44400/login-callback",
                },

                PostLogoutRedirectUris = new List<string>
                {
                    "https://localhost:44400/logout-callback",
                },

                AllowedCorsOrigins = new List<string>
                {
                    "https://localhost:44400",
                },

                AllowedScopes = new List<string>
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Email,
                    IdentityServerConstants.StandardScopes.OfflineAccess,
                    "PrefeituraBrasilAPI", "CPF"
                }
            },
        };

}
