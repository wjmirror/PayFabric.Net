using PayFabric.Net.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PayFabric.Net
{
    /// <summary>
    /// PayFabric transaction service
    /// </summary>
    public interface ITransactionService
    {
        /// <summary>
        /// Create and save a transaction on Payfabric server.
        /// </summary>
        /// <param name="setupTransaction">the callback function to setup the transaction.</param>
        /// <returns></returns>
        Task<string> CreateTransaction(Action<Transaction> setupTransaction);


        /// <summary>
        /// update a transaction with new information.
        /// </summary>
        /// <param name="setupTransaction">the callback function to setup the transaction. 
        /// Note: only the property specified as Non-Null value will be updated.</param>
        /// <returns></returns>
        Task<bool> UpdateTransaction(Action<Transaction> setupTransaction);

        /// <summary>
        /// Retrieve a specified transaction
        /// </summary>
        /// <param name="key">The key of the specified transaction.</param>
        /// <returns></returns>
        Task<Transaction> GetTransaction(string key);


        /// <summary>
        /// Create a transaction on Payfabric server and immediately process it with payment gateway.
        /// </summary>
        /// <param name="setupTransaction">the callback function to setup the transaction.</param>
        /// <returns>The transaction process response</returns>
        Task<ServiceNetResponse> CreateProcessTransaction(Action<Transaction> setupTransaction);


        /// <summary>
        /// Process a saved transaction with payment gateway.
        /// </summary>
        /// <param name="setupTransaction">The Transaction.Key must be set in the setupTransaction call back function.</param>
        /// <returns>The transaction process response</returns>
        Task<ServiceNetResponse> ProcessTransaction(Action<Transaction> setupTransaction);

        /// <summary>
        /// Process a reference transaction, Capture, Refund, Void transaction call this function.
        /// </summary>
        /// <param name="setupTransaction">Setup the transaction, the <see cref="TransactionType">transaction type</see> and ReferenceKey must be set in the setupTransaction call back function. </param>
        /// <returns></returns>
        Task<ServiceNetResponse> ProcessReferenceTransaction(Action<Transaction> setupTransaction);

    }
}
