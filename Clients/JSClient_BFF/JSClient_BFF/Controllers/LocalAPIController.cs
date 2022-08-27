using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;


//https://docs.duendesoftware.com/identityserver/v6/quickstarts/js_clients/js_with_backend/#define-a-local-api 

namespace JSClient_BFF.Controllers
{
    [Route("[controller]/[action]")]
    [Authorize]
    public class LocalAPIController : ControllerBase
    {
        [HttpGet]
        public IResult LocalIdentityHandler()
        {
            var claimsPrincipal = HttpContext.User;
            var accessToken = HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken).Result;

            var name = HttpContext.User.FindFirst("name")?.Value ?? HttpContext.User.FindFirst("sub")?.Value;
            return Results.Json(new { message = "Local API Success!", user = name, accessToken = accessToken });
        }
    }
}
