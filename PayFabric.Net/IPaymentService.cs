using PayFabric.Net.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PayFabric.Net
{
    public interface IPaymentService
    {
        Task<ServiceNetResponse> Charge(decimal amount, string currency, Card card, ExtendedInformation extInfo);
        Task<ServiceNetResponse> PreAuthorize(decimal amount, string currency, Card card, ExtendedInformation extInfo);
        Task<ServiceNetResponse> Capture( string transactionKey, decimal? amount, ExtendedInformation extInfo);
        Task<ServiceNetResponse> Void(string transactionKey,  ExtendedInformation extInfo);
        Task<ServiceNetResponse> Refund(string transactionKey, decimal? amount, ExtendedInformation extInfo);
        Task<ServiceNetResponse> Credit(decimal amount, string currency, Card card, ExtendedInformation extInfo);
    }
}
