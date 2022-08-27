// https://docs.duendesoftware.com/identityserver/v6/quickstarts/js_clients/js_with_backend/

using Duende.Bff.Yarp;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthorization();

// Para poder chamar meus controllers locais
builder.Services.AddControllers();
//builder.Services.AddControllersWithViews();

builder.Services
    .AddBff()
    .AddRemoteApis();

JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = "Cookies";
        options.DefaultChallengeScheme = "oidc";
        options.DefaultSignOutScheme = "oidc";
    })
    .AddCookie("Cookies")
    .AddOpenIdConnect("oidc", options =>
    {
        options.Authority = "https://localhost:5001";
        options.ClientId = "bff";
        options.ClientSecret = "secret";
        options.ResponseType = "code";
        options.Scope.Add("PrefeituraBrasilAPI");
        options.SaveTokens = true;
        options.GetClaimsFromUserInfoEndpoint = true;
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();

app.UseBff();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapBffManagementEndpoints();

    // Uncomment this for Controller support
    // Mapeia as requisições para a API Local serem autenticadas usando o BFF do IdentityServer
    //https://docs.duendesoftware.com/identityserver/v6/quickstarts/js_clients/js_with_backend/#add-api-support
    endpoints.MapControllers()
        .RequireAuthorization()
        .AsBffApiEndpoint();

    //endpoints.MapGet("/LocalAPI/Identity", LocalIdentityHandler)
    //    .RequireAuthorization()
    //    .AsBffApiEndpoint();

    // Mapeia a api remota (do projeto \Clients Teste\PrefeituraBrasil.IdentityServer.Api) para ser chamada em /remote/ anexando o token de autenticação à request
    endpoints.MapRemoteBffApiEndpoint("/remote", "https://localhost:6001")
        .RequireAccessToken(Duende.Bff.TokenType.User);


});

// Adicionado para mapear rotas para os controlers.
app.MapControllers();

//app.MapGet("/", () => "Hello World!");

app.Run();

// [Authorize]
// static IResult LocalIdentityHandler(ClaimsPrincipal user, HttpContext context)
// {
//    var name = user.FindFirst("name")?.Value ?? user.FindFirst("sub")?.Value;
//    return Results.Json(new { message = "Local API Success!", user = name });
// }