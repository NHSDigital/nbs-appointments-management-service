using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Nhs.Appointments.Api.File;

public static class FileResponseWriter
{
    public static FileContentResult WriteResult(FileResponse result)
    {
        var contentResult = new FileContentResult(
            result.Content.ToArray(), 
            new MediaTypeHeaderValue("text/csv"));
        contentResult.FileDownloadName = result.FileName;
        return contentResult;
    }
}


