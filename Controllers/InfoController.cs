using BozoAIAggregator.Utility;
using Microsoft.AspNetCore.Mvc;

namespace BozoAIAggregator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InfoController : ControllerBase
    {
        // GET: api/Info
        [HttpGet]
        public IActionResult Get()
        {
            var buildDate = BuildDateAttribute.GetBuildDate();
            var response = new
            {
                status = "Healthy",
                buildDate = buildDate
            };
            return Ok(response);
        }
    }
}
