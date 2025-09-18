using Microsoft.AspNetCore.Mvc;

namespace Nhs.Appointments.Asp.Controllers;

[ApiController]
[Route("sites")]
public class SiteController : ControllerBase
{
    [HttpGet]
    public IActionResult GetSitesByArea()
    {
        //GetSiteMetaDataFunction
        return Ok();
    }
    
    [HttpGet("{site}/meta")]
    public IActionResult GetMetaData(string site)
    {
        //GetSitesByAreaFunction
        return Ok();
    }
}
