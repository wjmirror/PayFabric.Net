using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Linq;
using PayFabric.Net.Models;

using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;




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


            _walletService = TestServices.ServiceProvider.GetService<IWalletService>();
            _paymentService = TestServices.ServiceProvider.GetService<IPaymentService>();

            _cardA = new Card
            {
                Customer = "TESTAUTO",
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

            _cardB = new Card
            {
                Customer = "TESTAUTO",
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
                DocumentHead = new LevelTwoData
                {
                    DutyAmount = 100M,
                    FreightAmount = 110M,
                    OrderDate = DateTime.Now,
                    PONumber = "Po0013",
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

            Assert.AreEqual(_cardA.CardHolder.FirstName, card.CardHolder.FirstName);

            var removeResult = await this._walletService.Remove(id);
            Assert.AreEqual(true, removeResult);

        }

        [TestMethod]
        public async Task TestSuccess_Wallet_Service_IsSuccess()
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
            Assert.AreEqual(_cardB.CardHolder.FirstName, card.CardHolder.FirstName);

            //Update Card B
            var patchB = new Card()
            {
                ID = Guid.Parse(idB),
                CardHolder = new CardHolder
                {
                    FirstName = "Jim",
                },
                ExpirationDate = "0625"
            };
            var updateResultB = await this._walletService.Update(patchB);
            Assert.AreEqual(true, updateResultB.Success);

            //Retrieve cardB again 
            card = await this._walletService.Get(idB);
            Assert.IsNotNull(card);
            Assert.AreEqual("Jim", card.CardHolder.FirstName);
            Assert.AreEqual("0625", card.ExpirationDate);

            //Charge on cardB 
            var cb = new Card()
            {
                ID = Guid.Parse(idB),
                Cvc = _cardB.Cvc,
                Customer = _cardB.Customer
            };
            var chargeResult = await this._paymentService.Sale(105M, "USD", cb, _extendedInformation);
            Assert.AreEqual(chargeResult.Success, true);

            //Lock B 
            var lockResult = await this._walletService.Lock(idB, "Test Lock");
            Assert.AreEqual(true, lockResult);

            //Charge on card B again 
            cb = new Card()
            {
                ID = Guid.Parse(idB),
                Cvc = _cardB.Cvc,
                Customer = _cardB.Customer
            };
            chargeResult = await this._paymentService.Sale(115M, "USD", cb, _extendedInformation);
            Assert.AreEqual(chargeResult.Success, true);

            //Unlock B 
            var unlockResult = await this._walletService.Unlock(idB);
            Assert.AreEqual(true, unlockResult);


            //Get all cards of customer TESTAUTO
            var cards = await this._walletService.GetByCustomer("TESTAUTO");
            Assert.IsNotNull(cards);
            Assert.IsNotNull(cards.Where(e => e.ID == Guid.Parse(idB)).FirstOrDefault());


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
                    City = "Wheton",
                    Country = "USA",
                    Line1 = "218 Esat Avenue",
                    State = "IL",
                    Zip = "60139",
                    Email = "Jon@johny.com"
                },
                IsDefaultCard = true,
                IsSaveCard = true,
                Tender = TenderTypeEnum.CreditCard
            };

            var chargeResult = await this._paymentService.Sale(105M, "USD", testCard, _extendedInformation);
            Assert.IsTrue(chargeResult.Success);

            var cards = await this._walletService.GetByCustomer(testCard.Customer);
            Assert.IsNotNull(cards);
            Assert.IsTrue(cards.Count > 0);

            //cleanup
            foreach (var card in cards)
            {
                if (card.IsLocked.GetValueOrDefault())
                {
                    await this._walletService.Unlock(card.ID.ToString());
                }
                await this._walletService.Remove(card.ID.ToString());
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
                foreach (var card in cards)
                {
                    if (card.IsLocked.GetValueOrDefault())
                    {
                        await this._walletService.Unlock(card.ID.ToString());
                    }
                    await this._walletService.Remove(card.ID.ToString());
                }
            }

        }
    }
}
