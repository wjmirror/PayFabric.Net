using PayFabric.Net.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Threading.Tasks;

namespace PayFabric.Net
{
    public interface IWalletService
    {
        Task<WalletTransactionResponse> Create(Card card);
        /// <summary>
        /// Update the wallet card entry.
        /// Note: Only specify the ID and the properties that need to be updated.
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        Task<WalletTransactionResponse> Update(Card card);
        Task<Card> Get(string Id);
        Task<ICollection<Card>> GetByCustomer(string customerNumber);
        Task<bool> Lock(string Id, string lockReason);
        Task<bool> Unlock(string Id);
        Task<bool> Remove(string Id);
    }
}
