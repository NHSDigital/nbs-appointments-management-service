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
        throw new NotImplementedException();
    }
    
    [HttpGet]
    public ActionResult QueryByReference()
    {
        //QueryBookingByReferenceFunction
        throw new NotImplementedException();
    }
    
    [HttpGet]
    public ActionResult QueryByNhsNumber()
    {
        //QueryBookingByNhsNumberFunction
        throw new NotImplementedException();
    }
    
    [HttpPost]
    public ActionResult MakeBooking()
    {
        //MakeBookingFunction
        throw new NotImplementedException();
    }
    
    [HttpPost]
    public ActionResult SetStatus()
    {
        //SetBookingStatusFunction
        throw new NotImplementedException();
    }
    
    [HttpPost]
    public ActionResult Cancel()
    {
        //CancelBookingFunction
        throw new NotImplementedException();
    }
}
