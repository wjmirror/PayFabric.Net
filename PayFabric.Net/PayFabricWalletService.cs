using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using PayFabric.Net.Models;
using PayFabric.Net.Mapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;


namespace PayFabric.Net
{
    public class PayFabricWalletService : IWalletService
    {
        private readonly HttpClient httpClient;
        private readonly PayFabricOptions options;

        private readonly ILogger logger;
        public PayFabricWalletService(IOptions<PayFabricOptions> options,
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

        private Uri parseUri(string apiPath, params object[] paras)
        {
            string uri = this.options.BaseUrl + string.Format(apiPath, paras);
            return new Uri(uri);
        }
        private PayFabricCard Convert2PayFabricCard(Card card)
        {
            PayFabricCardMapper pfCardMapper = new PayFabricCardMapper();
            var pfCard = pfCardMapper.MapToPayFabricCard(card);
            return pfCard;
        }

        private Card Convert2Card(PayFabricCard payfabricCard)
        {
            Card card = new Card()
            {
                Id = payfabricCard.ID,
                Number = payfabricCard.Account,
                FirstName = payfabricCard.CardHolder?.FirstName,
                LastName = payfabricCard.CardHolder?.LastName,
                Customer = payfabricCard.Customer,
                ExpirationDate = payfabricCard.ExpDate,
                IsLocked = payfabricCard.IsLocked,
                IsDefault = payfabricCard.IsDefaultCard
            };
            if (payfabricCard.Billto != null)
            {
                var bill = payfabricCard.Billto;
                card.Address = new Address()
                {
                    Line1 = bill.Line1,
                    Line2 = bill.Line2,
                    Line3 = bill.Line3,
                    City = bill.City,
                    State = bill.State,
                    Email = bill.Email,
                    Country = bill.Country,
                    Zip = bill.Zip
                };
            }
            return card;
        }

        public async Task<WalletTransactionResult> Create(Card card)
        {
            WalletTransactionResult result = new WalletTransactionResult();
            try
            {
                HttpRequestMessage message = new HttpRequestMessage();
                message.RequestUri = this.parseUri("/wallet/create");
                message.Method = HttpMethod.Post;
                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.NullValueHandling = NullValueHandling.Ignore;
                string payLoad = JsonConvert.SerializeObject(Convert2PayFabricCard(card), settings);
                message.Content = new StringContent(payLoad, Encoding.UTF8, "application/json");
                if (logger != null)
                    logger.LogTrace("Create Wallet Request - PayLoad:{0}", payLoad);

                using (HttpResponseMessage responseMessage = await this.httpClient.SendAsync(message))
                {
                    string responseContent = await responseMessage.Content.ReadAsStringAsync();

                    if (responseMessage.StatusCode == HttpStatusCode.OK)
                    {
                        JObject jobj = JObject.Parse(responseContent);
                        result.ProcessMessage = jobj.Value<string>("Message");
                        result.Id = jobj.Value<string>("Result");
                        if (!string.IsNullOrWhiteSpace(result.Id))
                            result.Success = true;
                    }
                    else
                    {
                        result.Success = false;
                        result.ProcessMessage = responseContent;
                    }
                }
            }
            catch (Exception exception)
            {
                result.Success = false;
                result.ProcessMessage = exception.Message;
            }
            return result;
        }

        public async Task<WalletTransactionResult> Update(Card card)
        {
            WalletTransactionResult result = new WalletTransactionResult();
            try
            {
                HttpRequestMessage message = new HttpRequestMessage();
                message.RequestUri = this.parseUri("/wallet/update");
                message.Method = HttpMethod.Post;
                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.NullValueHandling = NullValueHandling.Ignore;

                PayFabricCard payFabricCard = Convert2PayFabricCard(card);

                if (!string.IsNullOrWhiteSpace(payFabricCard.Account))
                    throw new Exception("PayFabric is unable to update the account/card number. To update an account/card number, delete the old Wallet entry and create a new one");

                if (!string.IsNullOrWhiteSpace(payFabricCard.Customer))
                {
                    payFabricCard.NewCustomerNumber = payFabricCard.Customer;
                    payFabricCard.Customer = null;
                }

                string payLoad = JsonConvert.SerializeObject(payFabricCard, settings);
                message.Content = new StringContent(payLoad, Encoding.UTF8, "application/json");
                if (logger != null)
                    logger.LogTrace("Update Wallet Request - PayLoad:{0}", payLoad);

                using (HttpResponseMessage responseMessage = await this.httpClient.SendAsync(message))
                {
                    string responseContent = await responseMessage.Content.ReadAsStringAsync();

                    if (responseMessage.StatusCode == HttpStatusCode.OK)
                    {
                        JObject jobj = JObject.Parse(responseContent);
                        var res = jobj.Value<string>("Result");
                        if (string.Compare(res, "true", true) == 0)
                            result.Success = true;
                    }
                    else
                    {
                        result.Success = false;
                        result.ProcessMessage = responseContent;
                    }
                }
            }
            catch (Exception exception)
            {
                result.Success = false;
                result.ProcessMessage = exception.Message;
            }

            return result;
        }

        public async Task<Card> Get(string Id)
        {
            Card card = null;

            HttpRequestMessage message = new HttpRequestMessage();
            message.RequestUri = this.parseUri("/wallet/get/{0}", Id);
            message.Method = HttpMethod.Get;

            if (logger != null)
                logger.LogTrace("Get Wallet Request - Id:{0}", Id);

            using (HttpResponseMessage responseMessage = await this.httpClient.SendAsync(message))
            {
                string responseContent = await responseMessage.Content.ReadAsStringAsync();

                if (responseMessage.StatusCode == HttpStatusCode.OK)
                {
                    JObject jobj = JObject.Parse(responseContent);
                    PayFabricCard payFabricCard = jobj.ToObject<PayFabricCard>();
                    card = this.Convert2Card(payFabricCard);
                }
                else
                {
                    throw new Exception($"Failed on Get Wallet Card, Id: {Id}, HttpStatusCode: {responseMessage.StatusCode}, Content: {responseContent}");
                }
            }

            return card;
        }

        public async Task<ICollection<Card>> GetByCustomer(string customerNumber)
        {
            ICollection<Card> cards = null;

            HttpRequestMessage message = new HttpRequestMessage();
            message.RequestUri = this.parseUri("/wallet/getByCustomer?customer={0}&tender=CreditCard", customerNumber);
            message.Method = HttpMethod.Get;

            if (logger != null)
                logger.LogTrace("Get Wallets Request - customer:{0}", customerNumber);

            using (HttpResponseMessage responseMessage = await this.httpClient.SendAsync(message))
            {
                string responseContent = await responseMessage.Content.ReadAsStringAsync();

                if (responseMessage.StatusCode == HttpStatusCode.OK)
                {
                    JArray jobj = JArray.Parse(responseContent);
                    var payFabricCards = jobj.ToObject<List<PayFabricCard>>();
                    cards = payFabricCards.Select(pf => this.Convert2Card(pf)).ToList();
                }
                else
                {
                    throw new Exception($"Failed on Get Wallet Card, customer: {customerNumber}, HttpStatusCode: {responseMessage.StatusCode}, Content: {responseContent}");
                }
            }

            return cards;
        }

        public async Task<bool> Lock(string Id, string lockReason)
        {
            bool result = false;
            var encodedLockReason = WebUtility.UrlEncode(lockReason);
            HttpRequestMessage message = new HttpRequestMessage();
            message.RequestUri = this.parseUri("/wallet/lock/{0}?lockreason={1}", Id, encodedLockReason);
            message.Method = HttpMethod.Get;

            if (logger != null)
                logger.LogTrace("Lock Wallet Request - Id:{0}, Reason: {1}", Id, lockReason);

            using (HttpResponseMessage responseMessage = await this.httpClient.SendAsync(message))
            {
                string responseContent = await responseMessage.Content.ReadAsStringAsync();

                if (responseMessage.StatusCode == HttpStatusCode.OK)
                {
                    JObject jobj = JObject.Parse(responseContent);
                    string res = jobj.Value<string>("Result");
                    if (string.Compare(res, "true", true) == 0)
                        result = true;
                }
                else
                {
                    throw new Exception($"Failed on Lock Wallet Card, Id: {Id}, HttpStatusCode: {responseMessage.StatusCode}, Content: {responseContent}");
                }
            }

            return result;
        }

        public async Task<bool> Remove(string Id)
        {
            bool result = false;
            HttpRequestMessage message = new HttpRequestMessage();
            message.RequestUri = this.parseUri("/wallet/delete/{0}", Id);
            message.Method = HttpMethod.Get;

            if (logger != null)
                logger.LogTrace("Lock Wallet Request - Id:{0}", Id);

            using (HttpResponseMessage responseMessage = await this.httpClient.SendAsync(message))
            {
                string responseContent = await responseMessage.Content.ReadAsStringAsync();

                if (responseMessage.StatusCode == HttpStatusCode.OK)
                {
                    JObject jobj = JObject.Parse(responseContent);
                    string res = jobj.Value<string>("Result");
                    if (string.Compare(res, "true", true) == 0)
                        result = true;
                }
                else
                {
                    throw new Exception($"Failed on UnLock Wallet Card, Id: {Id}, HttpStatusCode: {responseMessage.StatusCode}, Content: {responseContent}");
                }
            }

            return result;
        }

        public async Task<bool> Unlock(string Id)
        {
            bool result = false;
            HttpRequestMessage message = new HttpRequestMessage();
            message.RequestUri = this.parseUri("/wallet/unlock/{0}", Id);
            message.Method = HttpMethod.Get;

            if (logger != null)
                logger.LogTrace("Lock Wallet Request - Id:{0}", Id);

            using (HttpResponseMessage responseMessage = await this.httpClient.SendAsync(message))
            {
                string responseContent = await responseMessage.Content.ReadAsStringAsync();

                if (responseMessage.StatusCode == HttpStatusCode.OK)
                {
                    JObject jobj = JObject.Parse(responseContent);
                    string res = jobj.Value<string>("Result");
                    if (string.Compare(res, "true", true) == 0)
                        result = true;
                }
                else
                {
                    throw new Exception($"Failed on UnLock Wallet Card, Id: {Id}, HttpStatusCode: {responseMessage.StatusCode}, Content: {responseContent}");
                }
            }

            return result;
        }


    }
}
