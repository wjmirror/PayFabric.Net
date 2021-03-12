using System;
using System.Collections.Generic;
using System.Text;

namespace PayFabric.Net.Models
{
    public partial class PayFabricProduct
    {

        public string ItemCommodityCode { get; set; }
        public string ItemProdCode { get; set; }
        public string ItemUPC { get; set; }
        public string ItemUOM { get; set; }
        public string ItemDesc { get; set; }
        public string ItemAmount { get; set; }
        public string ItemCost { get; set; }
        public string ItemDiscount { get; set; }
        public string ItemFreightAmount { get; set; }
        public string ItemHandlingAmount { get; set; }
        public string ItemQuantity { get; set; }
    }
}
