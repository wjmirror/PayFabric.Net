using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using PayFabric.Net.Mapper;
using PayFabric.Net.Models;
using SSCo.PaymentService.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace PayFabric.Net.Test
{
    [TestClass]
    public class PayFabricPayloadMapperTest
    {
        private ILogger<PayFabricPaymentService> logger = null;
        private IOptions<PayFabricOptions> payFabricOptions = null;
        private PayFabricPaymentService paymentService = null;
        [TestInitialize]
        public void TestInit()
        {
            TestServices.InitializeService();
            logger = TestServices.GetLogger<PayFabricPaymentService>();
            payFabricOptions = TestServices.ServiceProvider.GetService<IOptions<PayFabricOptions>>();
            paymentService = new PayFabricPaymentService(payFabricOptions,logger);
        }
        [TestMethod]
        public void MapToPayFabricPayload_GoodData()
        {
            PayFabricPayloadMapper payFabricPayloadMapper = new PayFabricPayloadMapper();
            PayFabricPayload payLaod = payFabricPayloadMapper.MapToPayFabricPayload(29.12M, "USD", GetCard(), GetExtendInfo(), "Book", payFabricOptions.Value );
            string payloadstring= JsonConvert.SerializeObject(payLaod);
            Assert.IsNotNull(payLaod);
        }

        [TestMethod]
        public void PreAuthorize_Valid_Ctreaditcard()
        {
            ServiceNetResponse serviceNetResponse = paymentService.PreAuthorize(28.98M, "USD", GetCard(), GetExtendInfo()).Result;
            Assert.IsNotNull(serviceNetResponse);
            Assert.IsTrue(serviceNetResponse.Success);
            Assert.IsTrue(serviceNetResponse.Transaction.Status== TransactionStatus.Approved);
        }

        [TestMethod]
        public void Charge_Valid_CreaditCard()
        {
            ServiceNetResponse serviceNetResponse = paymentService.Charge(30.98M, "USD", GetCard(), GetExtendInfo()).Result;
            Assert.IsNotNull(serviceNetResponse);
            Assert.IsTrue(serviceNetResponse.Success);
            Assert.IsTrue(serviceNetResponse.Transaction.TransactionKey.Length>3);
            Assert.IsTrue(serviceNetResponse.Transaction.Status== TransactionStatus.Approved);
        }

        [TestMethod]
        public void Void_Transaction()
        {
            ServiceNetResponse serviceNetResponse = paymentService.Void ("19062000283069", null).Result;
            Assert.IsNotNull(serviceNetResponse);
            //Assert.IsTrue(serviceNetResponse.Success);
            //Assert.IsTrue(serviceNetResponse.Transaction.ResultMessage.ToLower() == "declined");
        }

        [TestMethod]
        public void Capture_Transaction()
        {
            ServiceNetResponse serviceNetResponse = paymentService.Capture("19062000283069", 22.8M,null).Result;
            Assert.IsNotNull(serviceNetResponse);
            //Assert.IsTrue(serviceNetResponse.Success);
            //Assert.IsTrue(serviceNetResponse.Transaction.ResultMessage.ToLower() == "declined");
        }

        [TestMethod]
//        [ExpectedException(typeof(ArgumentException))]
        [ExpectedException(typeof(AggregateException))]
        public void Credit_Transaction_With_Amount()
        {
            ServiceNetResponse serviceNetResponse = paymentService.Credit("19062000283069", 22.8M, null).Result;
            Assert.IsNotNull(serviceNetResponse);
            //Assert.IsTrue(serviceNetResponse.Transaction.ResultMessage.ToLower() == "declined");
        }

        [TestMethod]
        public void Credit_Transaction()
        {
            ServiceNetResponse serviceNetResponse = paymentService.Credit("19062000283069", null, null).Result;
            Assert.IsNotNull(serviceNetResponse);
            //Assert.IsTrue(serviceNetResponse.Success);
            //Assert.IsTrue(serviceNetResponse.Transaction.ResultMessage.ToLower() == "declined");
        }

        [TestMethod]
        public void Refund_Transaction()
        {
            ServiceNetResponse serviceNetResponse = paymentService.Refund( 22.8M, "USD",GetCard(), null).Result;
            Assert.IsNotNull(serviceNetResponse);
            //Assert.IsTrue(serviceNetResponse.Success);
            //Assert.IsTrue(serviceNetResponse.Transaction.ResultMessage.ToLower() == "declined");
        }

        [TestMethod]
        public void Void_ParseResponse()
        {
            string processResponse = @"{'AVSAddressResponse':null,'AVSZipResponse':null,'AuthCode':null,'CVV2Response':null,'IAVSAddressResponse':null,'Message':'Unable to process the transaction.Invalid reference transaction, which is only valid for approved transaction.','OriginationID':null,'PayFabricErrorCode':'1000','RespTrxTag':null,'ResultCode':null,'Status':'Failure','TAXml':null,'TerminalID':null,'TerminalResultCode':null,'TrxDate':null,'TrxKey':null}";
            PayFabricResponse pResponse = JsonConvert.DeserializeObject<PayFabricResponse>(processResponse);
            Assert.IsNotNull(pResponse);
        }




        private ExtendedInformation GetExtendInfo()
        {
            return new ExtendedInformation
            {
                Customer = "ABC",
                InvoiceNumber = "ZX00943",
                LevelTwoData = new LevelTwoData
                {
                    DutyAmount = 24.79M,
                    FreightAmount = 7.99M,
                    OrderDate = DateTime.Now,
                    PurchaseOrder = "P0001334",
                    TaxAmount = 6.12M,
                }
            };
        }

        private Card GetCard()
        {
            return new Card 
            {
                FirstName ="John",
                LastName="Johny",
                Number= "378282246310005",
                Cvv="532",
                ExpirationDate="0925",
                Address = new Address
                {
                     City="Wheton",
                     Country="USA",
                     Line1="218 Esat Avenue",
                     State="IL",
                     Zip="60139",Email="Jon@johny.com"
                }
            };
        }
    }
}
    

