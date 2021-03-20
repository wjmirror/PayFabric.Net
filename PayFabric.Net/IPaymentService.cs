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

        /// <summary>
        /// PreAuthorize transaction reserve of a specified amount on the customer’s credit card or account
        /// </summary>
        /// <param name="amount">The amount to reserve.</param>
        /// <param name="currency">The currency of the amount.</param>
        /// <param name="card">The credit card information.</param>
        /// <param name="extInfo">The extension information.</param>
        /// <returns>The ServiceNetResponse object, include the http status, raw response and transaction response</returns>
        Task<ServiceNetResponse> PreAuthorize(decimal amount, string currency, Card card, ExtendedInformation extInfo);

        /// <summary>
        /// Capture transaction will attempt to execute and finalize (capture) a pre-authorized transaction with specific amount, if Amount is null, it will capture with authorized amount. if Amount is provoided, it could be able to capture an authorization transaction multiple times, which depends on what gateway been used. (Note: Following gateways support multiple captures, Authorize.Net, USAePay & Payeezy(aka First Data GGE4).)
        /// </summary>
        /// <param name="transactionKey">The transaction key returned from the Authorization transaction. </param>
        /// <param name="amount">The capture amount.  If Amount is null, it will capture with authorized amount. </param>
        /// <param name="extInfo">The extension infomation, usaully be null </param>
        /// <returns>The ServiceNetResponse object, include the http status, raw response and transaction response</returns>
        Task<ServiceNetResponse> Capture( string transactionKey, decimal? amount, ExtendedInformation extInfo);

        /// <summary>
        /// Void transaction attempt to cancel a transaction that has already been processed successfully with a payment gateway, but before settlement with the bank, if cancellation is not possible a refund (credit) must be performed.
        /// </summary>
        /// <param name="transactionKey"></param>
        /// <param name="extInfo">The extension information, usualy be null.</param>
        /// <returns>The ServiceNetResponse object, include the http status, raw response and transaction response</returns>
        Task<ServiceNetResponse> Void(string transactionKey,  ExtendedInformation extInfo);


        /// <summary>
        /// Refund transaction will attempt to credit a transaction that has already been submitted to a payment gateway and has been settled from the bank. PayFabric attempts to submit a CREDIT transaction for the same exact amount as the original SALE transaction.
        /// </summary>
        /// <param name="transactionKey">The previous settled transaction key.</param>
        /// <param name="amount">Amount to refund, for Payfabric, this amount must be null, since it does not support partial refund. </param>
        /// <param name="extInfo">The extension informaiont, usually be null.</param>
        /// <returns>The ServiceNetResponse object, include the http status, raw response and transaction response</returns>
        Task<ServiceNetResponse> Refund(string transactionKey, decimal? amount, ExtendedInformation extInfo);

        /// <summary>
        /// A Refund is issued to transfer money from the company’s account to the customer’s account or credit card.
        /// </summary>
        /// <param name="amount">The amount to refund</param>
        /// <param name="currency">the currency of the amount</param>
        /// <param name="card">the credit card information</param>
        /// <param name="extInfo">the extension information</param>
        /// <returns>The ServiceNetResponse object, include the http status, raw response and transaction response</returns>
        Task<ServiceNetResponse> Credit(decimal amount, string currency, Card card, ExtendedInformation extInfo);

        /// <summary>
        /// Force transaction is to enter an already approved authorization/transaction. A Force is typically used for capturing a phone or voice authorization. When entering a Force you will be required to enter the authorization code.
        /// </summary>
        /// <param name="authorizationCode">the previous authorization code</param>
        /// <param name="amount">amount</param>
        /// <param name="currency">the currency of the amount</param>
        /// <param name="card">The credit card</param>
        /// <param name="extInfo">the extension information</param>
        /// <returns>The ServiceNetResponse object, include the http status, raw response and transaction response</returns>
        Task<ServiceNetResponse> Force(string authorizationCode, decimal amount, string currency, Card card, ExtendedInformation extInfo);
    }
}
