using System;
using System.Collections.Generic;
using System.Text;

namespace PayFabric.Net.Models
{
    /// <summary>
    /// The transaction status
    /// </summary>
    public enum TransactionStatus
    {
        /// <summary>
        /// Initial status
        /// </summary>
        Unknown,

        /// <summary>
        /// The process has not yet processed with payment gateway
        /// </summary>
        UnProcess,

        /// <summary>
        /// The transaction is approved
        /// </summary>
        Approved,

        /// <summary>
        /// The transaction is denied
        /// </summary>
        Denied,

        /// <summary>
        /// The transation is declined by bank
        /// </summary>
        Declined,

        /// <summary>
        /// The transaction is failed to process
        /// </summary>
        Failure,

        /// <summary>
        /// The address verification failed.
        /// </summary>
        AVSFailure
    }
}
