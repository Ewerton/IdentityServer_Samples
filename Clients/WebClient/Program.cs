using Microsoft.AspNetCore.Authentication;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();


JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

//https://docs.duendesoftware.com/identityserver/v6/quickstarts/2_interactive/#configure-authentication-services
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = "oidc";
})
    .AddCookie("Cookies")
    .AddOpenIdConnect("oidc", options =>
    {
        options.Authority = "https://localhost:5001"; // Endereço no meu Servidor de Identidade

        options.ClientId = "web";
        options.ClientSecret = "secret";
        options.ResponseType = "code"; // uses the authorization code flow with PKCE to connect to the OpenID Connect provider

        options.SaveTokens = true; // Faz com que o ASP.NET Core salve automaticamente o id, access e refresh tokens nas propriedades do cookie de autenticação.

        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");
        options.Scope.Add("PrefeituraBrasilAPI"); // Habilita este cliente à requisitar acesso à API da Prefeitura Brasil
        options.Scope.Add("offline_access"); // Habilita este cliente a usar os refresh tokens.
        options.Scope.Add("verification"); // gera uma claim para informar se o email foi confirmado
        options.ClaimActions.MapJsonKey("email_verified", "email_verified");

        // MinhaClaimCustomizada não aparece no WebClient, porque?
        //options.Scope.Add("MinhaClaimCustomizada");
        //options.ClaimActions.MapJsonKey("picture", "MinhaClaimCustomizada", "pictureValue");

        //https://docs.duendesoftware.com/identityserver/v6/quickstarts/2_interactive/#getting-claims-from-the-userinfo-endpoint
        options.GetClaimsFromUserInfoEndpoint = true;
    });


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication(); // added
app.UseAuthorization();

//app.MapRazorPages();
app.MapRazorPages().RequireAuthorization(); // added 

app.Run();
