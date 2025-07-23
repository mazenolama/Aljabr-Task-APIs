using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SlotManagement.Data;
using SlotManagement.Dtos;
using SlotManagement.Models;

namespace SlotManagement.Controllers
{
    [Authorize] // Requires valid JWT token for all actions
    [ApiController]
    [Route("api/[controller]")]
    public class SlotsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SlotsController(AppDbContext context)
        {
            _context = context;
        }

   
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SlotDto>>> GetSlots(
        [FromQuery] DateTime? date,
        [FromQuery] string? status,
        [FromQuery] int? userId)

        {
            var query = _context.Slots.AsQueryable();

            query = query.Where(s => !s.Deleted);

            //  Filter by UTC date range
            if (date.HasValue)
            {
                var utcStart = DateTime.SpecifyKind(date.Value.Date, DateTimeKind.Utc);
                var utcEnd = utcStart.AddDays(1);

                query = query.Where(s =>
                    s.StartTime >= utcStart && s.StartTime < utcEnd ||
                    s.EndTime >= utcStart && s.EndTime < utcEnd);
            }

            //  Filter by status
            if (!string.IsNullOrWhiteSpace(status))
            {
                Console.WriteLine($"Status param: {status}");

                if (!string.IsNullOrWhiteSpace(status))
                {
                    query = query.Where(s => EF.Functions.ILike(s.Status, status));


                }
                else
                {
                    return BadRequest("Invalid status value.");
                }
            }
            // Filter with UserId
            if (userId.HasValue)
            {
                query = query.Where(s => s.CreatedBy == userId.Value);
            }

            var slots = await query
            .Include(s => s.CreatedByUser)
            .Select(s => new SlotDto
            {
                Id = s.SlotId,
                Start = s.StartTime.ToUniversalTime().ToString("yyyy-MM-dd HH:mm"),
                End = s.EndTime.ToUniversalTime().ToString("yyyy-MM-dd HH:mm"),
                Available = s.IsAvailable,
                Status = s.Status,
                CreatedBy = s.CreatedBy,
                CreatedByName = s.CreatedByUser != null ? s.CreatedByUser.Name : null,
                CreatedOn = s.CreatedOn.ToUniversalTime().ToString("yyyy-MM-dd"),
                ModifiedOn = s.ModifiedOn.ToUniversalTime().ToString("yyyy-MM-dd")
            })
            .ToListAsync();



            return Ok(slots);
        }



        // Only Admins can create a slot
        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<ActionResult<Slot>> CreateSlot(Slot slot)
        {
            var newStart = slot.StartTime.ToUniversalTime();
            var newEnd = slot.EndTime.ToUniversalTime();

            Console.WriteLine($"Attempting to create slot: {newStart} - {newEnd}");

            bool conflictExists = await _context.Slots.AnyAsync(s =>
                !s.Deleted &&
                s.StartTime < newEnd &&
                s.EndTime > newStart
            );

            Console.WriteLine($"Conflict detected: {conflictExists}");

            if (conflictExists)
            {
                return Conflict(new { error = "A slot already exists that overlaps with the specified time range." });
            }

            slot.StartTime = newStart;
            slot.EndTime = newEnd;

            _context.Slots.Add(slot);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetSlots), new { id = slot.SlotId }, slot);
        }

        // Only Admins can update a slot
        [Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSlot(int id, [FromBody] Slot updatedData)
        {
            var slot = await _context.Slots.FindAsync(id);
            if (slot == null)
                return NotFound();

            // Disallow update if slot is booked
            if (slot.Status.Equals("booked", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { error = "Cannot update a booked slot." });
            }

            if (!string.IsNullOrWhiteSpace(updatedData.Status))
                slot.Status = updatedData.Status;

            if (updatedData.StartTime != default)
                slot.StartTime = DateTime.SpecifyKind(updatedData.StartTime, DateTimeKind.Utc);

            if (updatedData.EndTime != default)
                slot.EndTime = DateTime.SpecifyKind(updatedData.EndTime, DateTimeKind.Utc);

            slot.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(slot);
        }



        [Authorize(Roles = "user")]  
        [HttpPut("{id}/book")]
        public async Task<IActionResult> BookSlot(int id)
        {
            var slot = await _context.Slots.FindAsync(id);
            if (slot == null) return NotFound();

            slot.IsAvailable = false;
            slot.Status = "booked";
            slot.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(slot); 
        }

        [Authorize(Roles = "admin")]
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelSlot(int id)
        {
            var slot = await _context.Slots.FindAsync(id);
            if (slot == null || slot.Deleted)
                return NotFound();

            slot.Status = "cancelled";
            slot.IsAvailable = true; 
            slot.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }


        // Only Admins can soft-delete a slot
        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSlot(int id)
        {
            var slot = await _context.Slots.FindAsync(id);
            if (slot == null)
                return NotFound();

            slot.Deleted = true;
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
