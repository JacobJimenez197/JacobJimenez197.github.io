using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlataformaAPI.Models;

namespace PlataformaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public UserController(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
                .Include(u => u.Role)
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    u.UserName,
                    RoleId = u.RoleId,
                    RoleName = u.Role.Name,
                    u.Matricula,
                    u.CreatedAt,
                    u.UpdatedAt
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateDto userDto)
        {
            if (!await _context.Roles.AnyAsync(r => r.Id == userDto.RoleId))
            {
                return BadRequest("El RoleId especificado no existe");
            }

            var user = new User
            {
                UserName = userDto.Email,
                Email = userDto.Email,
                Name = userDto.Name,
                RoleId = userDto.RoleId,
                Matricula = userDto.Matricula,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, userDto.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateDto userDto)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }

            if (!await _context.Roles.AnyAsync(r => r.Id == userDto.RoleId))
            {
                return BadRequest("El RoleId especificado no existe");
            }

            user.Name = userDto.Name;
            user.Email = userDto.Email;
            user.UserName = userDto.Email;
            user.RoleId = userDto.RoleId;
            user.Matricula = userDto.Matricula;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);

            if (!string.IsNullOrEmpty(userDto.NewPassword))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                await _userManager.ResetPasswordAsync(user, token, userDto.NewPassword);
            }

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return NoContent();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.Role)  
                .Where(u => u.Id == id)
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    u.UserName,
                    RoleId = u.RoleId,
                    RoleName = u.Role.Name,  
                    u.Matricula,
                    u.CreatedAt,
                    u.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }
    }
    
}