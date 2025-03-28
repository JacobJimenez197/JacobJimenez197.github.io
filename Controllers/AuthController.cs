using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PlataformaAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<User> userManager, ILogger<AuthController> logger,
        SignInManager<User> signInManager,
        IConfiguration config,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _config = config;
        _logger = logger;
        _configuration = configuration;
    }
    private JwtSecurityToken GetToken(List<Claim> authClaims)
    {
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

        var token = new JwtSecurityToken(
            issuer: _configuration["JWT:ValidIssuer"],
            audience: _configuration["JWT:ValidAudience"],
            expires: DateTime.Now.AddHours(3),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        return token;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
            return Unauthorized();

        var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
        if (!result.Succeeded)
            return Unauthorized();

        var authClaims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var userRoles = await _userManager.GetRolesAsync(user);
        foreach (var role in userRoles)
        {
            authClaims.Add(new Claim(ClaimTypes.Role, role));
        }

        var token = GetToken(authClaims);

        return Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(token),
            expiration = token.ValidTo
        });
    }
    [HttpPost("google")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AuthResponse>> GoogleLogin([FromBody] GoogleAuthRequest request)
    {
        _logger.LogInformation("Inicio de petición Google Login");

        try
        {
            // 1. Validación básica
            if (string.IsNullOrEmpty(request?.Token))
            {
                _logger.LogWarning("Token no proporcionado");
                return BadRequest(new { Message = "Token requerido" });
            }

            // 2. Verificación de token Firebase
            FirebaseToken decodedToken;
            try
            {
                decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.Token);
                _logger.LogInformation("Token verificado para: {Email}", decodedToken.Claims["email"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verificando token Firebase");
                return Unauthorized(new { Message = "Token inválido" });
            }

            // 3. Procesamiento seguro del usuario
            var user = await ProcessUserAsync(decodedToken);

            // 4. Generación de JWT
            var token = GenerateJwtToken(user);

            _logger.LogInformation("Login exitoso para: {Email}", user.Email);

            return Ok(new AuthResponse
            {
                Token = token,
                User = new UserDto
                {
                    Email = user.Email,
                    Name = user.Name,
                    Role = user.RoleId.ToString()
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Error crítico en GoogleLogin");
            return StatusCode(500, new { Message = "Error interno del servidor" });
        }
    }

    private async Task<User> ProcessUserAsync(FirebaseToken decodedToken)
    {
        var email = decodedToken.Claims["email"].ToString();
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            user = new User
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                Name = decodedToken.Claims["name"].ToString(),
                Matricula = email.Split('@')[0],
                RoleId = 2 
            };

            var result = await _userManager.CreateAsync(user, "TempPassword123!");
            if (!result.Succeeded)
            {
                throw new Exception($"Error creando usuario: {string.Join(", ", result.Errors)}");
            }
        }

        return user;
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.RoleId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Secret"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["JWT:ValidIssuer"],
            audience: _config["JWT:ValidAudience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(3),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
