// See https://aka.ms/new-console-template for more information
using Nhs.Appointments.ApiClient.Auth;
using System.Text.Json;

Console.WriteLine("Hello, World!");

var requestSigner = new RequestSigningHandler(new RequestSigner(TimeProvider.System, "2EitbEouxHQ0WerOy3TwcYxh3/wZA0LaGrU1xpKg0KJ352H/mK0fbPtXod0T0UCrgRHyVjF6JfQm/LillEZyEA=="));
requestSigner.InnerHandler = new HttpClientHandler();
var http = new HttpClient(requestSigner);

var applyTemplateRequest = new
{
    site = "ABC01",
    from = "2025-01-01",
    until = "2025-12-31",
    template = new
    {
        days = new[] {"Monday",
            "Tuesday",
            "Wednesday",
            "Thursday",
            "Friday",
            "Saturday",
            "Sunday"}
    },
    sessions = new[] {
        new
        {
            from = "09:00",
            until = "17:00",
            slotLength = 5,
            capacity = 2,
            services = new[] {"COVID"}
        }
    }
};

var payload = JsonSerializer.Serialize(applyTemplateRequest);
var response = await http.PostAsync("http://localhost:7071/api/availability/apply-template", new StringContent(payload));
Console.WriteLine(response.StatusCode.ToString());