using System;
using System.Collections.Generic;
using System.Text;

namespace PayFabric.Net.Models
{
    public enum TransactionStatus
    {
        Unknown,
        Approved,
        Denied,
        Declined,
        Failure,
        AVSFailure
    }
}
