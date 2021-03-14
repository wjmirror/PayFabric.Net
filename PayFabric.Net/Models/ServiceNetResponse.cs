using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace PayFabric.Net.Models
{
    public partial class ServiceNetResponse
    {
        /// <summary>
        /// Indicate if the transaction call success
        /// </summary>
        public bool Success { get; set; } 

        /// <summary>
        /// The http status code 
        /// </summary>
        public string ResponseCode { get; set; }

        /// <summary>
        /// The transaction status, <see cref="TransactionStatus"/>
        /// </summary>
        public TransactionStatus TransactionStatus { get; set; }
        /// <summary>
        /// The raw http response message.
        /// </summary>
        public HttpResponseMessage RawResponse { get; set; } 

        /// <summary>
        /// The transaction response returned from PayFabric
        /// </summary>
        public TransactionResponse TransactionResponse { get; set; } 

        /// <summary>
        /// The exception during the transaction process.
        /// </summary>
        public Exception ServiceException { get; set; }
    }
}
