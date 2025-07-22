using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SlotManagement.Data;
using SlotManagement.Models;

namespace SlotManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SlotsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SlotsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Slot>>> GetSlots()
        {
            return await _context.Slots.Where(s => !s.Deleted).ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Slot>> CreateSlot(Slot slot)
        {
            _context.Slots.Add(slot);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetSlots), new { id = slot.SlotId }, slot);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSlot(int id, Slot slot)
        {
            if (id != slot.SlotId) return BadRequest();

            _context.Entry(slot).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSlot(int id)
        {
            var slot = await _context.Slots.FindAsync(id);
            if (slot == null) return NotFound();

            slot.Deleted = true;
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
