using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using SSCo.PaymentService;

namespace PayFabric.Net
{
    public partial class PayFabricOptions: IPaymentServiceOptions
    {
        public string BaseUrl { get; set; }
        public string DeviceId { get; set; }
        public string Password { get; set; }
        public  string SetupId { get; set; }
        public string Tender { get; set; }
        public string Cvc { get; set; }
        public HttpMessageHandler MessageHandler { get; set; }
    }
}
