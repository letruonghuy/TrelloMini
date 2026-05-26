using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using JiraMini.Data;
using JiraMini.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace JiraMini.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AppDbContext context, ILogger<AuthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("google-login")]
        public IActionResult GoogleLogin(string returnUrl = "/dashboard.html")
        {
            _logger.LogInformation("Google login initiated with returnUrl: {returnUrl}", returnUrl);
            
            var redirectUrl = Url.Action("GoogleResponse", "Auth", new { returnUrl = returnUrl }, Request.Scheme);
            _logger.LogInformation("Google login redirect URL: {redirectUrl}", redirectUrl);

            var properties = new AuthenticationProperties
            {
                RedirectUri = redirectUrl
            };

            return Challenge(properties, "Google");
        }

        // Handle the Google OAuth callback - this is called by ASP.NET Core after Google auth
        [HttpGet("google-response")]
        public async Task<IActionResult> GoogleResponse(string returnUrl = null)
        {
            try
            {
                var user = HttpContext.User;
                _logger.LogInformation("GoogleResponse called. User authenticated: {isAuthenticated}", user?.Identity?.IsAuthenticated);
                
                if (user?.Identity == null || !user.Identity.IsAuthenticated)
                {
                    _logger.LogWarning("User not authenticated in GoogleResponse");
                    return Unauthorized();
                }

                var email = user.FindFirst(ClaimTypes.Email)?.Value;
                var name = user.FindFirst(ClaimTypes.Name)?.Value ?? email?.Split('@')[0];
                
                _logger.LogInformation("Google user received - Email: {email}, Name: {name}", email, name);

                if (string.IsNullOrEmpty(email))
                {
                    _logger.LogWarning("No email claim returned from Google");
                    return BadRequest("No email claim returned from Google.");
                }

                // Ensure user exists in DB
                var existing = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (existing == null)
                {
                    _logger.LogInformation("Creating new user: {email}", email);
                    
                    var colors = new[] { "gradient-blue", "gradient-purple", "gradient-pink", "gradient-orange", "gradient-teal" };
                    var randomColor = colors[new Random().Next(colors.Length)];

                    var newUser = new User
                    {
                        Id = Guid.NewGuid(),
                        Username = name,
                        Email = email,
                        PasswordHash = string.Empty,
                        AvatarColor = randomColor,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Users.Add(newUser);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("New user created successfully: {userId}", newUser.Id);
                }
                else
                {
                    _logger.LogInformation("Existing user found: {userId}", existing.Id);
                }

                // Redirect to frontend returnUrl (if safe) or dashboard
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    _logger.LogInformation("Redirecting to returnUrl: {returnUrl}", returnUrl);
                    return Redirect(returnUrl);
                }

                _logger.LogInformation("Redirecting to dashboard");
                return Redirect("/dashboard.html");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GoogleResponse: {message}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An error occurred during login", details = ex.Message });
            }
        }

        [HttpPost("signout")]
        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { message = "Signed out" });
        }

        // Dev-friendly GET signout that redirects to login page after clearing cookie
        [HttpGet("signout-redirect")]
        public async Task<IActionResult> SignOutRedirect(string returnUrl = "/login.html")
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect(returnUrl);
        }

        // Return current authenticated user info
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var user = HttpContext.User;
            if (user?.Identity == null || !user.Identity.IsAuthenticated)
                return Unauthorized();

            var email = user.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return BadRequest("No email claim");

            var existing = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (existing == null)
                return NotFound();

            var dto = new UserDto
            {
                Id = existing.Id,
                Username = existing.Username,
                Email = existing.Email,
                CreatedAt = existing.CreatedAt
            };

            return Ok(new UserDto
            {
                Id = existing.Id,
                Username = existing.Username,
                Email = existing.Email,
                AvatarColor = existing.AvatarColor,
                CreatedAt = existing.CreatedAt
            }); ;
        }
    }
}