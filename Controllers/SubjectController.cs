using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlataformaAPI.Models;

namespace PlataformaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubjectController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SubjectController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SubjectResponseDto>>> GetSubjects()
        {
            return await _context.Subjects
                .Select(s => new SubjectResponseDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Code = s.Code,
                    Description = s.Description,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt
                })
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SubjectResponseDto>> GetSubject(int id)
        {
            var subject = await _context.Subjects.FindAsync(id);

            if (subject == null)
            {
                return NotFound();
            }

            return new SubjectResponseDto
            {
                Id = subject.Id,
                Name = subject.Name,
                Code = subject.Code,
                Description = subject.Description,
                CreatedAt = subject.CreatedAt,
                UpdatedAt = subject.UpdatedAt
            };
        }

        [HttpPost]
        public async Task<ActionResult<SubjectResponseDto>> PostSubject([FromBody] CreateSubjectDto createDto)
        {
            if (await _context.Subjects.AnyAsync(s => s.Code == createDto.Code))
            {
                return Conflict("Ya existe una materia con este código");
            }

            var subject = new Subject
            {
                Name = createDto.Name,
                Code = createDto.Code,
                Description = createDto.Description,
                UpdatedAt = null
            };

            _context.Subjects.Add(subject);
            await _context.SaveChangesAsync();

            var responseDto = new SubjectResponseDto
            {
                Id = subject.Id,
                Name = subject.Name,
                Code = subject.Code,
                Description = subject.Description,
                CreatedAt = subject.CreatedAt,
                UpdatedAt = subject.UpdatedAt
            };

            return CreatedAtAction(nameof(GetSubject), new { id = subject.Id }, responseDto);
        }
        
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSubject(int id, [FromBody] UpdateSubjectDto updateDto)
        {
            var subject = await _context.Subjects.FindAsync(id);
            if (subject == null)
            {
                return NotFound();
            }

            if (await _context.Subjects.AnyAsync(s => s.Code == updateDto.Code && s.Id != id))
            {
                return Conflict("Ya existe otra materia con este código");
            }

            subject.Name = updateDto.Name;
            subject.Code = updateDto.Code;
            subject.Description = updateDto.Description;
            subject.UpdatedAt = DateTime.UtcNow;

            _context.Entry(subject).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SubjectExists(id))
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
        public async Task<IActionResult> DeleteSubject(int id)
        {
            var subject = await _context.Subjects.FindAsync(id);
            if (subject == null)
            {
                return NotFound();
            }

            _context.Subjects.Remove(subject);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SubjectExists(int id)
        {
            return _context.Subjects.Any(e => e.Id == id);
        }
    }
}