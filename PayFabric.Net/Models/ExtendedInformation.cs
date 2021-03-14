using System;
using System.Collections.Generic;
using System.Text;

namespace PayFabric.Net.Models
{
    public partial class ExtendedInformation
    {
        public string Customer { get; set; }
        public String InvoiceNumber { get; set; }
        public String InvoiceDescription { get; set; }
        public string BatchNumber { get; set; }
        public LevelTwoData DocumentHead { get; set; }
        public ICollection<LevelThreeData> DocumentLines { get; set; }
        public Dictionary<string, object> ExtentionInformation { get; set; }
    }
}
