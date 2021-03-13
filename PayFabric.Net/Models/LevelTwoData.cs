using System;
using System.Collections.Generic;
using System.Text;

namespace PayFabric.Net.Models
{
    public class LevelTwoData
    {
        public string InvoiceNumber { get; set; }
        public string PONumber { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? DutyAmount { get; set; }
        public decimal? FreightAmount { get; set; }
        public decimal? HandlingAmount { get; set; }
        public bool? IsTaxExempt { get; set; }
        public decimal? TaxAmount { get; set; }
        public string ShipFromZip { get; set; }
        public string ShipToZip { get; set; }
        public DateTime? OrderDate { get; set; }
        public decimal? VATTaxAmount { get; set; }
        public decimal? VATTaxRate { get; set; }
    }
}
