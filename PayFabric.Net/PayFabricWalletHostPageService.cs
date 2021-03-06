using PayFabric.Net.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PayFabric.Net
{
    public class PayFabricWalletHostPageService : IPayFabricWalletHostPageService
    {
        public string GetCreateWalletEntryUrl(string customerNumber, TenderTypeEnum walletEntry)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetSecurityToken()
        {
            throw new NotImplementedException();
        }

        public string GetUpdateWalletEntryUrl(string cardId, Dictionary<string, string> options)
        {
            throw new NotImplementedException();
        }
    }


    public interface IPayFabricWalletHostPageService
    {
        Task<string> GetSecurityToken();
        string GetCreateWalletEntryUrl(string customerNumber, TenderTypeEnum walletEntry);
        string GetUpdateWalletEntryUrl(string cardId, Dictionary<string, string> options);
    }
}
