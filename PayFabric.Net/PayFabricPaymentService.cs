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
    public partial class PayFabricPaymentService : IPaymentService
    {
        private readonly HttpClient httpClient;
        private readonly PayFabricOptions options;
        private readonly ILogger logger;
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
        /// Type: Ship
        /// </summary>
        /// <param name="transactionKey"></param>
        /// <param name="amount"></param>
        /// <param name="extInfo"></param>
        /// <returns></returns>
        public async Task<ServiceNetResponse> Capture(string transactionKey, decimal? amount, ExtendedInformation extInfo)
        {

            return await this.ProcessTransaction((transaction) =>
            {
                transaction.Type = TransactionType.Capture.ToString("g");
                transaction.ReferenceKey = transactionKey;
                transaction.Amount = amount;
            });

        }



        /// <summary>
        /// Type: Sale
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="currency"></param>
        /// <param name="card"></param>
        /// <param name="extInfo"></param>
        /// <returns></returns>
        public async Task<ServiceNetResponse> Charge(decimal amount, string currency, Card card, ExtendedInformation extInfo)
        {
            return await this.CreateProcessTransaction((transaction) =>
            {
                transaction.Type = TransactionType.Sale.ToString("g");
                transaction.Amount = amount;
                transaction.Currency = currency;
                transaction.Card = card;
                if (!string.IsNullOrWhiteSpace(extInfo.Customer))
                    transaction.Customer = extInfo.Customer;

                SetupTransactionDocument(transaction, extInfo);
            });
        }
        /// <summary>
        /// Type: Credit
        /// GET /reference/151013003792?trxtype=CREDIT
        /// </summary>
        /// <param name="transactionKey"></param>
        /// <param name="amount"></param>
        /// <param name="extInfo"></param>
        /// <returns></returns>
        public async Task<ServiceNetResponse> Refund(string transactionKey, decimal? amount, ExtendedInformation extInfo)
        {
            if (amount.HasValue)
            {
                throw new ArgumentException("Sorry, Payfabric does not support parital Refund. Please use Credit function.");
            }

            return await this.ProcessTransaction((transaction) =>
            {
                transaction.Type = TransactionType.Refund.ToString("g");
                transaction.ReferenceKey = transactionKey;
            });

        }
        /// <summary>
        /// Type: BOOK
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="currency"></param>
        /// <param name="card"></param>
        /// <param name="extInfo"></param>
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

                SetupTransactionDocument(transaction, extInfo);
            });
        }



        /// <summary>
        /// Type: Credit
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="currency"></param>
        /// <param name="card"></param>
        /// <param name="extInfo"></param>
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

                SetupTransactionDocument(transaction, extInfo);
            });
        }
        /// <summary>
        /// Type:VOID
        /// GET /reference/151013003792?trxtype=VOID
        /// </summary>
        /// <param name="transactionKey"></param>
        /// <param name="extInfo"></param>
        /// <returns></returns>
        public async Task<ServiceNetResponse> Void(string transactionKey, ExtendedInformation extInfo)
        {
            return await this.ProcessTransaction((transaction) =>
            {
                transaction.Type = TransactionType.Void.ToString("g");
                transaction.ReferenceKey = transactionKey;
            });
        }

        #region Transaction functions

        protected virtual async Task<string> CreateTransaction(Action<Transaction> setupPayLoad)
        {
            string key = null;

            var transaction = new Transaction();
            if (!string.IsNullOrWhiteSpace(this.options.SetupId))
                transaction.SetupId = this.options.SetupId;

            if (setupPayLoad != null)
                setupPayLoad(transaction);

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

        protected virtual async Task<ServiceNetResponse> CreateProcessTransaction(Action<Transaction> setupPayLoad)
        {
            ServiceNetResponse result = new ServiceNetResponse();

            var transaction = new Transaction();
            if (!string.IsNullOrWhiteSpace(this.options.SetupId))
                transaction.SetupId = this.options.SetupId;

            if (setupPayLoad != null)
                setupPayLoad(transaction);

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

        protected virtual async Task<ServiceNetResponse> ProcessTransaction(Action<Transaction> setupPayLoad)
        {
            ServiceNetResponse result = new ServiceNetResponse();
            var transaction = new Transaction();

            if (setupPayLoad != null)
                setupPayLoad(transaction);


            HttpRequestMessage message = new HttpRequestMessage();

            //Process a Saved transaction
            if (!string.IsNullOrWhiteSpace(transaction.Key))
            {
                string urlPath = $"/transaction/process/{transaction.Key}";
                if (!string.IsNullOrWhiteSpace(transaction.Card?.Cvc))
                    urlPath += $"?cvc={transaction.Card.Cvc}";

                message.RequestUri = this.parseUri(urlPath);
                message.Method = HttpMethod.Get;

                if (this.logger.IsEnabled(LogLevel.Trace))
                    logger.LogTrace("Process Saved Transaction - Key:{0}", transaction.Key);

            }
            // Process a reference transaction
            else if (!string.IsNullOrWhiteSpace(transaction.ReferenceKey)) 
            {
                if (string.IsNullOrWhiteSpace(transaction.Type))
                    throw new Exception("The transaction type must be set for a reference transaction.");

                string urlPath = $"/reference/{transaction.Key}?trxtype={transaction.Type}";
                message.RequestUri = this.parseUri(urlPath);
                message.Method = HttpMethod.Post;

                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.NullValueHandling = NullValueHandling.Ignore;
                string payloadString = JsonConvert.SerializeObject(transaction, settings);
                message.Content = new StringContent(payloadString, Encoding.UTF8, "application/json");

                if (this.logger.IsEnabled(LogLevel.Trace))
                    logger.LogTrace("Process Reference Transaction - PayLoad:{0}", payloadString);
            }
            else
            {
                throw new Exception("Either a transaction Key or ReferenceKey must be set.");
            }

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

                if (extInfo.LevelTwoData != null)
                {
                    var lvl2 = extInfo.LevelTwoData;

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
                            Value = lvl2.DiscountAmount.Value.ToString()
                        });

                    if (lvl2.DutyAmount != null)
                        head.Add(new NameValue()
                        {
                            Name = "DutyAmount",
                            Value = lvl2.DutyAmount.Value.ToString()
                        });

                    if (lvl2.FreightAmount != null)
                        head.Add(new NameValue()
                        {
                            Name = "FreightAmount",
                            Value = lvl2.FreightAmount.Value.ToString()
                        });

                    if (lvl2.HandlingAmount != null)
                        head.Add(new NameValue()
                        {
                            Name = "HandlingAmount",
                            Value = lvl2.HandlingAmount.Value.ToString()
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
                            Value = lvl2.TaxAmount.Value.ToString()
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
                            Value = lvl2.VATTaxAmount.Value.ToString()
                        });

                    if (lvl2.VATTaxRate != null)
                        head.Add(new NameValue()
                        {
                            Name = "VATTaxRate",
                            Value = lvl2.VATTaxRate.Value.ToString()
                        });
                }

                if (transaction.Document.Head.Count == 0)
                    transaction.Document.Head = null;

                if(extInfo.LevelThreeData!=null && extInfo.LevelThreeData.Count > 0)
                {
                    //TODO: parse the level three data
                }
                
                //set pf.document is null if there is no actual content
                if (transaction.Document.Lines == null &&
                    transaction.Document.Head == null &&
                    transaction.Document.DefaultBillTo == null
                    && string.IsNullOrWhiteSpace(transaction.Document.UserDefined))
                    transaction.Document = null;
                    

            }
        }
        #endregion
    }
}

