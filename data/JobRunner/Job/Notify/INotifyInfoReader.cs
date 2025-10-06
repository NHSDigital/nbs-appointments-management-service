namespace JobRunner.Job.Notify;

public interface INotifyInfoReader<T>
{
    Task<IEnumerable<T>> ReadStreamAsync(Stream stream);
}
