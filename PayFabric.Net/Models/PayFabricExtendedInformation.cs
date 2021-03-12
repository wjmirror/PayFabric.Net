using System;
using System.Collections.Generic;
using System.Text;

namespace PayFabric.Net.Models
{
    public class PayFabricExtendedInformation
    {
        //public PayFabricLevelTwoData Head { get; set; }
        public List<PayFabricProduct> Lines { get; set; }
        public List<PayFabricNameValue> Head { get; set; }
    }
   
}
