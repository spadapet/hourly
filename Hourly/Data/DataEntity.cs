using Azure;
using Azure.Data.Tables;
using Newtonsoft.Json;

namespace Hourly.Data;

public sealed class DataEntity : ITableEntity
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string Data { get; set; }

    public DataEntity()
    { }

    public DataEntity(string partitionKey, string rowKey, object data)
    {
        this.PartitionKey = partitionKey;
        this.RowKey = rowKey;
        this.Data = JsonConvert.SerializeObject(data);
    }

    public T Deserialize<T>()
    {
        return JsonConvert.DeserializeObject<T>(this.Data);
    }
}
