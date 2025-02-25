using Microsoft.AspNetCore.Http;

namespace Nhs.Appointments.Api.Models;
public record BulkImportRequest(IFormFile File, string Type);
