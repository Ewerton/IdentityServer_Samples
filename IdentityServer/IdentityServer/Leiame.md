# Considerações sobre DbContexts e Migrations

O Identity Server utiliza dois DbContexts internos 
- ConfigurationDbContext: Utilizado para armazenar dados de configuração do Identity Server, como por exemplo, quais clientes, escopos, e resources estão criados
- PersistedGrantDbContext: Utilizado para dados dinamicos durante a utilização, por exemplo, quais usuários estão logados, quais os tokens e refresh tokens expedidos, etc

Neste mesmo projeto nós ainda utilizamos um terceiro DbContext que é o ApplicationDbContext utilizado pelo Identity do Asp.Net Core (https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-6.0&tabs=visual-studio)

Dificilmente você precisará fazer migrations em ConfigurationDbContext e PersistedGrantDbContext, mas se precisar, execute a seguinte linha de comando dentro da pasta \PrefeituraBrasil.IdentityServer\IdentityServer\IdentityServer :

- Para o PersistedGrantDbContext: dotnet ef migrations add InitialIdentityServerPersistedGrantDbMigration -c PersistedGrantDbContext -o Data/Migrations/IdentityServer/PersistedGrantDb
- Para o ConfigurationDbContext: dotnet ef migrations add InitialIdentityServerConfigurationDbMigration -c ConfigurationDbContext -o Data/Migrations/IdentityServer/ConfigurationDb
- Para o ApplicationDbContext: dotnet ef migrations add AddUsers -c ApplicationDbContext -o Data/Migrations


Mais detalhes em https://docs.duendesoftware.com/identityserver/v6/quickstarts/4_ef/