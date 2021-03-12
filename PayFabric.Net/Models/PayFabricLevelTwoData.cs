using System;
using System.Collections.Generic;
using System.Text;

namespace PayFabric.Net.Models
{
    public class PayFabricLevelTwoData
    {
        public string InvoiceNumber { get; set; }
        public string PONumber { get; set; }
        public string DiscountAmount { get; set; }
        public string DutyAmount { get; set; }
        public string FreightAmount { get; set; }
        public string HandlingAmount { get; set; }
        public string TaxExempt { get; set; }
        public string TaxAmount { get; set; }
        public string ShipFromZip { get; set; }
        public string ShipToZip { get; set; }
        public string OrderDate { get; set; }
        public string VATTaxAmount { get; set; }
        public string VATTaxRate { get; set; }
    }
}
