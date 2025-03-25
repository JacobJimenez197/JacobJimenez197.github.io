using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlataformaAPI.Models;

namespace PlataformaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupController : ControllerBase
    {
        private readonly AppDbContext _context;

        public GroupController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Group
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GroupResponseDto>>> GetGroups()
        {
            return await _context.Groups
                .Select(g => new GroupResponseDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    Code = g.Code,
                    CreatedAt = g.CreatedAt,
                    UpdatedAt = g.UpdatedAt
                })
                .ToListAsync();
        }

        // GET: api/Group/5
        [HttpGet("{id}")]
        public async Task<ActionResult<GroupResponseDto>> GetGroup(int id)
        {
            var group = await _context.Groups.FindAsync(id);

            if (group == null)
            {
                return NotFound();
            }

            return new GroupResponseDto
            {
                Id = group.Id,
                Name = group.Name,
                Code = group.Code,
                CreatedAt = group.CreatedAt,
                UpdatedAt = group.UpdatedAt
            };
        }

        // POST: api/Group
        [HttpPost]
        public async Task<ActionResult<GroupResponseDto>> PostGroup([FromBody] CreateGroupDto createDto)
        {
            // Validar que el código sea único
            if (await _context.Groups.AnyAsync(g => g.Code == createDto.Code))
            {
                return Conflict("Ya existe un grupo con este código");
            }

            var group = new Group
            {
                Name = createDto.Name,
                Code = createDto.Code,
                UpdatedAt = null
            };

            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            var responseDto = new GroupResponseDto
            {
                Id = group.Id,
                Name = group.Name,
                Code = group.Code,
                CreatedAt = group.CreatedAt,
                UpdatedAt = group.UpdatedAt
            };

            return CreatedAtAction(nameof(GetGroup), new { id = group.Id }, responseDto);
        }

        // PUT: api/Group/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutGroup(int id, [FromBody] UpdateGroupDto updateDto)
        {
            var group = await _context.Groups.FindAsync(id);
            if (group == null)
            {
                return NotFound();
            }

            // Validar que el código sea único (excluyendo el grupo actual)
            if (await _context.Groups.AnyAsync(g => g.Code == updateDto.Code && g.Id != id))
            {
                return Conflict("Ya existe otro grupo con este código");
            }

            group.Name = updateDto.Name;
            group.Code = updateDto.Code;
            group.UpdatedAt = DateTime.UtcNow;

            _context.Entry(group).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GroupExists(id))
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

        // DELETE: api/Group/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGroup(int id)
        {
            var group = await _context.Groups.FindAsync(id);
            if (group == null)
            {
                return NotFound();
            }

            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool GroupExists(int id)
        {
            return _context.Groups.Any(e => e.Id == id);
        }
    }
}