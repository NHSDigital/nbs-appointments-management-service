namespace CapacityDataExtracts.Documents;
public class SiteSessionParquet
{
    public string DATE { get; set; }
    public string TIME { get; set; }
    public int CAPACITY { get; set; }
    public string ODS_CODE { get; set; }
    public string SITE_NAME { get; set; }
    public string REGION { get; set; }
    public double LATITUDE { get; set; }
    public double LONGITUDE { get; set; }
    public string ICB { get; set; }
    public string[] SERVICE { get; set; }
}
