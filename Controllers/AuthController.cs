using Microsoft.AspNetCore.Mvc;
using EcommerceBackend.Data;
using EcommerceBackend.Models;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace EcommerceBackend.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly AppDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        public AuthController(IConfiguration config, AppDbContext context, IHttpClientFactory httpClientFactory)
        {
            _config = config;
            _context = context;
            _httpClientFactory = httpClientFactory;
        }


        [HttpGet("login")]
        public IActionResult Login()
        {
            var clientId = _config["GitHub:ClientId"];
            // Use the callback endpoint that matches your routing and GitHub settings.
            var redirectUri = Uri.EscapeDataString("https://ecommerceapiwebapp-daege4dme6bqbsak.uksouth-01.azurewebsites.net/auth/callback");
            var state = Guid.NewGuid().ToString(); // You can store this value for additional security.
            var authorizationUrl = $"https://github.com/login/oauth/authorize?client_id={clientId}&redirect_uri={redirectUri}&state={state}";
            return Redirect(authorizationUrl);
        }


        [HttpGet("callback")]
        public async Task<IActionResult> Callback([FromQuery] string code, [FromQuery] string state)
        {
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest("Missing code");
            }

            // Exchange code for GitHub access token.
            var token = await ExchangeCodeForToken(code);
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("GitHub token exchange failed");
            }


            var githubUser = await GetGitHubUser(token);
            if (githubUser == null)
            {
                return Unauthorized("Failed to retrieve GitHub user info");
            }

            bool isNewUser = false;


            var userProfile = await _context.UserProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.GitHubId == githubUser.id.ToString());

            if (userProfile == null)
            {
                isNewUser = true;

                /* string role = githubUser.login.Equals("rami55cz", StringComparison.OrdinalIgnoreCase)
                    ? "administrator"
                    : "customer"; */

                 string role = githubUser.login.Equals("rami55cz", StringComparison.OrdinalIgnoreCase)
                    ? "administrator"
                    : githubUser.login.Equals("rami5500", StringComparison.OrdinalIgnoreCase)
                        ? "customer"
                        : "vendor";

                userProfile = new UserProfile
                {
                    GitHubId = githubUser.id.ToString(),
                    Username = githubUser.login,
                    Role = role
                };

                _context.UserProfiles.Add(userProfile);
                await _context.SaveChangesAsync();

                // Re-read the saved user record to ensure we have the most recent data.
                userProfile = await _context.UserProfiles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.GitHubId == githubUser.id.ToString());
            }
            else
            {
                
                // Optionally, you can log or debug the userProfile.Role here to verify it reflects any changes.
            }

            // Build JWT claims using the freshly retrieved userProfile data.
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, githubUser.id.ToString()),
        new Claim(ClaimTypes.Name, githubUser.login),
        new Claim(ClaimTypes.Role, userProfile.Role)
    };

            var jwtToken = GenerateJwtToken(claims);
            var redirectUrl = $"https://ecommerceblazorfeapp-euf0a7byf9fzhdhr.uksouth-01.azurewebsites.net/tokenreceiver?token={jwtToken}&newUser={isNewUser}";
            return Redirect(redirectUrl);
        }

        private async Task<string> ExchangeCodeForToken(string code)
        {
            var clientId = _config["GitHub:ClientId"];
            var clientSecret = _config["GitHub:ClientSecret"];
            var request = new HttpRequestMessage(HttpMethod.Post, "https://github.com/login/oauth/access_token");
            var parameters = new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "code", code }
            };
            request.Content = new FormUrlEncodedContent(parameters);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var client = _httpClientFactory.CreateClient();
            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.TryGetProperty("access_token", out JsonElement tokenElement))
            {
                return tokenElement.GetString();
            }
            return null;
        }

        private async Task<GitHubUser> GetGitHubUser(string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            // GitHub requires a User-Agent header.
            request.Headers.UserAgent.TryParseAdd("MyApp");

            var client = _httpClientFactory.CreateClient();
            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<GitHubUser>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        private string GenerateJwtToken(IEnumerable<Claim> claims)
        {
            var secretKey = _config["Jwt:Secret"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = creds,
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public class GitHubUser
        {
            public long id { get; set; }
            public string login { get; set; }

        }
    }
}
