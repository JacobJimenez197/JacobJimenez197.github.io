using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlataformaAPI.Models;

namespace PlataformaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationMaterialController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReservationMaterialController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/ReservationMaterial
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReservationMaterialResponseDto>>> GetReservationMaterials()
        {
            return await _context.ReservationMaterials
                .Include(rm => rm.Material)
                .Select(rm => new ReservationMaterialResponseDto
                {
                    Id = rm.Id,
                    ReservationId = rm.ReservationId,
                    MaterialId = rm.MaterialId,
                    MaterialName = rm.Material.Name,
                    Quantity = rm.Quantity,
                    ReturnedQuantity = rm.ReturnedQuantity,
                    Status = rm.Status.ToString(),
                    CreatedAt = rm.CreatedAt,
                    UpdatedAt = rm.UpdatedAt
                })
                .ToListAsync();
        }

        // GET: api/ReservationMaterial/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ReservationMaterialResponseDto>> GetReservationMaterial(int id)
        {
            var reservationMaterial = await _context.ReservationMaterials
                .Include(rm => rm.Material)
                .FirstOrDefaultAsync(rm => rm.Id == id);

            if (reservationMaterial == null)
            {
                return NotFound();
            }

            return new ReservationMaterialResponseDto
            {
                Id = reservationMaterial.Id,
                ReservationId = reservationMaterial.ReservationId,
                MaterialId = reservationMaterial.MaterialId,
                MaterialName = reservationMaterial.Material.Name,
                Quantity = reservationMaterial.Quantity,
                ReturnedQuantity = reservationMaterial.ReturnedQuantity,
                Status = reservationMaterial.Status.ToString(),
                CreatedAt = reservationMaterial.CreatedAt,
                UpdatedAt = reservationMaterial.UpdatedAt
            };
        }

        // POST: api/ReservationMaterial
        [HttpPost]
        public async Task<ActionResult<ReservationMaterialResponseDto>> PostReservationMaterial(
            [FromBody] CreateReservationMaterialDto createDto)
        {
            // Validar que la reservación exista
            var reservation = await _context.Reservations.FindAsync(createDto.ReservationId);
            if (reservation == null)
            {
                return BadRequest("La reservación especificada no existe");
            }

            // Validar que el material exista
            var material = await _context.Materials.FindAsync(createDto.MaterialId);
            if (material == null)
            {
                return BadRequest("El material especificado no existe");
            }

            // Validar que haya suficiente stock
            if (material.Stock < createDto.Quantity)
            {
                return BadRequest($"No hay suficiente stock. Disponible: {material.Stock}");
            }

            var reservationMaterial = new ReservationMaterial
            {
                ReservationId = createDto.ReservationId,
                MaterialId = createDto.MaterialId,
                Quantity = createDto.Quantity,
                Status = ReservationMaterialStatus.Reserved,
                UpdatedAt = null
            };

            // Reducir el stock del material
            material.Stock -= createDto.Quantity;
            _context.Entry(material).State = EntityState.Modified;

            _context.ReservationMaterials.Add(reservationMaterial);
            await _context.SaveChangesAsync();

            // Cargar datos del material para la respuesta
            await _context.Entry(reservationMaterial).Reference(rm => rm.Material).LoadAsync();

            var responseDto = new ReservationMaterialResponseDto
            {
                Id = reservationMaterial.Id,
                ReservationId = reservationMaterial.ReservationId,
                MaterialId = reservationMaterial.MaterialId,
                MaterialName = reservationMaterial.Material.Name,
                Quantity = reservationMaterial.Quantity,
                ReturnedQuantity = reservationMaterial.ReturnedQuantity,
                Status = reservationMaterial.Status.ToString(),
                CreatedAt = reservationMaterial.CreatedAt,
                UpdatedAt = reservationMaterial.UpdatedAt
            };

            return CreatedAtAction(nameof(GetReservationMaterial), new { id = reservationMaterial.Id }, responseDto);
        }

        // PUT: api/ReservationMaterial/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReservationMaterial(int id, [FromBody] UpdateReservationMaterialDto updateDto)
        {
            var reservationMaterial = await _context.ReservationMaterials
                .Include(rm => rm.Material)
                .FirstOrDefaultAsync(rm => rm.Id == id);

            if (reservationMaterial == null)
            {
                return NotFound();
            }

            // Validar cantidad devuelta
            if (updateDto.ReturnedQuantity.HasValue)
            {
                if (updateDto.ReturnedQuantity.Value > reservationMaterial.Quantity)
                {
                    return BadRequest("La cantidad devuelta no puede ser mayor que la cantidad reservada");
                }

                if (updateDto.ReturnedQuantity.Value < 0)
                {
                    return BadRequest("La cantidad devuelta no puede ser negativa");
                }

                reservationMaterial.ReturnedQuantity = updateDto.ReturnedQuantity.Value;
            }

            // Actualizar estado si se proporciona
            if (!string.IsNullOrEmpty(updateDto.Status))
            {
                if (Enum.TryParse<ReservationMaterialStatus>(updateDto.Status, true, out var status))
                {
                    // Si se marca como devuelto, actualizar stock
                    if (status == ReservationMaterialStatus.Returned &&
                        reservationMaterial.Status != ReservationMaterialStatus.Returned)
                    {
                        var material = reservationMaterial.Material;
                        material.Stock += reservationMaterial.ReturnedQuantity;
                        _context.Entry(material).State = EntityState.Modified;
                    }

                    reservationMaterial.Status = status;
                }
                else
                {
                    return BadRequest("Estado inválido. Use 'reserved', 'returned' o 'damaged'");
                }
            }

            reservationMaterial.UpdatedAt = DateTime.UtcNow;

            _context.Entry(reservationMaterial).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReservationMaterialExists(id))
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

        // DELETE: api/ReservationMaterial/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservationMaterial(int id)
        {
            var reservationMaterial = await _context.ReservationMaterials
                .Include(rm => rm.Material)
                .FirstOrDefaultAsync(rm => rm.Id == id);

            if (reservationMaterial == null)
            {
                return NotFound();
            }

            // Devolver el stock al eliminar
            var material = reservationMaterial.Material;
            material.Stock += reservationMaterial.Quantity - reservationMaterial.ReturnedQuantity;
            _context.Entry(material).State = EntityState.Modified;

            _context.ReservationMaterials.Remove(reservationMaterial);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/ReservationMaterial/Reservation/5
        [HttpGet("Reservation/{reservationId}")]
        public async Task<ActionResult<IEnumerable<ReservationMaterialResponseDto>>> GetMaterialsByReservation(int reservationId)
        {
            return await _context.ReservationMaterials
                .Where(rm => rm.ReservationId == reservationId)
                .Include(rm => rm.Material)
                .Select(rm => new ReservationMaterialResponseDto
                {
                    Id = rm.Id,
                    ReservationId = rm.ReservationId,
                    MaterialId = rm.MaterialId,
                    MaterialName = rm.Material.Name,
                    Quantity = rm.Quantity,
                    ReturnedQuantity = rm.ReturnedQuantity,
                    Status = rm.Status.ToString(),
                    CreatedAt = rm.CreatedAt,
                    UpdatedAt = rm.UpdatedAt
                })
                .ToListAsync();
        }

        private bool ReservationMaterialExists(int id)
        {
            return _context.ReservationMaterials.Any(e => e.Id == id);
        }
    }
}