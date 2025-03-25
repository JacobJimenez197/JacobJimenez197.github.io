using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlataformaAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace PlataformaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class RoleController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RoleController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Role>>> GetRoles()
        {
            return await _context.Roles.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Role>> GetRole(int id)
        {
            var role = await _context.Roles.FindAsync(id);

            if (role == null)
            {
                return NotFound();
            }

            return role;
        }

        [HttpPost]
        public async Task<ActionResult<Role>> PostRole([FromBody] RoleCreateDto roleDto)
        {
            if (await _context.Roles.AnyAsync(r => r.NormalizedName == roleDto.Name.ToUpper()))
            {
                return Conflict("Ya existe un rol con este nombre");
            }

            var role = new Role
            {
                Name = roleDto.Name,
                NormalizedName = roleDto.Name.ToUpper(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRole), new { id = role.Id }, role);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutRole(int id, [FromBody] RoleUpdateDto roleDto)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            if (await _context.Roles.AnyAsync(r => r.NormalizedName == roleDto.Name.ToUpper() && r.Id != id))
            {
                return Conflict("Ya existe otro rol con este nombre");
            }

            role.Name = roleDto.Name;
            role.NormalizedName = roleDto.Name.ToUpper();
            role.ConcurrencyStamp = Guid.NewGuid().ToString();

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RoleExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            var usersWithRole = await _context.Users.AnyAsync(u => u.RoleId == id);
            if (usersWithRole)
            {
                return BadRequest("No se puede eliminar el rol porque tiene usuarios asociados");
            }

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RoleExists(int id)
        {
            return _context.Roles.Any(e => e.Id == id);
        }
    }

}