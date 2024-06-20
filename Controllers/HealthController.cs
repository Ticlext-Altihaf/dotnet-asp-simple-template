using BozoAIAggregator.Utility;
using Microsoft.AspNetCore.Mvc;

namespace BozoAIAggregator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        // GET: api/Health
        [HttpGet]
        public IActionResult Get()
        {
             return Ok(new { status = "Healthy" });
        }
    }
}
