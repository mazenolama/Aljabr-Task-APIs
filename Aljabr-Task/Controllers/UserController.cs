using Microsoft.AspNetCore.Mvc;
using SlotManagement.Data;
using System.Linq;

namespace Aljabr_Task.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetUsers()
        {
            var users = _context.Users
                .Select(u => new { u.UserId, u.Name })
                .ToList();

            return Ok(users);
        }
    }
}
