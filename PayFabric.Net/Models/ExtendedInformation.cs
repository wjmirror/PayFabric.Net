using System;
using System.Collections.Generic;
using System.Text;

namespace PayFabric.Net.Models
{
    public partial class ExtendedInformation
    {
        public string ReferenceTransactionKey { get; set; }
        public string OrderId { get; set; }
        public string Customer { get; set; }
        public String InvoiceNumber { get; set; }
        public String InvoiceDescription { get; set; }
        public string BatchNumber { get; set; }
        public PayFabricLevelTwoData LevelTwoData { get; set; }
        public ICollection<PayFabricProduct> LevelThreeData { get; set; }
        public Dictionary<string, object> ExtentionInformation { get; set; }
    }
}
