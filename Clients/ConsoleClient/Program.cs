// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.


using IdentityModel.Client;
using System.Text.Json;


var client = new HttpClient();

Thread.Sleep(7000);

// discover endpoints from Servidor de Identidade metadata
var disco = await client.GetDiscoveryDocumentAsync("https://localhost:5001");
if (disco.IsError)
{
    Console.WriteLine(disco.Error);
    Console.WriteLine("Press any key to continue");
    Console.ReadKey();
}

// Pede um token para o Servidor de Identidade
var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
{
    Address = disco.TokenEndpoint,
    ClientId = "client",
    ClientSecret = "secret",

    Scope = "PrefeituraBrasilAPI" // A API scope que defini lá no Servidor de Identidade
});

if (tokenResponse.IsError)
{
    Console.WriteLine(tokenResponse.Error);
    Console.WriteLine(tokenResponse.ErrorDescription);
    return;
}

Console.WriteLine(tokenResponse.Json);
Console.WriteLine("\n\n");

// Chama uma função do projeto teste\Api setando o token obtido do servidor de Identidade no header da requisição
var apiClient = new HttpClient();
apiClient.SetBearerToken(tokenResponse.AccessToken);

var response = await apiClient.GetAsync("https://localhost:6001/identity"); // chama um endereço da minha API protegida (\PrefeituraBrasil.IdentityServer\Api_Test\Api\Controllers\IdentityController.cs)
if (!response.IsSuccessStatusCode)
{
    Console.WriteLine(response.StatusCode);
}
else
{
    var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
    Console.WriteLine(JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true }));
}

Console.WriteLine("Press any key to continue");
Console.ReadKey();