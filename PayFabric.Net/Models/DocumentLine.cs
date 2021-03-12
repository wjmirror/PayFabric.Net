using System;
using System.Collections.Generic;
using System.Text;

namespace PayFabric.Net.Models
{
    public partial class DocumentLine
    {

        public ICollection<PayFabricNameValue> Columns { get; set; }
    }
}
