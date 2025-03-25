using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlataformaAPI.Models;

namespace PlataformaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReservationController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Reservation
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReservationResponseDto>>> GetReservations()
        {
            return await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Subject)
                .Include(r => r.Group)
                .Select(r => new ReservationResponseDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    UserName = r.User.Name,
                    SubjectId = r.SubjectId,
                    SubjectName = r.Subject != null ? r.Subject.Name : null,
                    GroupId = r.GroupId,
                    GroupName = r.Group != null ? r.Group.Name : null,
                    StartTime = r.StartTime,
                    EndTime = r.EndTime,
                    Status = r.Status.ToString(),
                    Purpose = r.Purpose,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt
                })
                .ToListAsync();
        }

        // GET: api/Reservation/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ReservationResponseDto>> GetReservation(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Subject)
                .Include(r => r.Group)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
            {
                return NotFound();
            }

            return new ReservationResponseDto
            {
                Id = reservation.Id,
                UserId = reservation.UserId,
                UserName = reservation.User.Name,
                SubjectId = reservation.SubjectId,
                SubjectName = reservation.Subject?.Name,
                GroupId = reservation.GroupId,
                GroupName = reservation.Group?.Name,
                StartTime = reservation.StartTime,
                EndTime = reservation.EndTime,
                Status = reservation.Status.ToString(),
                Purpose = reservation.Purpose,
                CreatedAt = reservation.CreatedAt,
                UpdatedAt = reservation.UpdatedAt
            };
        }

        // POST: api/Reservation
        [HttpPost]
        public async Task<ActionResult<ReservationResponseDto>> PostReservation([FromBody] CreateReservationDto createDto)
        {
            // Validar que el usuario exista
            var user = await _context.Users.FindAsync(createDto.UserId);
            if (user == null)
            {
                return BadRequest("El usuario especificado no existe");
            }

            // Validar que la materia exista si se proporciona
            if (createDto.SubjectId.HasValue && !await _context.Subjects.AnyAsync(s => s.Id == createDto.SubjectId))
            {
                return BadRequest("La materia especificada no existe");
            }

            // Validar que el grupo exista si se proporciona
            if (createDto.GroupId.HasValue && !await _context.Groups.AnyAsync(g => g.Id == createDto.GroupId))
            {
                return BadRequest("El grupo especificado no existe");
            }

            // Validar que la fecha de inicio sea anterior a la de fin
            if (createDto.StartTime >= createDto.EndTime)
            {
                return BadRequest("La fecha de inicio debe ser anterior a la fecha de fin");
            }

            var reservation = new Reservation
            {
                UserId = createDto.UserId,
                SubjectId = createDto.SubjectId,
                GroupId = createDto.GroupId,
                StartTime = createDto.StartTime,
                EndTime = createDto.EndTime,
                Purpose = createDto.Purpose,
                Status = ReservationStatus.Pending,
                UpdatedAt = null
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            // Cargar datos relacionados para la respuesta
            await _context.Entry(reservation).Reference(r => r.User).LoadAsync();
            await _context.Entry(reservation).Reference(r => r.Subject).LoadAsync();
            await _context.Entry(reservation).Reference(r => r.Group).LoadAsync();

            var responseDto = new ReservationResponseDto
            {
                Id = reservation.Id,
                UserId = reservation.UserId,
                UserName = reservation.User.Name,
                SubjectId = reservation.SubjectId,
                SubjectName = reservation.Subject?.Name,
                GroupId = reservation.GroupId,
                GroupName = reservation.Group?.Name,
                StartTime = reservation.StartTime,
                EndTime = reservation.EndTime,
                Status = reservation.Status.ToString(),
                Purpose = reservation.Purpose,
                CreatedAt = reservation.CreatedAt,
                UpdatedAt = reservation.UpdatedAt
            };

            return CreatedAtAction(nameof(GetReservation), new { id = reservation.Id }, responseDto);
        }

        // PUT: api/Reservation/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReservation(int id, [FromBody] UpdateReservationDto updateDto)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            // Validar que la materia exista si se proporciona
            if (updateDto.SubjectId.HasValue && !await _context.Subjects.AnyAsync(s => s.Id == updateDto.SubjectId))
            {
                return BadRequest("La materia especificada no existe");
            }

            // Validar que el grupo exista si se proporciona
            if (updateDto.GroupId.HasValue && !await _context.Groups.AnyAsync(g => g.Id == updateDto.GroupId))
            {
                return BadRequest("El grupo especificado no existe");
            }

            // Validar que la fecha de inicio sea anterior a la de fin si se actualizan ambas
            if (updateDto.StartTime.HasValue && updateDto.EndTime.HasValue &&
                updateDto.StartTime.Value >= updateDto.EndTime.Value)
            {
                return BadRequest("La fecha de inicio debe ser anterior a la fecha de fin");
            }

            // Actualizar campos
            if (updateDto.SubjectId.HasValue) reservation.SubjectId = updateDto.SubjectId;
            if (updateDto.GroupId.HasValue) reservation.GroupId = updateDto.GroupId;
            if (updateDto.StartTime.HasValue) reservation.StartTime = updateDto.StartTime.Value;
            if (updateDto.EndTime.HasValue) reservation.EndTime = updateDto.EndTime.Value;
            if (!string.IsNullOrEmpty(updateDto.Purpose)) reservation.Purpose = updateDto.Purpose;

            if (!string.IsNullOrEmpty(updateDto.Status))
            {
                if (Enum.TryParse<ReservationStatus>(updateDto.Status, true, out var status))
                {
                    reservation.Status = status;
                }
                else
                {
                    return BadRequest("Estado inválido. Use 'pending', 'confirmed', 'cancelled' o 'completed'");
                }
            }

            reservation.UpdatedAt = DateTime.UtcNow;

            _context.Entry(reservation).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReservationExists(id))
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

        // DELETE: api/Reservation/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservation(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Reservation/User/5
        [HttpGet("User/{userId}")]
        public async Task<ActionResult<IEnumerable<ReservationResponseDto>>> GetUserReservations(int userId)
        {
            var reservations = await _context.Reservations
                .Where(r => r.UserId == userId)
                .Include(r => r.User)
                .Include(r => r.Subject)
                .Include(r => r.Group)
                .Select(r => new ReservationResponseDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    UserName = r.User.Name,
                    SubjectId = r.SubjectId,
                    SubjectName = r.Subject != null ? r.Subject.Name : null,
                    GroupId = r.GroupId,
                    GroupName = r.Group != null ? r.Group.Name : null,
                    StartTime = r.StartTime,
                    EndTime = r.EndTime,
                    Status = r.Status.ToString(),
                    Purpose = r.Purpose,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt
                })
                .ToListAsync();

            return reservations;
        }

        private bool ReservationExists(int id)
        {
            return _context.Reservations.Any(e => e.Id == id);
        }
    }
}