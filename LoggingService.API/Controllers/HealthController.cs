using Microsoft.AspNetCore.Mvc;

namespace LoggingService.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok("Logging Service is healthy!");
    }
}