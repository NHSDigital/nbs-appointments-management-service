namespace Nhs.Appointments.Jobs.ChangeFeed;

public interface IDataFilter<in IModel>
{
    bool IsValidItem(IModel model);
}
