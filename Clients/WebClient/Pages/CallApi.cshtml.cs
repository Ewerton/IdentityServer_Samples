using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;

namespace MyApp.Namespace
{

    //https://docs.duendesoftware.com/identityserver/v6/quickstarts/3_api_access/#using-the-access-token

    public class CallApiModel : PageModel
    {
        public string Json = string.Empty;

        public async Task OnGet()
        {
            // Obtem o access token da minha session
            var accessToken = await HttpContext.GetTokenAsync("access_token");

            // Declara um httpCLiente e coloca o token no header deste client
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // Faz uma chamada à API
            var content = await client.GetStringAsync("https://localhost:6001/identity"); // Chama um fnção do Projeto API 

            var parsed = JsonDocument.Parse(content);
            var formatted = JsonSerializer.Serialize(parsed, new JsonSerializerOptions { WriteIndented = true });

            Json = formatted;
        }
    }
}
