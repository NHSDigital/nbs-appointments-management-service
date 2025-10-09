namespace JobRunner.Job.Notify;

public interface ISendTracker
{
    Task<bool> HasSuccessfulSent(string reference, string type, string templateId);
    Task RecordSend(string reference, string type, string templateId, bool success, string message);
    Task RefreshState(string file);
    Task Persist(string file);
}
