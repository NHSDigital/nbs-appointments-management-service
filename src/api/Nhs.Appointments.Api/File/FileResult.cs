using System.IO;

namespace Nhs.Appointments.Api.File;

public record FileResponse(string FileName, MemoryStream Content);
