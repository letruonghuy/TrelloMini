using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JiraMini.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AntiforgeryController : ControllerBase
    {
        private readonly IAntiforgery _antiforgery;

        public AntiforgeryController(IAntiforgery antiforgery)
        {
            _antiforgery = antiforgery;
        }

        [HttpGet("token")]
        public IActionResult GetToken()
        {
            var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
            return Ok(new { token = tokens.RequestToken });
        }
    }
}
