using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using SSCo.PaymentService;
using SSCo.PaymentService.Models;

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
    public class PayFabricPaymentServiceTest
    {
        private decimal _amount;
        private string _currency;
        private Card _badcard;
        private Card _goodcard;
        private ExtendedInformation _extendedInformation;
        private Address _address;

        private IPaymentService _paymentService = null;
        private bool isAuthorizeNet = false;

        [TestInitialize]
        public void TestInit()
        {
            TestServices.InitializeService();

#if NETSTANDARD
            var _payFabricOptions = TestServices.ServiceProvider.GetService<IOptions<PayFabricOptions>>().Value;
            _paymentService = TestServices.ServiceProvider.GetService<IPaymentService>();
#else

           var  _payFabricOptions = TestServices.GetPayFabricOptions();
            _paymentService = new PayFabricPaymentService(_payFabricOptions, TestServices.GetLogger<PayFabricPaymentService>());
#endif
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

            _goodcard = new Card
            {
                FirstName = "PantsON",
                LastName = "Fire",
                Number = "4111111111111111",
                Cvv = "532",
                ExpirationDate = "0925",
                Address = new Address
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
                Customer = "ABC",
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

        #region  Success Test Cases

        /// <summary>
        /// Test all 3 Steps as following
        /// 1. Charge
        /// 2. Void
        /// </summary>

        [TestMethod]
        public void TestSuccessCreditCardTransaction_ChargeVoid_TransactionIsSuccess()
        {

            ExtendedInformation extInfo = new ExtendedInformation
            {
                Customer="TEST_0199999",
                InvoiceNumber = "TEST" + DateTime.Now.ToString("yyyyMMdd_HHmmss.fffff"),
                LevelTwoData=new LevelTwoData
                {
                    DiscountAmount=10M,
                    DutyAmount=110M,
                    TaxAmount=10M,
                    FreightAmount=5M,
                    ShipFromZip="60139",
                    ShipToZip="60189",
                    PurchaseOrder="PO_1235",
                    OrderDate=DateTime.Now
                }
            };
            //Test PreAuthorize method.
            ServiceNetResponse result = _paymentService.Charge(115M, _currency, _goodcard, extInfo).Result;

            Assert.AreEqual(result.Success, true);
            Assert.IsNotNull(result.Transaction);

            var tranResult = result.Transaction;

            //Assert.AreEqual(tranResult.ResultCode, "0");
            //Assert.AreEqual(tranResult.ResultMessage, TransactionStatus.Approved.ToString("g"), true);
            Assert.AreEqual(TransactionStatus.Approved, tranResult.Status);
            Assert.IsNotNull(tranResult.TransactionKey);

            string transactionKey = tranResult.TransactionKey;


            //Test Void method.
            //result = _paymentService.Void(transactionKey, _extendedInformation).Result;
            //Assert.AreEqual(result.Success, true);
            //Assert.IsNotNull(result.Transaction);

            //tranResult = result.Transaction;
            //Assert.AreEqual(TransactionStatus.Approved, tranResult.Status);


        }

        /// <summary>
        /// Test all 3 Steps as following
        /// 1. PreAuthorize
        /// 2. Capture
        /// 3. Void
        /// </summary>

        [TestMethod]
        public  void TestSuccessCreditCardTransaction_PreAuthorizeCaptureVoid_TransactionIsSuccess()
        {

            _extendedInformation.InvoiceNumber = "TEST" + DateTime.Now.ToString("yyyyMMdd_HHmmss.fffff");
            //Test PreAuthorize method.
            ServiceNetResponse result =  _paymentService.PreAuthorize(_amount, _currency, _goodcard, _extendedInformation).Result;
            
            Assert.AreEqual(result.Success, true);
            Assert.IsNotNull(result.Transaction);

            var tranResult = result.Transaction;

            //Assert.AreEqual(tranResult.ResultCode, "0");
            //Assert.AreEqual(tranResult.ResultMessage, TransactionStatus.Approved.ToString("g"), true);
            Assert.AreEqual(TransactionStatus.Approved,tranResult.Status);
            Assert.IsNotNull(tranResult.TransactionKey);

            string transactionKey = tranResult.TransactionKey;

            //Test Capture method.
            result = _paymentService.Capture(transactionKey,_amount, _extendedInformation).Result;
            Assert.AreEqual(result.Success, true);
            Assert.IsNotNull(result.Transaction);

            tranResult = result.Transaction;

            Assert.AreEqual(TransactionStatus.Approved,tranResult.Status);

            //Re-assigning latest transaction key for next Void method to pass.
            transactionKey = tranResult.TransactionKey;

            //Test Void method.
            result = _paymentService.Void(transactionKey, _extendedInformation).Result;
            Assert.AreEqual(result.Success, true);
            Assert.IsNotNull(result.Transaction);

            tranResult = result.Transaction;
            Assert.AreEqual(TransactionStatus.Approved, tranResult.Status);


        }

        /// <summary>
        /// Test all 2 Steps as following
        /// 1. PreAuthorize
        /// 2. Void
        /// </summary>

        [TestMethod]
        public void TestSuccessCreditCardTransaction_PreAuthorizeVoid_TransactionIsSuccess()
        {
            _extendedInformation.InvoiceNumber = "TEST" + DateTime.Now.ToString("yyyyMMdd_HHmmss.fffff");

            //Test PreAuthorize method.
            ServiceNetResponse result = _paymentService.PreAuthorize(_amount, _currency, _goodcard, _extendedInformation).Result;

            Assert.AreEqual(result.Success, true);
            Assert.IsNotNull(result.Transaction);

            var tranResult = result.Transaction;

            //Assert.AreEqual(tranResult.ResultCode, "0");
            //Assert.AreEqual(tranResult.ResultMessage, TransactionStatus.Approved.ToString("g"), true);
            Assert.AreEqual(TransactionStatus.Approved, tranResult.Status);
            Assert.IsNotNull(tranResult.TransactionKey);

            string transactionKey = tranResult.TransactionKey;

            //Test Void method.
            result = _paymentService.Void(transactionKey, _extendedInformation).Result;
            Assert.AreEqual(result.Success, true);
            Assert.IsNotNull(result.Transaction);

            tranResult = result.Transaction;
            Assert.AreEqual(TransactionStatus.Approved, tranResult.Status);

        }


        /// <summary>
        /// Test all 3 Steps as following
        /// 1. PreAuthorize
        /// 2. Capture
        /// 3. Credit
        /// </summary>

        [TestMethod]
        public void TestSuccessCreditCardTransaction_PreAuthorizeCaptureCredit_TransactionIsSuccess()
        {
            _extendedInformation.InvoiceNumber = "TEST" + DateTime.Now.ToString("yyyyMMdd_HHmmss.fffff");

            //Test PreAuthorize method.
            ServiceNetResponse result = _paymentService.PreAuthorize(_amount, _currency, _goodcard, _extendedInformation).Result;

            Assert.AreEqual(result.Success, true);
            Assert.IsNotNull(result.Transaction);

            var tranResult = result.Transaction;

            //Assert.AreEqual(tranResult.ResultCode, "0");
            //Authorize.Net ResultCode 1
            Assert.AreEqual(TransactionStatus.Approved, tranResult.Status);
            //Assert.AreEqual(tranResult.StatusCode, TransactionStatus.Approved.ToString("g"), true);
            Assert.IsNotNull(tranResult.TransactionKey);

            string transactionKey = tranResult.TransactionKey;

            //Test Capture method.
            result = _paymentService.Capture(transactionKey, _amount, _extendedInformation).Result;
            Assert.AreEqual(result.Success, true);
            Assert.IsNotNull(result.Transaction);

            tranResult = result.Transaction;

            Assert.AreEqual(TransactionStatus.Approved,tranResult.Status);

            //Re-assigning latest transaction key for next Void method to pass.
            transactionKey = tranResult.TransactionKey;

            //Test Credit method. 
            //TODO: need check this late, Authorize.Net does not support this credit 
            if (!isAuthorizeNet)
            {
                result = _paymentService.Credit(transactionKey, null, _extendedInformation).Result;
                Assert.AreEqual(result.Success, true);
                Assert.IsNotNull(result.Transaction);

                tranResult = result.Transaction;
                Assert.AreEqual(TransactionStatus.Approved, tranResult.Status);
            }
            else
            {
                //try to void at first
                result = _paymentService.Void(transactionKey, _extendedInformation).Result;
                if (!result.Success)
                {
                    result = _paymentService.Credit(transactionKey, null, _extendedInformation).Result;
                }
                Assert.AreEqual(result.Success, true);
                Assert.IsNotNull(result.Transaction);

            }

        }

        /// <summary>
        /// Test all 3 Steps as following
        /// 1. PreAuthorize
        /// 2. Capture
        /// 3. Refund
        /// </summary>

        [TestMethod]
        public void TestSuccessCreditCardTransaction_PreAuthorizeCaptureRefundTransaction_TransactionIsSuccess()
        {
            _extendedInformation.InvoiceNumber = "TEST" + DateTime.Now.ToString("yyyyMMdd_HHmmss.fffff");

            //Test PreAuthorize method.
            ServiceNetResponse result = _paymentService.PreAuthorize(_amount, _currency, _goodcard, _extendedInformation).Result;

            Assert.AreEqual(result.Success, true);
            Assert.IsNotNull(result.Transaction);

            var tranResult = result.Transaction;

            //Assert.AreEqual(tranResult.ResultCode, "0");
            //Assert.AreEqual(tranResult.ResultMessage, TransactionStatus.Approved.ToString("g"), true);
            Assert.AreEqual(TransactionStatus.Approved, tranResult.Status);
            Assert.IsNotNull(tranResult.TransactionKey);

            string transactionKey = tranResult.TransactionKey;

            //Test Capture method.
            result = _paymentService.Capture(transactionKey, _amount, _extendedInformation).Result;
            Assert.AreEqual(result.Success, true);
            Assert.IsNotNull(result.Transaction);

            tranResult = result.Transaction;

            Assert.AreEqual(TransactionStatus.Approved, tranResult.Status);

            // Test Refund
            _amount = 22.99M;
            result = _paymentService.Refund(_amount, _currency, _goodcard, _extendedInformation).Result;

            Assert.AreEqual(result.Success, true);
            Assert.IsNotNull(result.Transaction);

             tranResult = result.Transaction;

            //Assert.AreEqual(tranResult.ResultCode, "0");
            //Assert.AreEqual(tranResult.ResultMessage, TransactionStatus.Approved.ToString("g"), true);
            Assert.AreEqual(TransactionStatus.Approved, tranResult.Status);


        }

        /// <summary>
        /// Test all 3 Steps as following
        /// 1. PreAuthorize
        /// 2. Capture
        /// 3. Partial Refund with previous transaction key
        /// </summary>

        [TestMethod]
        public void TestSuccessCreditCardTransaction_PreAuthorizeCaptureRefundWithReferencedTransaction_TransactionIsSuccess()
        {
            _extendedInformation.InvoiceNumber = "TEST" + DateTime.Now.ToString("yyyyMMdd_HHmmss.fffff");

            //Test PreAuthorize method.
            ServiceNetResponse result = _paymentService.PreAuthorize(_amount, _currency, _goodcard, _extendedInformation).Result;

            Assert.AreEqual(result.Success, true);
            Assert.IsNotNull(result.Transaction);

            var tranResult = result.Transaction;

            //Assert.AreEqual(tranResult.ResultCode, "0");
            //Assert.AreEqual(tranResult.ResultMessage, TransactionStatus.Approved.ToString("g"), true);
            Assert.AreEqual(TransactionStatus.Approved, tranResult.Status);
            Assert.IsNotNull(tranResult.TransactionKey);

            string transactionKey = tranResult.TransactionKey;

            //Test Capture method.
            result = _paymentService.Capture(transactionKey, _amount, _extendedInformation).Result;
            Assert.AreEqual(result.Success, true);
            Assert.IsNotNull(result.Transaction);

            tranResult = result.Transaction;

            Assert.AreEqual(TransactionStatus.Approved, tranResult.Status);
            transactionKey = tranResult.TransactionKey;

            // Test Refund
            if (!isAuthorizeNet)//TODO: The authorize.net failed
            {
                string referencekey = transactionKey;
                _amount = 22.99M;
                _extendedInformation.ReferenceTransactionKey = referencekey;
                result = _paymentService.Refund(_amount, _currency, _goodcard, _extendedInformation).Result;

                Assert.AreEqual(result.Success, true);
                Assert.IsNotNull(result.Transaction);

                tranResult = result.Transaction;

                //Assert.AreEqual(tranResult.ResultCode, "0");
                //Assert.AreEqual(tranResult.ResultMessage, TransactionStatus.Approved.ToString("g"), true);
                Assert.AreEqual(TransactionStatus.Approved, tranResult.Status);
            }
            else
            {
                //try to void at first
                result = _paymentService.Void(transactionKey, _extendedInformation).Result;
                if (!result.Success)
                {
                    result = _paymentService.Credit(transactionKey, null, _extendedInformation).Result;
                }
                Assert.AreEqual(result.Success, true);
                Assert.IsNotNull(result.Transaction);
            }

        }


        [TestMethod]
        public void PreauthorizeCreditCardTransaction_SubmitRequest_PreAuthorizationIsSuccess()
        {
            _extendedInformation.InvoiceNumber = "TEST" + DateTime.Now.ToString("yyyyMMdd_HHmmss.fffff");

            //Test PreAuthorize method.
            ServiceNetResponse result = _paymentService.PreAuthorize(_amount, _currency, _goodcard, _extendedInformation).Result;

            Assert.AreEqual(true,result.Success);
            Assert.IsNotNull(result.Transaction);

            var tranResult = result.Transaction;

            //Jim: 2/26/2021, Authorize.Net result code is 1
            //Assert.AreEqual(tranResult.ResultCode, "0");
            Assert.AreEqual(tranResult.Status, TransactionStatus.Approved);
            //Assert.AreEqual(tranResult.StatusCode, TransactionStatus.Approved.ToString("g"), true);
            Assert.IsTrue(!string.IsNullOrWhiteSpace(tranResult.AuthorizationCode));



        }
#endregion

        


#region Fail Test Cases

#region "Testing on Amount Value"


        /// <summary>
        /// Validate if ammount is negative.
        /// </summary>
        [TestMethod]
        public void ValidatePreAuthorizeMethod_AmountIsNegative_PreAuthozationFail()
        {
            _amount = -1.0M;
            _extendedInformation.InvoiceNumber = "TEST" + DateTime.Now.ToString("yyyyMMdd_HHmmss.fffff");
            ServiceNetResponse result = _paymentService.PreAuthorize(_amount, _currency, _goodcard, _extendedInformation).Result;
            Assert.AreEqual(result.Success, false);
            var tranResult = result.Transaction;
            //Assert.AreEqual(tranResult.ResultCode, "4");
            Assert.AreEqual(TransactionStatus.Denied, tranResult.Status);
       
        }

        ///// <summary>
        ///// Validate if ammount is in range 1000-2000 range.
        ///// Certain amounts in this range return specific PayPal results. You can generate the results by adding $1000 to that RESULT value.
        ///// For example, for RESULT value 13 (Referral), submit the amount 1013. 
        ///// If the amount is in this range but does not correspond to a result supported by this testing mechanism, Payflow returns RESULT value 12 (Declined).
        ///// </summary>
        //[TestMethod]
        //public void ValidatePreAuthorizeMethod_AmountIsInRange1000To2000_PreAuthozationFail()
        //{
        //    _amount = 1104M;
        //    ServiceNetResponse result = _paymentService.PreAuthorize(_amount, _currency, _goodcard, _extendedInformation).Result;
        //    var tranResult = result.Transaction;
        //    Assert.AreEqual(result.Success, true);
        //    //Assert.AreEqual(tranResult.ResultCode, "12"); --Commenting this since status code is different based on ammount passed
        //    Assert.AreEqual(tranResult.StatusCode, TransactionStatus.Denied.ToString("g"), true);
        //}


        /// <summary>
        /// Validate if ammount is over 2000+ range.
        /// RESULT value 12 (Declined)
        /// </summary>
        [TestMethod]
        public void ValidatePreAuthorizeMethod_AmountIsOver2000Range_PreAuthozationFail()
        {
            if (isAuthorizeNet)
                return;
            _extendedInformation.InvoiceNumber = "TEST" + DateTime.Now.ToString("yyyyMMdd_HHmmss.fffff");

            _amount = 2019M;
            ServiceNetResponse result = _paymentService.PreAuthorize(_amount, _currency, _goodcard, _extendedInformation).Result;
            var tranResult = result.Transaction;
            Assert.AreEqual(result.Success, false);
            //Assert.AreEqual(tranResult.ResultCode, "12");
            Assert.AreEqual(TransactionStatus.Denied,tranResult.Status);

        }

#endregion

#region "Testing on Result Code values"

        /// <summary>
        /// Validate Invalid merchant information.
        /// Use the AMT=1005. Applies only to the following processors: Global Payments East and Central, and American Express
        /// </summary>
        [TestMethod]
        public void ValidatePreAuthorizeMethod_Invalid_MerchantInformation_PreAuthozationFail()
        {
            _extendedInformation.InvoiceNumber = "TEST" + DateTime.Now.ToString("yyyyMMdd_HHmmss.fffff");

            _amount = 1005M;
            ServiceNetResponse result = _paymentService.PreAuthorize(_amount, _currency, _goodcard, _extendedInformation).Result;
            var tranResult = result.Transaction;
            //Assert.AreEqual(tranResult.ResultCode, "5");
            if(!isAuthorizeNet)
                Assert.AreEqual(TransactionStatus.Denied,tranResult.Status);
        }


        /// <summary>
        /// Validate Invalid account number.
        /// Submit an invalid account number, for example, 000000000000000
        /// </summary>
        [TestMethod]
        public void ValidatePreAuthorizeMethod_Invalid_Account_Number_PreAuthozationFail()
        {
            _extendedInformation.InvoiceNumber = "TEST" + DateTime.Now.ToString("yyyyMMdd_HHmmss.fffff");

            _goodcard.Number = "000000000000000";
            ServiceNetResponse result = _paymentService.PreAuthorize(_amount, _currency, _goodcard, _extendedInformation).Result;
            var tranResult = result.Transaction;
            Assert.AreEqual(result.ResponseCode , "412");
        }

#endregion
        
#region Testing on incorrect value pass




        /// <summary>
        /// Validating PreAuthorize when Invalid expiration date provided.
        /// </summary>
        [TestMethod]
        public void ValidatePreAuthorizeMethod_Invalid_Expiration_Date_PreAuthozationFail()
        {
            _extendedInformation.InvoiceNumber = "TEST" + DateTime.Now.ToString("yyyyMMdd_HHmmss.fffff");

            _goodcard.ExpirationDate = "0298"; //Invalid expiration date.
            
            ServiceNetResponse result = _paymentService.PreAuthorize(_amount, _currency, _goodcard, _extendedInformation).Result;
            Assert.AreEqual(result.Success, false);
            var tranResult = result.Transaction;
            Assert.AreEqual(TransactionStatus.Denied,tranResult.Status);
        }


        /// <summary>
        /// Validating PreAuthorize when card information is incorrect.
        /// </summary>
        [TestMethod]
        public void ValidatePreAuthorizeMethod_CardInformationIsIncorrect_PreAuthozationFail()
        {
            _extendedInformation.InvoiceNumber = "TEST" + DateTime.Now.ToString("yyyyMMdd_HHmmss.fffff");

            Card card = this._badcard;
            if (isAuthorizeNet)
            {
                card = new Card
                {
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
                        Zip = "46282",
                        Email = "Jon@johny.com"
                    }
                };
            }
            ServiceNetResponse result = _paymentService.PreAuthorize(_amount, _currency, card, _extendedInformation).Result;
            Assert.AreEqual(false, result.Success);
        }


        /// <summary>
        /// Testing Address Verification -Success
        /// Yes / No / Not Supported (Y / N / X) response
        /// </summary>
        [TestMethod]
        public void ValidatePreAuthorizeMethod_CorrectAddress_TestSuccess()
        {
            _extendedInformation.InvoiceNumber = "TEST" + DateTime.Now.ToString("yyyyMMdd_HHmmss.fffff");
            _goodcard.Address.Line1 = "218 Esat Avenue";
            ServiceNetResponse result = _paymentService.PreAuthorize(_amount, _currency, _goodcard, _extendedInformation).Result;
            Assert.AreEqual(result.Success, true);
            var tranResult = result.Transaction;
            Assert.AreEqual(tranResult.AVSAddressResult, "Y");
        }

        /// <summary>
        /// Testing Address Verification -Fail
        /// Yes / No / Not Supported (Y / N / X) response
        /// </summary>
        [TestMethod]
        public void ValidatePreAuthorizeMethod_BILLTOSTREET_PreAuthozationFail()
        {
            _extendedInformation.InvoiceNumber = "TEST" + DateTime.Now.ToString("yyyyMMdd_HHmmss.fffff");
            _goodcard.Address.Line1 = "668 Esat Avenue";
            ServiceNetResponse result = _paymentService.PreAuthorize(_amount, _currency, _goodcard, _extendedInformation).Result;
            Assert.AreEqual(true,result.Success );
            var tranResult = result.Transaction;
            if (!isAuthorizeNet)
            {
                Assert.AreEqual("X", tranResult.AVSAddressResult);
                Assert.AreEqual("X", tranResult.AVSZipResult);
            }
            else
            {
                Assert.AreEqual("Y", tranResult.AVSAddressResult);
                Assert.AreEqual("Y", tranResult.AVSZipResult);
            }

        }


        /// <summary>
        /// Testing Card Security Code -Success
        /// Yes / No / Not Supported (Y / N / X) response
        /// </summary>
        [TestMethod]
        public void ValidatePreAuthorizeMethod_CorrectCardSecurityCode_PreAuthozationSuccess()
        {
            _extendedInformation.InvoiceNumber = "TEST" + DateTime.Now.ToString("yyyyMMdd_HHmmss.fffff");

            _goodcard.Cvv = "222";
            if (isAuthorizeNet)
            {
                _goodcard.Cvv = "900";
            }

            ServiceNetResponse result = _paymentService.PreAuthorize(_amount, _currency, _goodcard, _extendedInformation).Result;
            Assert.AreEqual(true, result.Success);
            var tranResult = result.Transaction;
            if (isAuthorizeNet)
            {
                Assert.AreEqual(tranResult.CVV2Result, "M");
            }
            else
            {
                Assert.AreEqual(tranResult.CVV2Result, "Y");
            }
        }


        /// <summary>
        /// Testing Card Security Code -Fail
        /// Yes / No / Not Supported (Y / N / X) response
        /// 601 or higher Return value-	X 
        /// </summary>
        [TestMethod]
        public void ValidatePreAuthorizeMethod_InCorrectCardSecurityCode_PreAuthozationFail()
        {
            _extendedInformation.InvoiceNumber = "TEST" + DateTime.Now.ToString("yyyyMMdd_HHmmss.fffff");

            _goodcard.Cvv = "601";
            if (isAuthorizeNet)
                _goodcard.Cvv = "901";
            ServiceNetResponse result = _paymentService.PreAuthorize(_amount, _currency, _goodcard, _extendedInformation).Result;
            var tranResult = result.Transaction;
            if (isAuthorizeNet)
            {
                Assert.AreEqual(false, result.Success);
                Assert.AreEqual(tranResult.CVV2Result, "N");
            }
            else
            {
                Assert.AreEqual(true, result.Success);
                Assert.AreEqual(tranResult.CVV2Result, "X");
            }
                
            
        }


#endregion

#region Testing on Empty values pass

        /// <summary>
        /// Validating PreAuthorize when Currency is empty
        /// Expected result - HttpStatusCode.PreconditionFailed
        /// </summary>
        [TestMethod]
        public  void ValidatePreAuthorizeMethod_CurrencyIsEmpty_PreAuthozationFail()
        {
            _currency = "";
            _extendedInformation.InvoiceNumber = "TEST" + DateTime.Now.ToString("yyyyMMdd_HHmmss.fffff");
            ServiceNetResponse result =  _paymentService.PreAuthorize(_amount, _currency, _goodcard, _extendedInformation).Result; //Currency is empty ""
            Assert.AreEqual(result.Success, false);
        }

        /// <summary>
        /// Validating PreAuthorize when card information is empty
        /// Expected result - HttpStatusCode.PreconditionFailed
        /// </summary>
        [TestMethod]
        public  void ValidatePreAuthorizeMethod_CardInformationIsEmpty_PreAuthozationFail()
        {
            Card card = new Card(); // No card information is provided
            ServiceNetResponse result =  _paymentService.PreAuthorize(_amount, _currency, card, _extendedInformation).Result;
            Assert.AreEqual(result.Success, false);
        }

        /// <summary>
        /// Validating PreAuthorize when extended information is empty
        /// </summary>
        [TestMethod]
        public  void ValidatePreAuthorizeMethod_ExtendedInformationIsEmpty_PreAuthozationFail()
        {
            if (isAuthorizeNet)
                return;

            ExtendedInformation extendedInformation = new ExtendedInformation(); //Extended information is empty
            ServiceNetResponse result =  _paymentService.PreAuthorize(_amount, _currency, _badcard, _extendedInformation).Result;
            Assert.AreEqual(false,result.Success );

        }

#endregion

       

#endregion

    }
}
