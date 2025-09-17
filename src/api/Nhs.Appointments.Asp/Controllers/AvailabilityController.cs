using Microsoft.AspNetCore.Mvc;

namespace Nhs.Appointments.Asp.Controllers;

[ApiController]
public class AvailabilityController : ControllerBase
{
    [Route("availability/query")]
    public IActionResult Query()
    {
        return Ok();
    }
}
