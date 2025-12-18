using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Nhs.Appointments.Jobs.BlobAuditor.Sink.Config;

namespace Nhs.Appointments.Jobs.BlobAuditor.Sink;

public class ItemExclusionProcessor(IOptions<List<SinkExclusion>> options) : IItemExclusionProcessor
{
    public JObject Apply(string source, JObject item)
    {
        var exclusion = options.Value?
            .FirstOrDefault(x =>
                x.Source.Equals(source, StringComparison.OrdinalIgnoreCase));

        if (exclusion is null || exclusion.ExcludedPaths.Count == 0)
            return item;

        var clone = (JObject)item.DeepClone();

        foreach (var path in exclusion.ExcludedPaths)
        {
            RemovePath(clone, path);
        }

        return clone;
    }

    private static void RemovePath(JObject clone, string path)
    {
        var tokens = clone.SelectTokens(path).ToList();

        foreach (var token in tokens)
        {
            switch (token.Parent)
            {
                case JProperty property:
                    property.Remove();
                    break;

                case JArray array:
                    array.Remove(token);
                    break;
            }
        }
    }
}
