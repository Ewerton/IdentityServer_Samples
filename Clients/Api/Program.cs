using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = "https://localhost:5001"; // O servidor de Identity do projeto IdentityServer

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false
        };
    });

//https://docs.duendesoftware.com/identityserver/v6/quickstarts/1_client_credentials/#authorization-at-the-api
// Cria uma policy que obriga que os tokens recebidos pela API sejam de usuários autenticados e que tenham o escopo "PrefeituraBrasilAPI", ou seja, 
// tokens que foram expedidos para este propósito específico (acessar a API)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Requer_PrefeituraBrasilAPI", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "PrefeituraBrasilAPI");
    });
});

// https://docs.duendesoftware.com/identityserver/v6/quickstarts/js_clients/js_without_backend/#allowing-ajax-calls-to-the-web-api-with-cors
// permite CORS de https://localhost:5003 para https://localhost:6001.
builder.Services.AddCors(options =>
{
    // this defines a CORS policy called "default"
    options.AddPolicy("default", policy =>
    {
        policy.WithOrigins("https://localhost:5003")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// https://docs.duendesoftware.com/identityserver/v6/quickstarts/js_clients/js_without_backend/#allowing-ajax-calls-to-the-web-api-with-cors
// permite CORS de https://localhost:5003 para https://localhost:6001.
app.UseCors("default");

app.UseAuthentication(); // Adiciona autenticação
app.UseAuthorization();

//https://docs.duendesoftware.com/identityserver/v6/quickstarts/1_client_credentials/#authorization-at-the-api
//app.MapControllers();
app.MapControllers().RequireAuthorization("Requer_PrefeituraBrasilAPI"); // Protege todos os controles com as Policies criadas na linha 21, note que é possível aplicar estas policies à somente alguns endpoints, caso preciso possível 


app.Run();
