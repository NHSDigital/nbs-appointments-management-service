namespace Nbs.Appointments.Calculator.Models;
public class Capacity
{
    public Dictionary<string, int> Available { get; set; }

    public IEnumerable<UnAvailable> NotAvailable { get; set; }
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
