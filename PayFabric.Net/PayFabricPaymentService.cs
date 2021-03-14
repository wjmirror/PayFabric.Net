using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using PayFabric.Net.Models;
using Newtonsoft.Json;
using System.IO;
using System.Runtime.Serialization;
using System.Linq;


using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;



namespace PayFabric.Net
{
    /// <summary>
    /// PayFabricPaymentService is the PayFabric payment api client library. 
    /// </summary>
    public partial class PayFabricPaymentService : IPaymentService, ITransactionService
    {
        private readonly HttpClient httpClient;
        private readonly PayFabricOptions options;
        private readonly ILogger logger;

        /// <summary>
        /// Constructure 
        /// </summary>
        /// <param name="options">PayFabric service options</param>
        /// <param name="logger">Logger</param>
        public PayFabricPaymentService(IOptions<PayFabricOptions> options,
                                        ILogger logger)
        {
           
            this.options = options.Value;
            this.logger = logger;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            this.httpClient = new HttpClient(this.options?.MessageHandler ?? new HttpClientHandler());
            this.httpClient.BaseAddress = new Uri(this.options.BaseUrl);
            this.httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            this.httpClient.DefaultRequestHeaders.TryAddWithoutValidation("authorization", string.Format("{0}|{1}", this.options.DeviceId, this.options.Password));

        }
        /// <summary>
        /// Capture transaction will attempt to execute and finalize (capture) a pre-authorized transaction with specific amount, if Amount is null, it will capture with authorized amount. if Amount is provoided, it could be able to capture an authorization transaction multiple times, which depends on what gateway been used. (Note: Following gateways support multiple captures, Authorize.Net, USAePay & Payeezy(aka First Data GGE4).)
        /// </summary>
        /// <param name="transactionKey">The transaction key returned from the Authorization transaction. </param>
        /// <param name="amount">The capture amount.  If Amount is null, it will capture with authorized amount. </param>
        /// <param name="extInfo">The extension infomation, usaully be null </param>
        /// <returns></returns>
        public async Task<ServiceNetResponse> Capture(string transactionKey, decimal? amount, ExtendedInformation extInfo)
        {

            return await this.ProcessReferenceTransaction((transaction) =>
            {
                transaction.Type = TransactionType.Capture.ToString("g");
                transaction.ReferenceKey = transactionKey;
                transaction.Amount = amount;
            });

        }



        /// <summary>
        /// Sales transaction (aka Charge) is an immediate charge to the customer’s credit card or account. Money will not be moved until settlement has occurred. A Sale can only be reversed with a Void or a Refund. A Sale transaction does the same thing regardless of it being a credit card transaction, an eCheck transaction, or an ACH transaction.
        /// </summary>
        /// <param name="amount">The charge amount.</param>
        /// <param name="currency">Currency of the amout.</param>
        /// <param name="card">Credit Card / Echeck information.</param>
        /// <param name="extInfo">Extension information</param>
        /// <returns>The ServiceNetResponse object, include the http status, raw response and transaction response</returns>
        public async Task<ServiceNetResponse> Sale(decimal amount, string currency, Card card, ExtendedInformation extInfo)
        {
            return await this.CreateProcessTransaction((transaction) =>
            {
                transaction.Type = TransactionType.Sale.ToString("g");
                transaction.Amount = amount;
                transaction.Currency = currency;
                transaction.Card = card;
                if (!string.IsNullOrWhiteSpace(extInfo.Customer))
                    transaction.Customer = extInfo.Customer;

                if (!string.IsNullOrWhiteSpace(card.Customer))
                    transaction.Customer = card.Customer;

                SetupTransactionDocument(transaction, extInfo);
            });
        }
        /// <summary>
        /// Refund transaction will attempt to credit a transaction that has already been submitted to a payment gateway and has been settled from the bank. PayFabric attempts to submit a CREDIT transaction for the same exact amount as the original SALE transaction.
        /// </summary>
        /// <param name="transactionKey">The previous settled transaction key.</param>
        /// <param name="amount">Amount to refund, for Payfabric, this amount must be null, since it does not support partial refund. </param>
        /// <param name="extInfo">The extension informaiont, usually be null.</param>
        /// <returns></returns>
        public async Task<ServiceNetResponse> Refund(string transactionKey, decimal? amount, ExtendedInformation extInfo)
        {
            if (amount.HasValue)
            {
                throw new ArgumentException("Sorry, Payfabric does not support parital Refund. Please use Credit function.");
            }

            return await this.ProcessReferenceTransaction((transaction) =>
            {
                transaction.Type = TransactionType.Refund.ToString("g");
                transaction.ReferenceKey = transactionKey;
            });

        }
        /// <summary>
        /// PreAuthorize transaction reserve of a specified amount on the customer’s credit card or account
        /// </summary>
        /// <param name="amount">The amount to reserve.</param>
        /// <param name="currency">The currency of the amount.</param>
        /// <param name="card">The credit card information.</param>
        /// <param name="extInfo">The extension information.</param>
        /// <returns></returns>
        public async Task<ServiceNetResponse> PreAuthorize(decimal amount, string currency, Card card, ExtendedInformation extInfo)
        {
            return await this.CreateProcessTransaction((transaction) =>
            {
                transaction.Type = TransactionType.Authorization.ToString("g");
                transaction.Amount = amount;
                transaction.Currency = currency;
                transaction.Card = card;
                if (!string.IsNullOrWhiteSpace(extInfo.Customer))
                    transaction.Customer = extInfo.Customer;

                if (!string.IsNullOrWhiteSpace(card.Customer))
                    transaction.Customer = card.Customer;

                SetupTransactionDocument(transaction, extInfo);
            });
        }



        /// <summary>
        /// A Refund is issued to transfer money from the company’s account to the customer’s account or credit card.
        /// </summary>
        /// <param name="amount">The amount to refund</param>
        /// <param name="currency">the currency of the amount</param>
        /// <param name="card">the credit card information</param>
        /// <param name="extInfo">the extension information</param>
        /// <returns></returns>
        public async Task<ServiceNetResponse> Credit(decimal amount, string currency, Card card, ExtendedInformation extInfo)
        {
            return await this.CreateProcessTransaction((transaction) =>
            {
                transaction.Type = TransactionType.Refund.ToString("g");
                transaction.Amount = amount;
                transaction.Currency = currency;
                transaction.Card = card;
                if (!string.IsNullOrWhiteSpace(extInfo.Customer))
                    transaction.Customer = extInfo.Customer;

                if (!string.IsNullOrWhiteSpace(card.Customer))
                    transaction.Customer = card.Customer;

                SetupTransactionDocument(transaction, extInfo);
            });
        }
        /// <summary>
        /// Void transaction attempt to cancel a transaction that has already been processed successfully with a payment gateway, but before settlement with the bank, if cancellation is not possible a refund (credit) must be performed.
        /// </summary>
        /// <param name="transactionKey"></param>
        /// <param name="extInfo">The extension information, usualy be null.</param>
        /// <returns></returns>
        public async Task<ServiceNetResponse> Void(string transactionKey, ExtendedInformation extInfo)
        {
            return await this.ProcessReferenceTransaction((transaction) =>
            {
                transaction.Type = TransactionType.Void.ToString("g");
                transaction.ReferenceKey = transactionKey;
            });
        }

        #region Transaction functions

        /// <summary>
        /// Create and save a transaction on Payfabric server.
        /// </summary>
        /// <param name="setupTransaction">the callback function to setup the transaction.</param>
        /// <returns></returns>
        public virtual async Task<string> CreateTransaction(Action<Transaction> setupTransaction)
        {
            var transaction = new Transaction();
            if (!string.IsNullOrWhiteSpace(this.options.SetupId))
                transaction.SetupId = this.options.SetupId;

            if (setupTransaction != null)
                setupTransaction(transaction);

            return await this.CreateTransaction(transaction);
        }

        /// <summary>
        /// Create and save a transaction on Payfabric server.
        /// </summary>
        /// <param name="transaction">Transaction</param>
        /// <returns></returns>
        protected virtual async Task<string> CreateTransaction(Transaction transaction)
        {
            string key = null;
            if (string.IsNullOrWhiteSpace(transaction.Type))
                throw new Exception("The Transaction Type must be set. ");

            if (transaction.Amount == null)
                throw new Exception("The Transaction Amount must be set.");

            if (string.IsNullOrWhiteSpace(transaction.Currency))
                throw new Exception("The currency must be set.");

            try
            {
                HttpRequestMessage message = new HttpRequestMessage();
                message.RequestUri = this.parseUri("/transaction/create");
                message.Method = HttpMethod.Post;
                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.NullValueHandling = NullValueHandling.Ignore;
                string payloadString = JsonConvert.SerializeObject(transaction, settings);
                message.Content = new StringContent(payloadString, Encoding.UTF8, "application/json");
                if (this.logger.IsEnabled(LogLevel.Trace))
                    logger.LogTrace("Create Transaction Request - PayLoad:{0}", payloadString);

                using (HttpResponseMessage responseMessage = await this.httpClient.SendAsync(message))
                {

                    await TraceResponse(responseMessage);

                    if (responseMessage.StatusCode == HttpStatusCode.OK)
                    {
                        string responseConent = await responseMessage.Content.ReadAsStringAsync();
                        JObject jobj = JObject.Parse(responseConent);
                        key = jobj.Value<string>("Key");
                    }
                    else
                    {
                        string responseConent = await responseMessage.Content.ReadAsStringAsync();
                        this.logger.LogError($"Create Transaction Error, HttpStatus:{responseMessage.StatusCode}\r\n Message: {responseMessage.ReasonPhrase}\r\n Content:  {responseConent}");
                    }
                }
            }
            catch (Exception ex)
            {
                string cn = transaction.Card?.Account;
                if (!string.IsNullOrWhiteSpace(cn) && cn.Length >= 4)
                    cn = cn.Substring(cn.Length - 4);
                logger.LogError(ex, $"Exception in Create Transaction, Transaction Type: {transaction.Type}, Customer: {transaction.Customer},  Card Id: {transaction.Card?.ID}, Card Number End With: {cn} ");
            }

            return key;
        }

        /// <summary>
        /// update a transaction with new information.
        /// </summary>
        /// <param name="setupTransaction">the callback function to setup the transaction. 
        /// Note: only the property specified as Non-Null value will be updated.</param>
        /// <returns></returns>
        public virtual async Task<bool> UpdateTransaction(Action<Transaction> setupTransaction)
        {
            bool ret = false;

            var transaction = new Transaction();

            if (setupTransaction != null)
                setupTransaction(transaction);

            if (string.IsNullOrWhiteSpace(transaction.Key))
                throw new Exception("The transaction Key must be set. ");

            try
            {
                HttpRequestMessage message = new HttpRequestMessage();
                message.RequestUri = this.parseUri("/transaction/update");
                message.Method = HttpMethod.Post;
                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.NullValueHandling = NullValueHandling.Ignore;
                string payloadString = JsonConvert.SerializeObject(transaction, settings);
                message.Content = new StringContent(payloadString, Encoding.UTF8, "application/json");
                if (this.logger.IsEnabled(LogLevel.Trace))
                    logger.LogTrace("Update Transaction Request - PayLoad:{0}", payloadString);

                using (HttpResponseMessage responseMessage = await this.httpClient.SendAsync(message))
                {

                    await TraceResponse(responseMessage);

                    if (responseMessage.StatusCode == HttpStatusCode.OK)
                    {
                        string responseConent = await responseMessage.Content.ReadAsStringAsync();
                        JObject jobj = JObject.Parse(responseConent);
                        ret = jobj.Value<bool>("Result");
                    }
                    else
                    {
                        string responseConent = await responseMessage.Content.ReadAsStringAsync();
                        this.logger.LogError($"Create Transaction Error, HttpStatus:{responseMessage.StatusCode}\r\n Message: {responseMessage.ReasonPhrase}\r\n Content:  {responseConent}");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Exception in Update Transaction, Transaction Key {transaction.Key}");
            }

            return ret;
        }



        /// <summary>
        /// Retrieve a specified transaction
        /// </summary>
        /// <param name="key">The key of the specified transaction.</param>
        /// <returns></returns>
        public virtual async Task<Transaction> GetTransaction(string key)
        {
            Transaction transaction = null;

            HttpRequestMessage message = new HttpRequestMessage();
            string urlPath = $"/transaction/{key}";
            message.RequestUri = this.parseUri(urlPath);
            message.Method = HttpMethod.Get;

            if (this.logger.IsEnabled(LogLevel.Trace))
                logger.LogTrace("Get Transaction - Key:{0}", key);

            try
            {
                using (HttpResponseMessage responseMessage = await this.httpClient.SendAsync(message))
                {
                    string responseConent = await responseMessage.Content.ReadAsStringAsync();
                    transaction = JsonConvert.DeserializeObject<Transaction>(responseConent);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Exception in Get Transaction, Transaction Key: {key} ");
            }

            return transaction;
        }


        /// <summary>
        /// Create a transaction on Payfabric server and immediately process it with payment gateway.
        /// </summary>
        /// <param name="setupTransaction">the callback function to setup the transaction.</param>
        /// <returns>The transaction process response</returns>
        public virtual async Task<ServiceNetResponse> CreateProcessTransaction(Action<Transaction> setupTransaction)
        {
            ServiceNetResponse result = new ServiceNetResponse();

            var transaction = new Transaction();
            if (!string.IsNullOrWhiteSpace(this.options.SetupId))
                transaction.SetupId = this.options.SetupId;

            if (setupTransaction != null)
                setupTransaction(transaction);

            if (string.IsNullOrWhiteSpace(transaction.Type))
                throw new Exception("The Transaction Type must be set. ");

            if (transaction.Amount == null)
                throw new Exception("The Transaction Amount must be set.");

            if (string.IsNullOrWhiteSpace(transaction.Currency))
                throw new Exception("The currency must be set.");

            try
            {
                string urlPath = "/transaction/process";
                if (!string.IsNullOrWhiteSpace(transaction.Card?.Cvc))
                    urlPath += $"?cvc={transaction.Card.Cvc}";

                HttpRequestMessage message = new HttpRequestMessage();
                message.RequestUri = this.parseUri(urlPath);
                message.Method = HttpMethod.Post;
                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.NullValueHandling = NullValueHandling.Ignore;
                string payloadString = JsonConvert.SerializeObject(transaction, settings);
                message.Content = new StringContent(payloadString, Encoding.UTF8, "application/json");
                if (this.logger.IsEnabled(LogLevel.Trace))
                    logger.LogTrace("Create Transaction Request - PayLoad:{0}", payloadString);

                using (HttpResponseMessage responseMessage = await this.httpClient.SendAsync(message))
                {
                    await TraceResponse(responseMessage);
                    await ParseTransactionReponse(result, responseMessage);
                }
            }
            catch (Exception ex)
            {
                string cn = transaction.Card?.Account;
                if (!string.IsNullOrWhiteSpace(cn) && cn.Length >= 4)
                    cn = cn.Substring(cn.Length - 4);
                logger.LogError(ex, $"Exception in Create Transaction, Transaction Type: {transaction.Type}, Customer: {transaction.Customer},  Card Id: {transaction.Card?.ID}, Card Number End With: {cn} ");
            }
            return result;
        }


        /// <summary>
        /// Process a saved transaction with payment gateway.
        /// </summary>
        /// <param name="setupTransaction">The Transaction.Key must be set in the setupTransaction call back function.</param>
        /// <returns>The transaction process response</returns>
        public virtual async Task<ServiceNetResponse> ProcessTransaction(Action<Transaction> setupTransaction)
        {
            ServiceNetResponse result = new ServiceNetResponse();
            var transaction = new Transaction();

            if (setupTransaction != null)
                setupTransaction(transaction);

            if (string.IsNullOrWhiteSpace(transaction.Key))
                throw new Exception("The transaction key must be set.");

            HttpRequestMessage message = new HttpRequestMessage();
            string urlPath = $"/transaction/process/{transaction.Key}";
            if (!string.IsNullOrWhiteSpace(transaction.Card?.Cvc))
                urlPath += $"?cvc={transaction.Card.Cvc}";

            message.RequestUri = this.parseUri(urlPath);
            message.Method = HttpMethod.Get;

            if (this.logger.IsEnabled(LogLevel.Trace))
                logger.LogTrace("Process Saved Transaction - Key:{0}", transaction.Key);

            try
            {
                using (HttpResponseMessage responseMessage = await this.httpClient.SendAsync(message))
                {
                    await TraceResponse(responseMessage);
                    await ParseTransactionReponse(result, responseMessage);
                }
            }
            catch (Exception ex)
            {
                result.ServiceException = ex;
                string cn = transaction.Card?.Account;
                if (!string.IsNullOrWhiteSpace(cn) && cn.Length >= 4)
                    cn = cn.Substring(cn.Length - 4);
                logger.LogError(ex, $"Exception in Create Transaction, Transaction Type: {transaction.Type}, Customer: {transaction.Customer},  Card Id: {transaction.Card?.ID}, Card Number End With: {cn} ");
            }
            return result;
        }

        /// <summary>
        /// Process a reference transaction, Capture, Refund, Void transaction call this function.
        /// </summary>
        /// <param name="setupTransaction">Setup the transaction, the <see cref="TransactionType">transaction type</see> and ReferenceKey must be set in the setupTransaction call back function. </param>
        /// <returns></returns>
        public virtual async Task<ServiceNetResponse> ProcessReferenceTransaction(Action<Transaction> setupTransaction)
        {
            ServiceNetResponse result = new ServiceNetResponse();
            var transaction = new Transaction();

            if (setupTransaction != null)
                setupTransaction(transaction);

            if (string.IsNullOrWhiteSpace(transaction.ReferenceKey))
                throw new Exception("The ReferenceKey must be set.");

            if (string.IsNullOrWhiteSpace(transaction.Type))
                throw new Exception("The transaction type must be set for a reference transaction.");

            HttpRequestMessage message = new HttpRequestMessage();
            string urlPath = $"/transaction/process";
            message.RequestUri = this.parseUri(urlPath);
            message.Method = HttpMethod.Post;

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;
            string payloadString = JsonConvert.SerializeObject(transaction, settings);
            message.Content = new StringContent(payloadString, Encoding.UTF8, "application/json");
            if (this.logger.IsEnabled(LogLevel.Trace))
                logger.LogTrace("Process Reference Transaction - PayLoad:{0}", payloadString);

            try
            {
                using (HttpResponseMessage responseMessage = await this.httpClient.SendAsync(message))
                {
                    await TraceResponse(responseMessage);

                    await ParseTransactionReponse(result, responseMessage);
                }
            }
            catch (Exception ex)
            {
                result.ServiceException = ex;
                string cn = transaction.Card?.Account;
                if (!string.IsNullOrWhiteSpace(cn) && cn.Length >= 4)
                    cn = cn.Substring(cn.Length - 4);
                logger.LogError(ex, $"Exception in Create Transaction, Transaction Type: {transaction.Type}, Customer: {transaction.Customer},  Card Id: {transaction.Card?.ID}, Card Number End With: {cn} ");
            }
            return result;
        }
        #endregion

        #region Private functions

        private Uri parseUri(string apiPath, params object[] paras)
        {
            string uri = this.options.BaseUrl + string.Format(apiPath, paras);
            return new Uri(uri);
        }

        private async Task ParseTransactionReponse(ServiceNetResponse serviceResponse, HttpResponseMessage responseMessage)
        {
            serviceResponse.RawResponse = responseMessage;
            serviceResponse.ResponseCode = ((int)responseMessage.StatusCode).ToString();

            if (responseMessage.StatusCode == HttpStatusCode.OK)
            {
                string responseConent = await responseMessage.Content.ReadAsStringAsync();
                var transResponse = JsonConvert.DeserializeObject<TransactionResponse>(responseConent);
                serviceResponse.TransactionResponse = transResponse;
                TransactionStatus status;
                Enum.TryParse<TransactionStatus>(transResponse.Status, out status);
                serviceResponse.TransactionStatus = status;
                if (TransactionStatus.Approved == serviceResponse.TransactionStatus)
                    serviceResponse.Success = true;

            }
            else
            {
                string responseConent = await responseMessage.Content.ReadAsStringAsync();
                this.logger.LogError($"Transaction Error, HttpStatus:{responseMessage.StatusCode}\r\n Message: {responseMessage.ReasonPhrase}\r\n Content:  {responseConent}");
            }
        }

        private async Task TraceResponse(HttpResponseMessage responseMessage)
        {
            if (!this.logger.IsEnabled(LogLevel.Trace))
                return;

            string msgBody = string.Empty;
            try
            {
                msgBody = await responseMessage.Content.ReadAsStringAsync();
            }
            catch { }
            var authorizationHeader = responseMessage.RequestMessage.Headers.FirstOrDefault(p => 0 == string.Compare(p.Key, "authorization", true));
            if (!(authorizationHeader.Equals(null)))
                responseMessage.RequestMessage.Headers.Remove("authorization");
            string msg = String.Format("Transaction Response - Http Status: {0}{1}Request RequestMessage:{2},{3}Stutus Code:{4}{5}ReasonPhrase:{6}{7}Body:{8}{9} "
                , responseMessage.StatusCode.ToString("g")
                 , Environment.NewLine, responseMessage.RequestMessage
                , Environment.NewLine, ((int)responseMessage.StatusCode).ToString("g")
                , Environment.NewLine, responseMessage.ReasonPhrase, Environment.NewLine
                , msgBody, Environment.NewLine
                );
            logger.LogTrace(msg);
        }

        private void SetupTransactionDocument (Transaction transaction, ExtendedInformation extInfo)
        {
            if (extInfo != null)
            {
                System.Globalization.CultureInfo us = new System.Globalization.CultureInfo("en-us");

                if (string.IsNullOrWhiteSpace(transaction.Customer) && !string.IsNullOrWhiteSpace(extInfo.Customer))
                    transaction.Customer = extInfo.Customer;

                transaction.Document = new Document()
                {
                    Head = new System.Collections.Generic.List<NameValue>()
                };

                var head = transaction.Document.Head;

                if (!string.IsNullOrWhiteSpace(extInfo.InvoiceNumber))
                    head.Add(new NameValue()
                    {
                        Name = "InvoiceNumber",
                        Value = extInfo.InvoiceNumber
                    });

                if (extInfo.DocumentHead != null)
                {
                    var lvl2 = extInfo.DocumentHead;

                    if (!string.IsNullOrWhiteSpace(lvl2.PONumber))
                        head.Add(new NameValue()
                        {
                            Name = "PONumber",
                            Value = lvl2.PONumber
                        });

                    if (lvl2.DiscountAmount != null)
                        head.Add(new NameValue()
                        {
                            Name = "DiscountAmount",
                            Value = Convert.ToString(lvl2.DiscountAmount.Value, us)
                        });

                    if (lvl2.DutyAmount != null)
                        head.Add(new NameValue()
                        {
                            Name = "DutyAmount",
                            Value = Convert.ToString(lvl2.DutyAmount.Value, us)
                        });

                    if (lvl2.FreightAmount != null)
                        head.Add(new NameValue()
                        {
                            Name = "FreightAmount",
                            Value = Convert.ToString(lvl2.FreightAmount.Value,us)
                        });

                    if (lvl2.HandlingAmount != null)
                        head.Add(new NameValue()
                        {
                            Name = "HandlingAmount",
                            Value = Convert.ToString(lvl2.HandlingAmount.Value,us)
                        });

                    if (lvl2.IsTaxExempt.GetValueOrDefault())
                        head.Add(new NameValue()
                        {
                            Name = "TaxExempt",
                            Value = "Y"
                        });

                    if (lvl2.TaxAmount != null)
                        head.Add(new NameValue()
                        {
                            Name = "TaxAmount",
                            Value = Convert.ToString(lvl2.TaxAmount.Value,us)
                        });

                    if (!string.IsNullOrWhiteSpace(lvl2.ShipFromZip))
                        head.Add(new NameValue()
                        {
                            Name = "ShipFromZip",
                            Value = lvl2.ShipFromZip
                        });

                    if (!string.IsNullOrWhiteSpace(lvl2.ShipToZip))
                        head.Add(new NameValue()
                        {
                            Name = "ShipToZip",
                            Value = lvl2.ShipToZip
                        });

                    if (lvl2.OrderDate != null)
                        head.Add(new NameValue()
                        {
                            Name = "OrderDate",
                            Value = lvl2.OrderDate.Value.ToString("MM/dd/yyyy")
                        });


                    if (lvl2.VATTaxAmount != null)
                        head.Add(new NameValue()
                        {
                            Name = "VATTaxAmount",
                            Value = Convert.ToString(lvl2.VATTaxAmount.Value,us)
                        });

                    if (lvl2.VATTaxRate != null)
                        head.Add(new NameValue()
                        {
                            Name = "VATTaxRate",
                            Value = Convert.ToString(lvl2.VATTaxRate.Value,us)
                        });
                }

                if (transaction.Document.Head.Count == 0)
                    transaction.Document.Head = null;

                if(extInfo.DocumentLines!=null && extInfo.DocumentLines.Count > 0)
                {
                    transaction.Document.Lines = new List<DocumentLine>();
                    foreach (var l3 in extInfo.DocumentLines)
                    {
                        var line = new List<NameValue>();
                        if (l3.ItemCommodityCode != null)
                            line.Add(new NameValue { Name = "ItemCommodityCode", Value = l3.ItemCommodityCode });

                        if (l3.ItemProdCode != null)
                            line.Add(new NameValue { Name = "ItemProdCode", Value = l3.ItemProdCode });

                        if (l3.ItemUPC != null)
                            line.Add(new NameValue { Name = "ItemUPC", Value = l3.ItemUPC });

                        if (l3.ItemUOM != null)
                            line.Add(new NameValue { Name = "ItemUOM", Value = l3.ItemUOM });

                        if (l3.ItemDesc != null)
                            line.Add(new NameValue { Name = "ItemDesc", Value = l3.ItemDesc });

                        if (l3.ItemAmount != null)
                            line.Add(new NameValue { Name = "ItemAmount", Value = Convert.ToString(l3.ItemAmount.Value, us) });

                        if (l3.ItemCost != null)
                            line.Add(new NameValue { Name = "ItemCost", Value = Convert.ToString(l3.ItemCost.Value, us) });

                        if (l3.ItemDiscount != null)
                            line.Add(new NameValue { Name = "ItemDiscount", Value = Convert.ToString(l3.ItemDiscount.Value, us) });

                        if (l3.ItemFreightAmount != null)
                            line.Add(new NameValue { Name = "ItemFreightAmount", Value = Convert.ToString(l3.ItemFreightAmount.Value, us) });

                        if (l3.ItemHandlingAmount != null)
                            line.Add(new NameValue { Name = "ItemHandlingAmount", Value = Convert.ToString(l3.ItemHandlingAmount.Value, us) });

                        if (l3.ItemQuantity != null)
                            line.Add(new NameValue { Name = "ItemQuantity", Value = Convert.ToString(l3.ItemQuantity.Value, us) });

                        transaction.Document.Lines.Add(new DocumentLine { Columns = line });
                    }
                }

                //set up userdefined 
                if(extInfo.ExtentionInformation!=null && extInfo.ExtentionInformation.Count > 0)
                {
                    transaction.Document.UserDefined = new List<NameValue>();
                    foreach(var entry in extInfo.ExtentionInformation)
                    {
                        transaction.Document.UserDefined.Add(new NameValue
                        {
                            Name = entry.Key,
                            Value = Convert.ToString(entry.Value, us)
                        });
                    }
                }

                //set pf.document is null if there is no actual content
                if (transaction.Document.Lines == null &&
                    transaction.Document.Head == null &&
                    transaction.Document.DefaultBillTo == null &&
                    transaction.Document.UserDefined == null)
                    transaction.Document = null;
            }
        }
        #endregion
    }
}

