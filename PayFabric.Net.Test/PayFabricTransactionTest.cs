using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PayFabric.Net.Models;
using System.Threading.Tasks;

namespace PayFabric.Net.Test
{
    [TestClass]
    public class PayFabricTransactionServiceTest
    {
        private decimal _amount;
        private string _currency;
        private Card _badcard;
        private Card _goodcard;
        private ExtendedInformation _extendedInformation;
        private Address _address;

        private IPaymentService _paymentService = null;
        private ITransactionService _transactionService = null;
        private bool isAuthorizeNet = false;

        [TestInitialize]
        public void TestInit()
        {
            TestServices.InitializeService();

            var _payFabricOptions = TestServices.ServiceProvider.GetService<IOptions<PayFabricOptions>>().Value;
            _paymentService = TestServices.ServiceProvider.GetService<IPaymentService>();
            _transactionService = TestServices.ServiceProvider.GetService<ITransactionService>();

            if (string.Compare(_payFabricOptions.SetupId, "AuthorizeNet", true) == 0)
                isAuthorizeNet = true;

            _amount = 40.0M;
            _currency = "USD";
            _address = new Address
            {
                City = "Wheaton",
                Country = "USA",
                Email = "swarup.sinha@spray.com"
            };

            _badcard = new Card
            {
                CardHolder = new CardHolder
                {
                    FirstName = "PantsON",
                    LastName = "Fire",
                },
                Account = "4583194798565295",
                Cvc = "532",
                ExpirationDate = "0925",
                Billto = new Address
                {
                    City = "Wheton",
                    Country = "USA",
                    Line1 = "218 Esat Avenue",
                    State = "IL",
                    Zip = "60139",
                    Email = "Jon@johny.com"
                }
            };

            _goodcard = new Card
            {
                CardHolder = new CardHolder
                {
                    FirstName = "PantsON",
                    LastName = "Fire",
                },

                Account = "4111111111111111",
                Cvc = "532",
                ExpirationDate = "0925",
                Billto = new Address
                {
                    City = "wheaton",
                    Country = "USA",
                    Line1 = "1953 Wexford Cir",
                    State = "IL",
                    Zip = "60189",
                    Email = "Jon@johny.com"
                }
            };

            _extendedInformation = new ExtendedInformation
            {
                Customer = "TESTAUTO",
                InvoiceNumber = "Inv0001",
                DocumentHead = new LevelTwoData
                {
                    DutyAmount = 100M,
                    FreightAmount = 110M,
                    OrderDate = DateTime.Now,
                    PONumber= "Po0013",
                    TaxAmount = 43.98M,
                }
            };

        }


        [TestMethod]
        public async Task TestSuccess_Create_Update_Process_TransactionIsSuccess()
        {
            // Create transaction 
            string key = await this._transactionService.CreateTransaction((tran) =>
            {
                tran.Amount = _amount;
                tran.Card = _goodcard;
                tran.Type = TransactionType.Sale.ToString("g");
                tran.Customer = "TEST-TRANSACTION-001";
                tran.Currency = "USD";
                tran.Document = new Document
                {
                    Head = new List<NameValue>
                    {
                        new NameValue
                        {
                            Name="InvoiceNumber",
                            Value="TEST_INV_" + DateTime.Now.ToString("yyyyMMdd_HHmmss.fffff")
                        }
                    },
                    UserDefined = new List<NameValue> {
                        new NameValue
                        {
                            Name="DisableEmailReceipt",
                            Value="True"
                        },
                        new NameValue
                        {
                            Name="Jim",
                            Value="Wang"
                        }
                    }
                };
            });

            Assert.IsNotNull(key);

            //Get transaction 
            var transaction = await this._transactionService.GetTransaction(key);
            Assert.IsNotNull(transaction);
            Assert.AreEqual(_amount, transaction.Amount);

            //Update transaction 
            bool updateResult = await this._transactionService.UpdateTransaction(tran => 
            {
                tran.Key = key;
                tran.Amount = 215M;
            });
            Assert.AreEqual(true, updateResult);

            //Process transaction 
            var tranResponse = await this._transactionService.ProcessTransaction(tran =>
            {
                tran.Key = key;
                tran.Card = new Card
                {
                    Cvc = _goodcard.Cvc
                };
            });
            Assert.IsNotNull(tranResponse.TransactionResponse);
            Assert.AreEqual(TransactionStatus.Approved, tranResponse.TransactionStatus); 

        }

        

    }
}
