using Microsoft.AspNetCore.Mvc;

namespace Nhs.Appointments.Asp.Controllers;

[ApiController]
[Route("availability")]
public class AvailabilityController : ControllerBase
{
    [HttpGet("query")]
    public IActionResult Query()
    {
        //QueryAvailabilityFunction
        return Ok();
    }
}
