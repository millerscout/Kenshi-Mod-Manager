using Core;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("Home")]
    public class HomeController : ControllerBase
    {
        public HomeController()
        {
            LoadService.Setup();
        }

        [HttpGet("index")]
        public IActionResult Get() => Ok(LoadService.GetListOfMods());
    }
}