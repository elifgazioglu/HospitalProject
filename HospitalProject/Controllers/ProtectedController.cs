using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProtectedController : ControllerBase
    {
        // Bu endpoint JWT ile kimlik doğrulama gerektirir.
        [Authorize]
        [HttpGet("protected")]
        public ActionResult<string> GetProtected()
        {
            return "This is a protected resource";
        }
    }
}
