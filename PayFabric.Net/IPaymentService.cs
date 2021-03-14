using PayFabric.Net.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PayFabric.Net
{
    public interface IPaymentService
    {
        /// <summary>
        /// Sales transaction (aka Charge) is an immediate charge to the customer’s credit card or account. Money will not be moved until settlement has occurred. A Sale can only be reversed with a Void or a Refund. A Sale transaction does the same thing regardless of it being a credit card transaction, an eCheck transaction, or an ACH transaction.
        /// </summary>
        /// <param name="amount">The charge amount.</param>
        /// <param name="currency">Currency of the amout.</param>
        /// <param name="card">Credit Card / Echeck information.</param>
        /// <param name="extInfo">Extension information</param>
        /// <returns>The ServiceNetResponse object, include the http status, raw response and transaction response</returns>
        Task<ServiceNetResponse> Sale(decimal amount, string currency, Card card, ExtendedInformation extInfo);


        Task<ServiceNetResponse> PreAuthorize(decimal amount, string currency, Card card, ExtendedInformation extInfo);
        Task<ServiceNetResponse> Capture( string transactionKey, decimal? amount, ExtendedInformation extInfo);
        Task<ServiceNetResponse> Void(string transactionKey,  ExtendedInformation extInfo);
        Task<ServiceNetResponse> Refund(string transactionKey, decimal? amount, ExtendedInformation extInfo);
        Task<ServiceNetResponse> Credit(decimal amount, string currency, Card card, ExtendedInformation extInfo);
    }
}
