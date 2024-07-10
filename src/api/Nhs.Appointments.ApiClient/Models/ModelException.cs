namespace Nhs.Appointments.ApiClient.Models
{
    public class ModelException : Exception
    {
        public ModelException(string json, System.Text.Json.JsonException inner) : base($"Json could not be parsed: {json}", inner)
        {

        }
    }
}
