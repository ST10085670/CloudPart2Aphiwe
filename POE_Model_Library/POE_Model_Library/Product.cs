
using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;

namespace POE_Model_Library
{
    public class Product : ITableEntity
    {
        [Key]
        public int Product_ID { get; set; }
        public int Price { get; set; }
        public string? Product_Name { get; set; }
        public string? Product_Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? Stock_Level { get; set; }
        // ITableEnttiy Implementation

        public string? PartitionKey { get; set; }
        public string? RowKey { get; set; }
        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }

    }
}
