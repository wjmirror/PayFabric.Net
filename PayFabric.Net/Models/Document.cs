using System;
using System.Collections.Generic;
using System.Text;

namespace PayFabric.Net.Models
{
    public partial class Document
    {
        public ICollection<PayFabricNameValue> Head { get; set; }
        public ICollection<DocumentLine> Lines { get; set; }

        public string UserDefined { get; set; }

        public PayFabricAddress DefaultBillTo { get; set; }
    }
}
