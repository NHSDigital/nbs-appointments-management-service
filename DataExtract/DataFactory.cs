using Parquet.Data;
using Parquet.Schema;
using System.Collections;

namespace DataExtract;

public abstract class DataFactory
{
    public abstract DataField Field { get; }

    public abstract DataColumn CreateColumn(IEnumerable data);
}

public class DataFactory<TDocument, TData>(string fieldName, Func<TDocument, TData> dataGenerator) : DataFactory
{
    private readonly Lazy<DataField<TData>> _field = new(() => new DataField<TData>(fieldName));

    public override DataField Field => _field.Value;

    public override DataColumn CreateColumn(IEnumerable data)
    {
        if (data is IEnumerable<TDocument> typedData)
        {
            return new DataColumn(_field.Value, typedData.Select(dataGenerator).ToArray());
        }

        throw new InvalidOperationException("Data is not the correct type");
    }

}
