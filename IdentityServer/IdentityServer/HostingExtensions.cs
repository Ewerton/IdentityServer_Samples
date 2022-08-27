using Duende.IdentityServer;
using Duende.IdentityServer.Services;
using IdentityServer.Data;
using IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PrefeituraBrasil.IdentityServer;
using PrefeituraBrasil.IdentityServer.Service;
using PrefeituraBrasil.MailSender;
using PrefeituraBrasil.MailSender.Interfaces;
using Serilog;
using System.Reflection;

namespace IdentityServer;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddRazorPages();

        var migrationsAssembly = typeof(Program).GetTypeInfo().Assembly.GetName().Name;
        string connectionString = builder.Configuration.GetConnectionString("IdentityServer");

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            // Password settings.
            options.Password.RequiredLength = 8;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireDigit = true;
            options.Password.RequireNonAlphanumeric = true;

            // SignIn settings.
            options.SignIn.RequireConfirmedAccount = true;

            // User settings.
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders()
        .AddErrorDescriber<LocalizedIdentityErrorDescriber>(); // Para emitir as mensagens de erro em pt-BR (mensagens como "A senha deve ter x caracteres")

        builder.Services.AddIdentityServer(options =>
        {
            options.Events.RaiseErrorEvents = true;
            options.Events.RaiseInformationEvents = true;
            options.Events.RaiseFailureEvents = true;
            options.Events.RaiseSuccessEvents = true;

            // see https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/
            options.EmitStaticAudienceClaim = true;

            // Tempo em que a sessão do usuário ficará válida
            options.Authentication.CookieLifetime = TimeSpan.FromMinutes(10);
            options.Authentication.CookieSlidingExpiration = false;
        })

        // ConfigurationDbContext: Usado para armazenar configurações como clients, resources e scopes
        .AddConfigurationStore(options =>
        {
            options.ConfigureDbContext = b => b.UseNpgsql(connectionString,
                sql => sql.MigrationsAssembly(migrationsAssembly)); // Informa que os migrations vão ficar neste assembly
        })

        // PersistedGrantDbContext: Usado para armazenar dados operacionais dinamicos como authorization codes e refresh tokens
        .AddOperationalStore(options =>
        {
            options.ConfigureDbContext = b => b.UseNpgsql(connectionString,
                sql => sql.MigrationsAssembly(migrationsAssembly)); // Informa que os migrations vão ficar neste assembly
        })

        // Usado com o Config.cs para quando usamos "Config as a Code"
        //.AddInMemoryIdentityResources(Config.IdentityResources)
        //.AddInMemoryApiScopes(Config.ApiScopes)
        //.AddInMemoryClients(Config.Clients)
        // .AddTestUsers(TestUsers.Users) // Adiciona usuários apenas para teste (TestUsers.cs) 
        .AddAspNetIdentity<ApplicationUser>() // ApplicationUser customizado com CPF e NomeCompleto
        .AddProfileService<CustomProfileService>();  //CustomProfileService permite enviar dados customizados como CPF e NomeComleto nas Claims

        builder.Services.AddAuthentication();
        //.AddIdentityServerJwt(); // ?

        builder.Services.AddAuthentication();
            // Para adicionar login com o google https://docs.duendesoftware.com/identityserver/v6/quickstarts/2_interactive/#add-google-support
            //.AddGoogle(options =>
            //{
            //    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

            //    // register your IdentityServer with Google at https://console.developers.google.com
            //    // enable the Google+ API
            //    // set the redirect URI to https://localhost:5001/signin-google
            //    options.ClientId = "copy client ID from Google here";
            //    options.ClientSecret = "copy client secret from Google here";
            //});

            // Pra adicionar o Servidor de Identidade de Demo da Duende Software como External Provider    
            //.AddOpenIdConnect("oidc", "Demo IdentityServer", options =>
            //{
            //    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
            //    options.SignOutScheme = IdentityServerConstants.SignoutScheme;
            //    options.SaveTokens = true;

            //    options.Authority = "https://demo.duendesoftware.com";
            //    options.ClientId = "interactive.confidential";
            //    options.ClientSecret = "secret";
            //    options.ResponseType = "code";

            //    options.TokenValidationParameters = new TokenValidationParameters
            //    {
            //        NameClaimType = "name",
            //        RoleClaimType = "role"
            //    };
            //});


        // Não habilite o CORS aqui. Isso deve ser habilitado para cada client (veja Config.CS "AllowedCorsOrigins") 
        //builder.Services.AddCors();

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        // Não habilite o CORS aqui. Isso deve ser habilitado para cada client (veja Config.CS "AllowedCorsOrigins") 
        //app.UseCors(corsPolicyBuilder => corsPolicyBuilder
        //    .AllowAnyOrigin()
        //    .AllowAnyMethod()
        //    .AllowAnyHeader());


        //app.UseHttpsRedirection(); // ?
        app.UseStaticFiles();
        app.UseRouting();

        app.UseIdentityServer();
        app.UseAuthentication(); // 
        app.UseAuthorization();

        // http://docs.nwebsec.com/en/latest/nwebsec/Configuring-xfo.html
        // Não permite que este site seja carregado em um iFrame para proteger de atraques de Clickjacking
        app.UseXfo(options => options.SameOrigin());


        //https://www.hanselman.com/blog/net-6-hot-reload-and-refused-to-connect-to-ws-because-it-violates-the-content-security-policy-directive-because-web-sockets
        // Permite que a página faça chamadas wss (secure webservice) para o servidor. Este é o mecanismo usado pelo Visual Studio para fazer HotReload, portanto, sem isso o Hotreload não funciona
        if (app.Environment.IsDevelopment())
        {
            app.UseCsp(options => options
                .DefaultSources(s => s.Self()
                    .CustomSources("https://fonts.googleapis.com", "https://fonts.gstatic.com")) // Habilita o uso de Google Fonts
                .ImageSources(s => s.Self()
                    .CustomSources("data:")) // Habilita uso de imagem encodadas em base64
                .ConnectSources(s => s.CustomSources("wss:")) // Habilita execução do script js para HotReload pelo VS
                .FontSources(s => s.Self()
                    .CustomSources("https://fonts.googleapis.com", "https://fonts.gstatic.com"))); // Habilita o uso de Google Fonts
        }
        else
        {
            app.UseCsp(options => options
                 .DefaultSources(s => s.Self()
                     .CustomSources("https://fonts.googleapis.com", "https://fonts.gstatic.com")) // Habilita o uso de Google Fonts
                 .ImageSources(s => s.Self()
                     .CustomSources("data:")) // Habilita uso de imagem encodadas em base64
                 .FontSources(s => s.Self()
                     .CustomSources("https://fonts.googleapis.com", "https://fonts.gstatic.com"))); // Habilita o uso de Google Fonts
        }

        app.MapRazorPages()
             .RequireAuthorization();

        return app;
    }

    public static IServiceCollection RegisterDependencies(this IServiceCollection services, IConfiguration config)
    {
        SmtpConfiguration smtpConf = config.GetSection("SmtpConfiguration").Get<SmtpConfiguration>();
        services.AddSingleton<IEmailSmtpConfig>(smtpConf);
        //builder.Services.AddTransient<IEnviadorDeEmail, EmailSender>();
        services.AddTransient<IEmailSender, EmailSender>(); // O enviador de email usado pelo Identity Server
        services.AddTransient<ValidadorComplexidadeSenha>();
        return services;
    }


}