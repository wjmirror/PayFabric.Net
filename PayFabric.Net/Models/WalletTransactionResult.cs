using System;
using System.Collections.Generic;
using System.Text;

namespace PayFabric.Net.Models
{
    public class WalletTransactionResult
    {
        public bool Success { get; set; }
        public string Id { get; set; }
        public string ProcessMessage { get; set; }
    }
}
