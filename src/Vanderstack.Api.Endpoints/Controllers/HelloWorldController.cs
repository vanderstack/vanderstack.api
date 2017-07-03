using Microsoft.AspNetCore.Mvc;

namespace Vanderstack.Api.Endpoints.Controllers
{
    [Route("api/[controller]")]
    public class HelloWorldController : Controller
    {
        [HttpGet]
        public ActionResult Get()
        {
            return Ok("Hello World");
        }
    }
}
