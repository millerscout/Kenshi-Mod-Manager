using Core;
using Core.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

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

