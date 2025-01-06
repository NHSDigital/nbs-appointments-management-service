using Parquet.Schema;
using Parquet;

namespace BookingDataExtractTests;

public class BookingExtactDataRow
{
    public string ODS_CODE { get; set; }

    public string NHS_NUMBER { get; set; }

    public string APPOINTMENT_DATE_TIME {  get; set; }

    public string APPOINTMENT_STATUS { get; set; }

    public bool SELF_REFERRAL { get; set; }

    public string DATE_OF_BIRTH { get; set; }

    public string BOOKING_REFERENCE_NUMBER { get; set; }

    public string SERVICE { get; set; }

    public string CREATED_DATE_TIME { get; set; }

    public double LATITUDE { get; set; }

    public double LONGITUDE { get; set; }

    public string SITE_NAME { get; set; }

    public string SOURCE { get; set; }

    public string REGION { get; set; }

    public string ICB { get; set; }
}
