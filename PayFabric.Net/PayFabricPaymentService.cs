using System;
using System.Collections.Generic;
using System.Text;
using SSCo.PaymentService;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using PayFabric.Net.Mapper;
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
            if (amount == null)
            {
                var url = this.parseUri("/reference/{0}?trxtype={1}", transactionKey, PayFabricTransactionType.SHIP.ToString("g"));
                return await this.ProcessTransaction(url);
            }


            ServiceNetResponse serviceNetResponse = new ServiceNetResponse();
            try
            {
                HttpRequestMessage message = new HttpRequestMessage();

                message.RequestUri = this.parseUri("/transaction/process");
                message.Method = HttpMethod.Post;
                JObject jObject = new JObject(
                    new JProperty("Amount", amount.ToString()),
                    new JProperty("Type", PayFabricTransactionType.SHIP.ToString("g")),
                    new JProperty("ReferenceKey", transactionKey));
                string payLoad = jObject.ToString();

                message.Content = new StringContent(payLoad, Encoding.UTF8, "application/json");
                logger.LogTrace("Capture Request - POST: PayLoad:{0}", payLoad);


                using (HttpResponseMessage responseMessage = await this.httpClient.SendAsync(message))
                {
                    await LogResponse(responseMessage);
                    if (responseMessage.StatusCode == HttpStatusCode.OK)
                    {
                        await ParseSuccessResponse(serviceNetResponse, responseMessage);
                    }
                    else
                    {
                        logger.LogError($"Error in Capture transaction, Transaction Key:{transactionKey}, Amount: {amount}, HttpStatus: {responseMessage.StatusCode}, HttpMessage: {responseMessage.ReasonPhrase}");
                        await ParseFailerResponse(serviceNetResponse, responseMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error in processing capture transaction, transaction key: {transactionKey}, amount: {amount} .");
                serviceNetResponse.Success = false;
                serviceNetResponse.ResponseMessage = ex.Message;
            }

            return serviceNetResponse;
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
            PayFabricPayloadMapper payFabricPayloadMapper = new PayFabricPayloadMapper();
            PayFabricPayload payLaod = payFabricPayloadMapper.MapToPayFabricPayload(amount, currency, card, extInfo, PayFabricTransactionType.SALE.ToString("g"), options);
            return await CreateTransaction(payLaod);
        }
        /// <summary>
        /// Type: Credit
        /// GET /reference/151013003792?trxtype=CREDIT
        /// </summary>
        /// <param name="transactionKey"></param>
        /// <param name="amount"></param>
        /// <param name="extInfo"></param>
        /// <returns></returns>
        public async Task<ServiceNetResponse> Credit(string transactionKey, decimal? amount, ExtendedInformation extInfo)
        {
            if (amount.HasValue)
            {
                throw new ArgumentException("Sorry, Payfabric does not support parital credit. Please use Refund function.");
            }

            Uri uri = this.parseUri(string.Format("/reference/{0}?trxtype={1}", transactionKey, PayFabricTransactionType.CREDIT.ToString("g")));
            return await ProcessTransaction(uri);

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
            PayFabricPayloadMapper payFabricPayloadMapper = new PayFabricPayloadMapper();
            PayFabricPayload payLaod = payFabricPayloadMapper.MapToPayFabricPayload(amount, currency, card, extInfo, PayFabricTransactionType.BOOK.ToString("g"), options);
            return await CreateTransaction(payLaod);
        }



        /// <summary>
        /// Type: Credit
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="currency"></param>
        /// <param name="card"></param>
        /// <param name="extInfo"></param>
        /// <returns></returns>
        public async Task<ServiceNetResponse> Refund(decimal amount, string currency, Card card, ExtendedInformation extInfo)
        {
            PayFabricPayloadMapper payFabricPayloadMapper = new PayFabricPayloadMapper();
            PayFabricPayload payLaod = payFabricPayloadMapper.MapToPayFabricPayload(amount, currency, card, extInfo, PayFabricTransactionType.CREDIT.ToString("g"), options);
            return await CreateTransaction(payLaod);
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
            Uri uri = this.parseUri(string.Format("/reference/{0}?trxtype={1}", transactionKey, PayFabricTransactionType.Void.ToString("g")));
            return await ProcessTransaction(uri);
        }

        #region Private functions

        private Uri parseUri(string apiPath, params object[] paras)
        {
            string uri = this.options.BaseUrl + string.Format(apiPath, paras);
            return new Uri(uri);
        }

        private async Task<ServiceNetResponse> CreateTransaction(PayFabricPayload payload)
        {
            ServiceNetResponse serviceResponse = new ServiceNetResponse();
            try
            {
                HttpRequestMessage message = new HttpRequestMessage();
                message.RequestUri = this.parseUri("/transaction/create");
                message.Method = HttpMethod.Post;
                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.NullValueHandling = NullValueHandling.Ignore;
                string payloadString = JsonConvert.SerializeObject(payload,settings);
                message.Content = new StringContent(payloadString, Encoding.UTF8, "application/json");
                logger.LogTrace("Create Transaction Request - PayLoad:{0}", payloadString);
                using (HttpResponseMessage responseMessage = await this.httpClient.SendAsync(message))
                {

                    await LogResponse(responseMessage);

                    if (responseMessage.StatusCode == HttpStatusCode.OK)
                    {
                        string createResponse = await responseMessage.Content.ReadAsStringAsync();

                        JObject jobj = JObject.Parse(createResponse);
                        string transactionKey = jobj.Value<string>("Key");
                        serviceResponse.Transaction = new TransactionResult { TransactionKey = transactionKey };
                        Uri uri = null;
                        if (payload.Card?.Cvc != null)
                        {
                            uri = this.parseUri(string.Format("/transaction/process/{0}?cvc={1}", transactionKey, payload.Card.Cvc));
                        }
                        else
                        {
                            uri = this.parseUri(string.Format("/transaction/process/{0}", transactionKey));
                        }

                        return await ProcessTransaction(uri);
                    }
                    else
                    {
                        string cn = payload.Card?.Account;
                        if (!string.IsNullOrWhiteSpace(cn) && cn.Length >= 4)
                            cn = cn.Substring(cn.Length - 4);
                        logger.LogError($"Error in Create Transaction, Transaction Type: {payload.Type}, Customer: {payload.Customer},  Card Id: {payload.Card?.ID}, Card Number End With: {cn} ");
                        await ParseFailerResponse(serviceResponse, responseMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                string cn = payload.Card?.Account;
                if (!string.IsNullOrWhiteSpace(cn) && cn.Length >= 4)
                    cn = cn.Substring(cn.Length - 4);
                logger.LogError(ex, $"Exception in Create Transaction, Transaction Type: {payload.Type}, Customer: {payload.Customer},  Card Id: {payload.Card?.ID}, Card Number End With: {cn} ");

                serviceResponse.Success = false;
                serviceResponse.ResponseMessage = "Failed: " + ex.Message;
            }
            // 2nd Phase of Process Transaction failed . Only Create transaction Successfull . contain TransactionKey
            return serviceResponse;
        }

        private async Task<ServiceNetResponse> ProcessTransaction(Uri uri)
        {
            HttpRequestMessage message = new HttpRequestMessage();
            message.RequestUri = uri;
            message.Method = HttpMethod.Get;
            logger.LogTrace("Process Request - GET" + uri.ToString());
            ServiceNetResponse serviceResponse = new ServiceNetResponse();
            try
            {
                using (HttpResponseMessage responseMessage = this.httpClient.SendAsync(message).Result)
                {

                    await LogResponse(responseMessage);
                    if (responseMessage.StatusCode == HttpStatusCode.OK)
                    {
                        await ParseSuccessResponse(serviceResponse, responseMessage);
                    }
                    else
                    {
                        logger.LogError($"Error in Process Transaction, Uri: {uri}, HttpStatus: {responseMessage.StatusCode}, HttpMessage: {responseMessage.ReasonPhrase}");
                        await ParseFailerResponse(serviceResponse, responseMessage);
                    }
                }
            }
            catch(Exception ex)
            {
                logger.LogError(ex, $"Exception in Process Transaction, Uri: {uri}");
                serviceResponse.Success = false;
                serviceResponse.ResponseMessage = ex.Message;
            }
            return serviceResponse;
        }


        private async Task ParseSuccessResponse(ServiceNetResponse serviceResponse, HttpResponseMessage responseMessage)
        {
            serviceResponse.ResponseCode = ((int)responseMessage.StatusCode).ToString();
            serviceResponse.ResponseMessage = responseMessage.ReasonPhrase;
            serviceResponse.ResponseDateTime = responseMessage.Headers.Date?.Date.ToLongDateString();

            string processResponse = await responseMessage.Content.ReadAsStringAsync();
            serviceResponse.RawResponse = processResponse;

            PayFabricResponse pResponse = JsonConvert.DeserializeObject<PayFabricResponse>(processResponse);

            PayFabricResponseMapper payFabricResponseMapper = new PayFabricResponseMapper();
            payFabricResponseMapper.MaptoServiceNetResponse(serviceResponse, pResponse);

        }

        private async Task ParseFailerResponse(ServiceNetResponse serviceResponse, HttpResponseMessage responseMessage)
        {
            serviceResponse.Success = false;
            serviceResponse.ResponseCode = ((int)responseMessage.StatusCode).ToString("g");
            serviceResponse.ResponseMessage = responseMessage.ReasonPhrase;
            serviceResponse.ResponseDateTime = responseMessage.Headers.Date.ToString();
            
            string resultMessage = responseMessage.ReasonPhrase;

            try
            {
                string responseContent = await responseMessage.Content.ReadAsStringAsync();
                serviceResponse.RawResponse = responseContent;
                resultMessage = resultMessage + ", " + responseContent;
            }
            catch (Exception ex)
            {
                logger.LogWarning("Can't Parse Failure Message: {0}", ex.Message);
            }
            serviceResponse.Transaction = new TransactionResult { Status= TransactionStatus.Failure, StatusCode="Failure", ResultCode= "Failure", ResultMessage = resultMessage };
        }

        private async Task LogResponse(HttpResponseMessage responseMessage)
        {
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


        #endregion
    }
}

