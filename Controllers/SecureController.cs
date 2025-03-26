using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MF_SecureApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requires authentication
    public class SecureController : ControllerBase
    {
        [HttpGet("data")]
        public IActionResult GetData()
        {
            return Ok(new { Message = "This is secure data..!!!!" });
        }
    }
}