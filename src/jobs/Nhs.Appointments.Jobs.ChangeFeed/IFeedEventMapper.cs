namespace Nhs.Appointments.Jobs.ChangeFeed;

public interface IFeedEventMapper<in IModel, out TEvent>
{
    IEnumerable<TEvent> MapToEvents(IEnumerable<IModel> feedItems);
}
