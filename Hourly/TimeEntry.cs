using Azure;
using Azure.Data.Tables;

namespace Hourly
{
    public class TimeEntry : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public DateTimeOffset? PunchedTime { get; set; }

        public TimeEntry()
        {
        }

        public TimeEntry(string partitionKey, string rowKey)
        {
            this.PartitionKey = partitionKey;
            this.RowKey = rowKey;
        }
    }
}
