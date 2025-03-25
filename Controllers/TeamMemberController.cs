using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlataformaAPI.Models;

namespace PlataformaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeamMemberController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TeamMemberController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TeamMemberResponseDto>>> GetTeamMembers()
        {
            return await _context.TeamMembers
                .Include(tm => tm.User)
                .Select(tm => new TeamMemberResponseDto
                {
                    Id = tm.Id,
                    ReservationId = tm.ReservationId,
                    UserId = tm.UserId,
                    UserName = tm.User.Name,
                    UserEmail = tm.User.Email,
                    CreatedAt = tm.CreatedAt,
                    UpdatedAt = tm.UpdatedAt
                })
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TeamMemberResponseDto>> GetTeamMember(int id)
        {
            var teamMember = await _context.TeamMembers
                .Include(tm => tm.User)
                .FirstOrDefaultAsync(tm => tm.Id == id);

            if (teamMember == null)
            {
                return NotFound();
            }

            return new TeamMemberResponseDto
            {
                Id = teamMember.Id,
                ReservationId = teamMember.ReservationId,
                UserId = teamMember.UserId,
                UserName = teamMember.User.Name,
                UserEmail = teamMember.User.Email,
                CreatedAt = teamMember.CreatedAt,
                UpdatedAt = teamMember.UpdatedAt
            };
        }

        [HttpPost]
        public async Task<ActionResult<TeamMemberResponseDto>> PostTeamMember([FromBody] CreateTeamMemberDto createDto)
        {
            var reservation = await _context.Reservations.FindAsync(createDto.ReservationId);
            if (reservation == null)
            {
                return BadRequest("La reservación especificada no existe");
            }

            var user = await _context.Users.FindAsync(createDto.UserId);
            if (user == null)
            {
                return BadRequest("El usuario especificado no existe");
            }

            if (await _context.TeamMembers.AnyAsync(tm =>
                tm.ReservationId == createDto.ReservationId && tm.UserId == createDto.UserId))
            {
                return Conflict("Este usuario ya es miembro del equipo para esta reservación");
            }

            var teamMember = new TeamMember
            {
                ReservationId = createDto.ReservationId,
                UserId = createDto.UserId,
                UpdatedAt = null
            };

            _context.TeamMembers.Add(teamMember);
            await _context.SaveChangesAsync();

            await _context.Entry(teamMember).Reference(tm => tm.User).LoadAsync();

            var responseDto = new TeamMemberResponseDto
            {
                Id = teamMember.Id,
                ReservationId = teamMember.ReservationId,
                UserId = teamMember.UserId,
                UserName = teamMember.User.Name,
                UserEmail = teamMember.User.Email,
                CreatedAt = teamMember.CreatedAt,
                UpdatedAt = teamMember.UpdatedAt
            };

            return CreatedAtAction(nameof(GetTeamMember), new { id = teamMember.Id }, responseDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTeamMember(int id)
        {
            var teamMember = await _context.TeamMembers.FindAsync(id);
            if (teamMember == null)
            {
                return NotFound();
            }

            _context.TeamMembers.Remove(teamMember);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("Reservation/{reservationId}")]
        public async Task<ActionResult<IEnumerable<TeamMemberResponseDto>>> GetTeamMembersByReservation(int reservationId)
        {
            return await _context.TeamMembers
                .Where(tm => tm.ReservationId == reservationId)
                .Include(tm => tm.User)
                .Select(tm => new TeamMemberResponseDto
                {
                    Id = tm.Id,
                    ReservationId = tm.ReservationId,
                    UserId = tm.UserId,
                    UserName = tm.User.Name,
                    UserEmail = tm.User.Email,
                    CreatedAt = tm.CreatedAt,
                    UpdatedAt = tm.UpdatedAt
                })
                .ToListAsync();
        }

        [HttpGet("User/{userId}")]
        public async Task<ActionResult<IEnumerable<TeamMemberResponseDto>>> GetReservationsByUser(int userId)
        {
            return await _context.TeamMembers
                .Where(tm => tm.UserId == userId)
                .Include(tm => tm.User)
                .Include(tm => tm.Reservation)
                .Select(tm => new TeamMemberResponseDto
                {
                    Id = tm.Id,
                    ReservationId = tm.ReservationId,
                    UserId = tm.UserId,
                    UserName = tm.User.Name,
                    UserEmail = tm.User.Email,
                    CreatedAt = tm.CreatedAt,
                    UpdatedAt = tm.UpdatedAt
                })
                .ToListAsync();
        }
    }
}