### PayFabric.Net is a PayFabric payment Api client library on .net standard 2.0. 
This is not an official .net library from PayFabric.  PayFabric does not have a .net library. It only has a Api document repository on github. 
https://github.com/PayFabric/APIs/blob/master/PayFabric/README.md 

Author: Jim Wang mailto:jim.wang1014@gmail.com

#### Instruction 
Add following PayFabricOptions in appSettings
```json
{
    "PayFabricOptions": {
        "BaseUrl": "https://sandbox.payfabric.com/payment/api",
        "DeviceId": "x:00000000-0000-0000-0000-000000000000", //The device ID in PayFabric Settings -> Device Management.
        "Password": "xxxxx", //The password of the device
        "SetupId": "Evo"   
    }
}
```
*Note:* The SetupId is the gateway profile name in PayFabric settings, ignore or set the value as null/empty to use the default gateway.
#### PaymentService Sampe codes 

1. Add following code in ConfigurationServices when you use dependency injection 
```csharp
    //configure PayFabricOptions
    services.Configure<PayFabricOptions>(options =>Configuration.GetSection(nameof(PayFabricOptions)).Bind(options));

    // Add Logger interface for PayFabricService
    services.AddScoped<Microsoft.Extensions.Logging.ILogger>(sp => sp.GetService<ILoggerFactory>().CreateLogger("PayFabric.Net"));

    // Add IPaymentService
    services.AddScoped<IPaymentService, PayFabricPaymentService>(); 
    
    // Add ITransactionService
    services.AddScoped<ITransactionService, PayFabricPaymentService>();

    // Add IWalletService 
    services.AddScoped<IWalletService, PayFabricWalletService>();

```

 2. Example: Charge the credit card. 
 ```csharp
    public async Task Charge()
    {
        var amount = 100M;
        var currency = "USD";
        Card card = new Card
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
            },
            Tender= TenderTypeEnum.CreditCard

        };

        ExtendedInformation extInfo = new ExtendedInformation
        {
            Customer = "TEST_0199999",
            InvoiceNumber = "TEST" + DateTime.Now.ToString("yyyyMMdd_HHmmss.fffff"),
            DocumentHead = new LevelTwoData
            {
                DiscountAmount = 10M,
                DutyAmount = 110M,
                TaxAmount = 10M,
                FreightAmount = 5M,
                ShipFromZip = "60139",
                ShipToZip = "60189",
                PONumber = "PO_1235",
                OrderDate = DateTime.Now
            },
            DocumentLines = new List<LevelThreeData> {
                new LevelThreeData
                {
                    ItemDesc="SHOE-LA01-BLACK",
                    ItemQuantity=1M,
                    ItemUOM ="PAIR",
                    ItemAmount=55M,
                    ItemDiscount=5M
                },
                new LevelThreeData
                {
                    ItemDesc="SHOE-LA02-WHITE",
                    ItemQuantity=1M,
                    ItemUOM ="PAIR",
                    ItemAmount=55M,
                    ItemDiscount=5M
                }
            }
        };
        ServiceNetResponse response =  await paymentService.Sale(amount, currency, card, extInfo);
        if(response.Success)
        {
            TransactionResponse transactionResponse=response.TransactionResponse;
            string transactionKey= transactionResponse.TransactionKey;
            //TODO: do while the transaction is success.  
        }
    }
 ```

3. Example: Pre-Authorize the credit card transaction when order is booked .
```csharp
    public async Task PreAuthorize(decimal amount, string currency,Card card, ExtendedInformation extInfo)
    {
        ServiceNetResponse response =  await paymentService.PreAuthorize(amount, currency, card, extInfo);
        if(response.Success)
        {
            TransactionResponse transactionResponse=response.TransactionResponse;
            string transactionKey=transactionResponse.TransactionKey;
            //TODO: save the transactionKey, which we need to capture the transaction when the order is shipped.
        }
    }
```
 4. Example: Capture the previous Pre-Authorized transaction when order is shipped. 
 ```csharp
    public async Task Capture(string transactionKey, decimal amount, ExtendedInformation extInfo)
    {
        ServiceNetResponse response =  await paymentService.Capture(transactionKey, amount, extInfo);
        if(response.Success)
        {
            TransactionResponse transactionResult=response.TransactionResponse;
            //TODO: do while the transaction is success. 
        }
    }
 ```


 5. Example: Void the transaction - We can void the transaction before the transaction is settlled. (The bank usually settle the transaction at end of the day.)  
 ```csharp
    public async Task Void(string transactionKey)
    {
        ServiceNetResponse response =  await paymentService.Void(transactionKey,null);
        if(response.Success)
        {
            TransactionResponse transactionResponse=response.TransactionResponse;
            //TODO: do while the transaction is success.  
        }
    }
 ```


**Note:** Please see the test project for more samples. https://github.com/wjmirror/PayFabric.Net/tree/master/PayFabric.Net.Test

**Note:** The ExtendedInformation is passed to payment API as Level2/3 data. The ExtendedInformation.InvoiceNumber is important, some payment gateway use it to detect duplicate transaction. And banks give us discounted price when the invoice number is specified. 

#### WalletService  
The wallet service provide the service to store the credit card/echeck in Payfabric. Then, we can pass the credit card id when calling payment service functions.
Example: 
```csharp
//Save the credit card 
public async Task<string> SaveCreditCard()
{
    Card card = new Card
            {
                Customer = "0199999", //This is required
                FirstName = "PantsON",
                LastName = "Fire",
                Number = "4583194798565295",
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
                Tender = TenderTypeEnum.CreditCard
            };
    WalletTransactionResult result await = this.walletService.Create(card);
    
    return result.Id; //return the wallet entry Id. 
}

//Charge to credit card with wallet entry Id. 
public async Task Charge(string customerNumber, string cardId, string cvv, decimal amount, string currency, ExtendedInformation extInfo )
{
    Card card = new Card()
    {
        Id = cardId,
        Cvv = cvv,
        Customer = customerNumber
    };
    var chargeResult = await this.paymentService.Sale(amount, currency, card, extInfo);
}


//We can do Save credit card and charge together. 
public async Task SaveAndChargeCreditCard()
{
    Card card = new Card
            {
                Customer = "0199999",  //Customer is Required when IsSaveCard is true. 
                FirstName = "PantsON",
                LastName = "Fire",
                Number = "4583194798565295",
                ExpirationDate = "0925",
                Address = new Address
                {
                    City = "Wheton",
                    Country = "USA",
                    Line1 = "218 East Avenue",
                    State = "IL",
                    Zip = "60139",
                    Email = "Jon@johny.com"
                },
                IsSaveCard = true //This tell payfabric to save the credit card. 
            };
     extInfo = new ExtendedInformation
     {
         InvoiceNumber = "Inv0001"
     };
     var chargeResult = await this.paymentService.Sale(105M, "USD", card, extInfo);
     Assert.IsTrue(chargeResult.Success); 
}


//Retrieve all the card from Payfabric wallet service 
Public Task<ICollection<Card>> GetCustomerCards(string customer)
{
    var cards= this.walletService.GetByCustomer(customer);
}

``` 
**Note:** Please see the test project for more samples.  https://github.com/wjmirror/PayFabric.Net/tree/master/PayFabric.Net.Test




