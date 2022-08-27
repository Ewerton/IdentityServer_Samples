using IdentityServer;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up");

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, lc) => lc
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
        .Enrich.FromLogContext()
        .ReadFrom.Configuration(ctx.Configuration));

    // Registra as Dependencias no DI Container
    builder.Services.RegisterDependencies(builder.Configuration);

    var app = builder
        .ConfigureServices()
        .ConfigurePipeline();


    // if (app.Environment.IsDevelopment())
    // {
    // Uma classe que agrupa todas as tarefas de migration, seeding, etc.
    // Em PROD, talvez seja melhor usar outra estratégia de migration.
    DatabaseInitializer.InitializeDatabase(app);
    // }

    app.Run();
}
catch (Exception ex) when (ex.GetType().Name is not "StopTheHostException") // https://github.com/dotnet/runtime/issues/60600
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}