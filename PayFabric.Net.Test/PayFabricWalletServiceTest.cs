using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using SSCo.PaymentService;
using SSCo.PaymentService.Models;
using System.Threading.Tasks;
using System.Linq;

#if NETSTANDARD
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
#endif

#if NET45
using Serilog;
using PayFabric.Net.Logging;
#endif

namespace PayFabric.Net.Test
{
    [TestClass]
    public class PayFabricWalletServiceTest
    {

        private Card _cardA;
        private Card _cardB;

        private IWalletService _walletService = null;
        private IPaymentService _paymentService = null;
        private ExtendedInformation _extendedInformation;

        #region test initialization

        [TestInitialize]
        public void TestInit()
        {
            TestServices.InitializeService();

#if NETSTANDARD
            _walletService = TestServices.ServiceProvider.GetService<IWalletService>();
            _paymentService = TestServices.ServiceProvider.GetService<IPaymentService>();
#endif

#if NET45

            PayFabricOptions  _payFabricOptions = TestServices.GetPayFabricOptions();
            ILogger _logger = TestServices.GetLogger<PayFabricPaymentService>();
            _walletService = new PayFabricWalletService(_payFabricOptions, _logger);
            _paymentService= new PayFabricPaymentService(_payFabricOptions, _logger);
#endif

            _cardA = new Card
            {
                Customer = "TESTAUTO",
                FirstName = "PantsON",
                LastName = "Fire",
                Number = "4583194798565295",
                Cvv = "532",
                ExpirationDate = "0925",
                Address = new Address
                {
                    City = "Wheton",
                    Country = "USA",
                    Line1 = "218 Esat Avenue",
                    State = "IL",
                    Zip = "60139",
                    Email = "Jon@johny.com"
                }
            };

            _cardB = new Card
            {
                Customer = "TESTAUTO",
                FirstName = "PantsON",
                LastName = "Fire",
                Number = "4111111111111111",
                Cvv = "532",
                ExpirationDate = "0925",
                Address = new Address
                {
                    City = "Wheton",
                    Country = "USA",
                    Line1 = "218 Esat Avenue",
                    State = "IL",
                    Zip = "60139",
                    Email = "Jon@johny.com"
                }
            };

            _extendedInformation = new ExtendedInformation
            {
                InvoiceNumber = "Inv0001",
                LevelTwoData = new LevelTwoData
                {
                    DutyAmount = 100M,
                    FreightAmount = 110M,
                    OrderDate = DateTime.Now,
                    PurchaseOrder = "Po0013",
                    TaxAmount = 43.98M,
                }
            };
        }

        #endregion

        /// <summary>
        /// Test all 3 Steps as following
        /// 1. Create
        /// 2. Get
        /// 3. Remove
        /// </summary>

        [TestMethod]
        public async Task TestSuccessCreate_Wallet_TransactionIsSuccess()
        {
            var result = await this._walletService.Create(_cardA);
            Assert.AreEqual(true, result.Success);
            Assert.IsNotNull(result.Id);

            var id = result.Id;

            Card card = await this._walletService.Get(id);
            Assert.IsNotNull(card);

            Assert.AreEqual(_cardA.FirstName, card.FirstName);

            var removeResult = await this._walletService.Remove(id);
            Assert.AreEqual(true, removeResult);

        }

        [TestMethod]
        public async Task TestSuccess_Wallet_APPERP_9628_IsSuccess()
        {
            //Create Card A
            var createResultA = await this._walletService.Create(_cardA);
            Assert.AreEqual(true, createResultA.Success);
            var idA = createResultA.Id;

            //Create Card B
            var createResultB = await this._walletService.Create(_cardB);
            Assert.AreEqual(true, createResultB.Success);
            var idB = createResultB.Id;

            //Retrieve CardB
            Card card = await this._walletService.Get(idB);
            Assert.IsNotNull(card);
            Assert.AreEqual(_cardB.FirstName, card.FirstName);

            //Update Card B
            var patchB = new Card()
            {
                Id = idB,
                FirstName = "Jim",
                ExpirationDate = "0625"
            };
            var updateResultB = await this._walletService.Update(patchB);
            Assert.AreEqual(true, updateResultB.Success);

            //Retrieve cardB again 
            card = await this._walletService.Get(idB);
            Assert.IsNotNull(card);
            Assert.AreEqual("Jim", card.FirstName);
            Assert.AreEqual("0625", card.ExpirationDate);

            //Charge on cardB 
            var cb = new Card()
            {
                Id = idB,
                Cvv = _cardB.Cvv,
                Customer = _cardB.Customer
            };
            var chargeResult = await this._paymentService.Charge(105M, "USD", cb, _extendedInformation);
            Assert.AreEqual(chargeResult.Success, true);

            //Lock B 
            var lockResult = await this._walletService.Lock(idB, "Test Lock");
            Assert.AreEqual(true, lockResult);

            //Charge on card B again 
            cb = new Card()
            {
                Id = idB,
                Cvv = _cardB.Cvv,
                Customer = _cardB.Customer
            };
            chargeResult = await this._paymentService.Charge(115M, "USD", cb, _extendedInformation);
            Assert.AreEqual(chargeResult.Success, true);

            //Unlock B 
            var unlockResult = await this._walletService.Unlock(idB);
            Assert.AreEqual(true, unlockResult);


            //Get all cards of customer TESTAUTO
            var cards = await this._walletService.GetByCustomer("TESTAUTO");
            Assert.IsNotNull(cards);
            Assert.IsNotNull(cards.Where(e => e.Id == idB).FirstOrDefault());


            //Remove Card A
            var removeResultA = await this._walletService.Remove(idA);
            Assert.AreEqual(true, removeResultA);

            //Remove CardB
            var removeResultB = await this._walletService.Remove(idB);
            Assert.AreEqual(true, removeResultB);
        }

        [TestMethod]
        public async Task TestSuccess_Create_Charge_SaveCard__IsSuccess()
        {
            var testCard = new Card
            {
                Customer = "TESTAUTO-3",
                FirstName = "PantsON",
                LastName = "Fire",
                Number = "4111111111111111",
                Cvv = "532",
                ExpirationDate = "0925",
                Address = new Address
                {
                    City = "Wheton",
                    Country = "USA",
                    Line1 = "218 Esat Avenue",
                    State = "IL",
                    Zip = "60139",
                    Email = "Jon@johny.com"
                },
                IsDefault = true,
                IsSaveCard = true
            };

            var chargeResult = await this._paymentService.Charge(105M, "USD", testCard, _extendedInformation);
            Assert.IsTrue(chargeResult.Success);

            var cards = await this._walletService.GetByCustomer(testCard.Customer);
            Assert.IsNotNull(cards);
            Assert.IsTrue(cards.Count > 0);

            //cleanup
            foreach (var card in cards)
            {
                if (card.IsLocked.GetValueOrDefault())
                {
                    await this._walletService.Unlock(card.Id);
                }
                await this._walletService.Remove(card.Id);
            }
        }

        [TestMethod]
        public async Task TestSuccess_UnLock_Remove_TESTAUTO_IsSuccess()
        {
            //Get all cards of customer TESTAUTO
            var cards = await this._walletService.GetByCustomer("TESTAUTO");
            Assert.IsNotNull(cards);

            if (cards.Count > 0)
            {
                foreach ( var card in cards)
                {
                    if (card.IsLocked.GetValueOrDefault())
                    {
                        await this._walletService.Unlock(card.Id);
                    }
                    await this._walletService.Remove(card.Id);
                }
            }

        }




    }
}
