using System;
using System.Collections.Generic;
using System.Text;

namespace PayFabric.Net.Models
{
    public partial class LevelThreeData
    {

        public string ItemCommodityCode { get; set; }
        public string ItemProdCode { get; set; }
        public string ItemUPC { get; set; }
        public string ItemUOM { get; set; }
        public string ItemDesc { get; set; }
        public decimal? ItemAmount { get; set; }
        public decimal? ItemCost { get; set; }
        public decimal? ItemDiscount { get; set; }
        public decimal? ItemFreightAmount { get; set; }
        public decimal? ItemHandlingAmount { get; set; }
        public decimal? ItemQuantity { get; set; }
    }
}
