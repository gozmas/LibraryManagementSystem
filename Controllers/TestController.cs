using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
       

        [HttpGet("public")]
        public IActionResult Public()
        {
            return Ok("This endpoint is public.");
        }

        [Authorize]
        [HttpGet("protected")]
        public IActionResult Protected()
        {
            return Ok("This endpoint requires a valid JWT token.");
        }

        [Authorize(Roles = "Member")]
        [HttpGet("member")]
        public IActionResult MemberOnly()
        {
            return Ok("This endpoint is only accessible by Member users.");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public IActionResult AdminOnly()
        {
            return Ok("This endpoint is only accessible by Admin users.");
        }
        
    }
}