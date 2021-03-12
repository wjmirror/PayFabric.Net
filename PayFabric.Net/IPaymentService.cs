using SSCo.PaymentService.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PayFabric.Net
{
    public interface IPaymentService
    {
        Task<PayFabricResponse> Charge(decimal amount, string currency, PayFabricCard card, ExtendedInformation extInfo);
        Task<PayFabricResponse> PreAuthorize(decimal amount, string currency, PayFabricCard card, ExtendedInformation extInfo);
        Task<PayFabricResponse> Capture( string transactionKey, decimal? amount, ExtendedInformation extInfo);
        Task<PayFabricResponse> Void(string transactionKey,  ExtendedInformation extInfo);
        Task<PayFabricResponse> Credit(string transactionKey, decimal? amount, ExtendedInformation extInfo);
        Task<PayFabricResponse> Refund(decimal amount, string currency, PayFabricCard card, ExtendedInformation extInfo);
    }
}
