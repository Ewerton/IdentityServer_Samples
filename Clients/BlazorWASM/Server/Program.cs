using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.

    //builder.Services.AddControllers();
    builder.Services.AddControllersWithViews();
    builder.Services.AddRazorPages();

    builder.Services.AddBff();

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = "cookie";
        options.DefaultChallengeScheme = "oidc";
        options.DefaultSignOutScheme = "oidc";
    })
        .AddCookie("cookie", options =>
        {
            options.Cookie.Name = "__Host-blazor";
            options.Cookie.SameSite = SameSiteMode.Strict;
        })
        .AddOpenIdConnect("oidc", options =>
        {
            //options.Authority = "https://demo.duendesoftware.com";
            options.Authority = "https://localhost:5001"; // Endereço no meu Servidor de Identidade

            //options.ClientId = "interactive.confidential";
            options.ClientId = "blazor_wasm";
            options.ClientSecret = "secret";
            options.ResponseType = "code";
            options.ResponseMode = "query";

            options.Scope.Clear();
            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.Scope.Add("PrefeituraBrasilAPI");
            options.Scope.Add("offline_access");

            options.MapInboundClaims = false;
            options.GetClaimsFromUserInfoEndpoint = true;
            options.SaveTokens = true;
        });


    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseWebAssemblyDebugging();
    }
    else
    {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHttpsRedirection();

    app.UseBlazorFrameworkFiles();
    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthentication();
    app.UseBff();
    app.UseAuthorization();

    app.MapBffManagementEndpoints();
    app.MapRazorPages();

    app.MapControllers()
        .RequireAuthorization()
        .AsBffApiEndpoint();

    app.MapFallbackToFile("index.html");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}