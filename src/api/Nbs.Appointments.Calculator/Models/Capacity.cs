namespace Nbs.Appointments.Calculator.Models;
public class Capacity
{
    public Dictionary<string, int> Available { get; set; } = new Dictionary<string, int>();

    public List<UnAvailable> NotAvailable { get; set; } = new List<UnAvailable>();
}

public class UnAvailable 
{
    public UnAvailable(string[] services, int occurance)
    {
        Services = services;
        Occurance = occurance;
    }

    public string[] Services { get; set; }
    public int Occurance { get; set; }
}
