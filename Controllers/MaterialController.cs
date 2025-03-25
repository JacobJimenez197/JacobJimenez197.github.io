using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlataformaAPI.Models;

namespace PlataformaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaterialController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MaterialController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Material
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MaterialResponseDto>>> GetMaterials()
        {
            return await _context.Materials
                .Select(m => new MaterialResponseDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Description = m.Description,
                    Stock = m.Stock,
                    Category = m.Category.ToString(),
                    CreatedAt = m.CreatedAt,
                    UpdatedAt = m.UpdatedAt
                })
                .ToListAsync();
        }

        // GET: api/Material/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MaterialResponseDto>> GetMaterial(int id)
        {
            var material = await _context.Materials.FindAsync(id);

            if (material == null)
            {
                return NotFound();
            }

            return new MaterialResponseDto
            {
                Id = material.Id,
                Name = material.Name,
                Description = material.Description,
                Stock = material.Stock,
                Category = material.Category.ToString(),
                CreatedAt = material.CreatedAt,
                UpdatedAt = material.UpdatedAt
            };
        }

        // POST: api/Material
        [HttpPost]
        public async Task<ActionResult<MaterialResponseDto>> PostMaterial([FromBody] CreateMaterialDto createDto)
        {
            if (!Enum.TryParse<MaterialCategory>(createDto.Category, true, out var category))
            {
                return BadRequest("Categoría inválida. Use 'material' o 'reactivo'");
            }

            var material = new Material
            {
                Name = createDto.Name,
                Description = createDto.Description,
                Stock = createDto.Stock,
                Category = category,
                UpdatedAt = null
            };

            _context.Materials.Add(material);
            await _context.SaveChangesAsync();

            var responseDto = new MaterialResponseDto
            {
                Id = material.Id,
                Name = material.Name,
                Description = material.Description,
                Stock = material.Stock,
                Category = material.Category.ToString(),
                CreatedAt = material.CreatedAt,
                UpdatedAt = material.UpdatedAt
            };

            return CreatedAtAction(nameof(GetMaterial), new { id = material.Id }, responseDto);
        }

        // PUT: api/Material/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMaterial(int id, [FromBody] UpdateMaterialDto updateDto)
        {
            var material = await _context.Materials.FindAsync(id);
            if (material == null)
            {
                return NotFound();
            }

            if (!Enum.TryParse<MaterialCategory>(updateDto.Category, true, out var category))
            {
                return BadRequest("Categoría inválida. Use 'material' o 'reactivo'");
            }

            material.Name = updateDto.Name;
            material.Description = updateDto.Description;
            material.Stock = updateDto.Stock;
            material.Category = category;
            material.UpdatedAt = DateTime.UtcNow;

            _context.Entry(material).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MaterialExists(id))
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

        // DELETE: api/Material/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMaterial(int id)
        {
            var material = await _context.Materials.FindAsync(id);
            if (material == null)
            {
                return NotFound();
            }

            _context.Materials.Remove(material);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Material/Category/material
        [HttpGet("Category/{category}")]
        public async Task<ActionResult<IEnumerable<MaterialResponseDto>>> GetMaterialsByCategory(string category)
        {
            if (!Enum.TryParse<MaterialCategory>(category, true, out var categoryEnum))
            {
                return BadRequest("Categoría inválida. Use 'material' o 'reactivo'");
            }

            var materials = await _context.Materials
                .Where(m => m.Category == categoryEnum)
                .Select(m => new MaterialResponseDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Description = m.Description,
                    Stock = m.Stock,
                    Category = m.Category.ToString(),
                    CreatedAt = m.CreatedAt,
                    UpdatedAt = m.UpdatedAt
                })
                .ToListAsync();

            return materials;
        }

        private bool MaterialExists(int id)
        {
            return _context.Materials.Any(e => e.Id == id);
        }
    }
}