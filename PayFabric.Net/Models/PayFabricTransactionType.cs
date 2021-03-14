using System;
using System.Collections.Generic;
using System.Text;

namespace PayFabric.Net.Models
{
    public enum PayFabricTransactionType
    {
        /// <summary>
        /// A Sale at one processor may be called Capture at another. An approved Sale is an immediate charge to the customer’s credit card or account. Money will not be moved until settlement has occurred. A Sale can only be reversed with a Void or a Refund. A Sale transaction does the same thing regardless of it being a credit card transaction, an eCheck transaction, or an ACH transaction.
        /// 
        /// A Credit Card Sale transaction can be reversed by issuing a Refund or Void.
        /// </summary>
        Sale,

        /// <summary>
        /// A Authorization may also be known as a Pre-Authorize, Pre-Authorization or Authorization. When dealing with credit card transactions a Authorization is the reserve of a specified amount on the customer’s credit card or account. A Authorization prevents the customer from using that portion of their credit / funds, but does not actually charge the card nor transfer any money. A Authorization is useful for companies that ship merchandise one or more days after receiving an order. By issuing a Authorization, a company reserves the necessary amount on the customer’s card at order placement time. As long as the Authorization transaction remains open an approved Capture transaction is guaranteed. A Capture transaction is necessary to complete the Authorization.
        /// 
        /// The number of days a Authorization will stay open is determined by each cardholder’s issuing bank. The most common number is seven to ten days, but some banks may hold Authorization for as long as four weeks and little as three days.
        /// 
        /// A Authorization transaction cannot be reversed. Issuing a Void for a Authorization transaction will not free up any money on the customer’s credit card. To free up the reserved money you would need to issue a Capture transaction and then Void that captured amount.
        /// 
        /// When dealing with eChecks and ACH, a Authorization is not supported.
        /// </summary>
        Authorization,

        /// <summary>
        /// A Capture may also be known as Capture or Delayed Capture. A Capture can only be issued for a transaction that previously has been a Authorization. Under ordinary circumstances, a Capture is assured approval as long as the amount is equal to or less than the original Authorization amount and the Capture transaction is sent before the Authorization has expired. A Capture results in an immediate charge to the customer’s credit card or account. If the Capture is for less than the original Authorization amount, the remainder of the original Authorization amount is released back to the customer’s credit line or account.
        /// 
        /// A Capture transaction can be reversed by issuing a Refund or Void.
        ///
        /// NOTE: Some gateways do not allow Capture transactions with amounts greater than the original Authorization amount. Please check with your gateway to see if that feature is available.
        /// </summary>
        Capture,

        /// <summary>
        /// A Refund is issued to transfer money from the company’s account to the customer’s account or credit card. There are two types of Refund transactions that you can issue: referenced Refund and non-referenced Refund. A referenced Refund occurs when you remove the payment line, or delete a sales document. To create a non-referenced Refund you need to create a brand new return document. Some payment gateways allow for the reversal of a Refund by issuing a Void if that transaction has not yet been settled. If the transaction has settled you will need to issue a Sale to reverse the Refund.
        /// 
        /// NOTE: Some gateways do not allow non-referenced Refund transactions to be processed. Please check with your gateway to see if that feature is available.
        /// </summary>
        Refund,

        /// <summary>
        /// A Force is used to enter an already approved authorization/transaction. A Force is typically used for capturing a phone or voice authorization. When entering a Force you will be required to enter the authorization code.
        /// 
        /// A Force transaction can be reversed by issuing a Refund or a Void.
        /// </summary>
        Force,

        /// <summary>
        /// A Void is issued for an unsettled approved transaction. When a Void is successfully issued, neither the Void nor the original transaction will appear on the customer’s statement. A Void can only be issued against an unsettled transaction. When a Void is sent, if the original transaction has already been settled, the Void will be denied and a warning will be displayed. A Credit Card settled Sale transaction must be reversed with a Refund.
        /// </summary>
        Void,


        /// <summary>
        /// A Verify is used to validate credit card number. It's not a payment, PayFabric won't settle Verify transactions, and unable to reverse Verify transactions. Verify transaction only supports in Create Transaction/Update Transaction/Process Transaction APIs, it is not available in portal and hosted payment page. Only EVO gateway supports Verify transaction with Credit Card payment method. PayFabric will set tranaction's Amount to 0.00 if anything other than 0.00 is passed through for Verify transaction.
        /// </summary>
        Verify
    }
}
