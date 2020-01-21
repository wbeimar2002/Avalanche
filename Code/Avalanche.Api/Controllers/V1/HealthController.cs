using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Avalanche.Api.Controllers.V1
{
    [Route("api/v1/[controller]")]
    public class HealthController : Controller
    {
        [Route("check")]
        [HttpGet]
        public IActionResult HealthCheck()
        {
            Console.WriteLine("Avalanche Api is healthy.");
            return new OkObjectResult(new
            {
                DateTime = DateTime.UtcNow
            });
        }
    }
}