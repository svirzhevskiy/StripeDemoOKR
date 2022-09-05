using Microsoft.AspNetCore.Mvc;

namespace StripeDemoOKR
{
    [Route("home/")]
    public class HomeController : ControllerBase
    {
        [HttpGet("status")]
        public IActionResult CheckStatus()
        {
            return new OkObjectResult("Works fine!");
        }
    }
}