using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkFlow.Services;

namespace WorkFlow.Controllers
{
    [ApiController]
    [Route("api/admin/seed")]
    public class SeedController : ControllerBase
    {
        private readonly SeedService _seed;
        private readonly IWebHostEnvironment _env;

        public SeedController(SeedService seed, IWebHostEnvironment env)
        {
            _seed = seed;
            _env = env;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Seed()
        {
            if (!_env.IsDevelopment())
                return Forbid();

            var seeded = await _seed.SeedAsync();
            if (!seeded)
                return Conflict(new { message = "Seed data already exists." });

            return Ok(new { message = "Seed data created." });
        }
    }
}
