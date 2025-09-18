using Microsoft.AspNetCore.Mvc;

namespace Nhs.Appointments.Asp.Controllers;

[ApiController]
[Route("booking")]
public class BookingController : ControllerBase
{
    [HttpGet]
    public ActionResult Query()
    {
        //QueryBookingsFunction
        return Ok();
    }
    
    [HttpGet]
    public ActionResult QueryByReference()
    {
        //QueryBookingByReferenceFunction
        return Ok();
    }
    
    [HttpGet]
    public ActionResult QueryByNhsNumber()
    {
        //QueryBookingByNhsNumberFunction
        return Ok();
    }
    
    [HttpPost]
    public ActionResult MakeBooking()
    {
        //MakeBookingFunction
        return Ok();
    }
    
    [HttpPost]
    public ActionResult SetStatus()
    {
        //SetBookingStatusFunction
        return Ok();
    }
    
    [HttpPost]
    public ActionResult Cancel()
    {
        //CancelBookingFunction
        return Ok();
    }
}
