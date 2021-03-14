using System;
using System.Collections.Generic;
using System.Text;

namespace PayFabric.Net.Models
{
    public class WalletTransactionResponse
    {
        public bool Success { get; set; }
        public string Id { get; set; }
        public string ProcessMessage { get; set; }
    }
}
