### PayFabric.Net is a PayFabric payment Api wrapper. 

#### Instruction 
Add following PayFabricOptions in appSettings
```json
{
    "PayFabricOptions": {
        "BaseUrl": "https://sandbox.payfabric.com/payment/api",
        "DeviceId": "2:a5e5b070-70c4-2f2a-dc2e-7992411d404c",
        "Password": "Xu834uhtr!@",
        "SetupId": "Evo b2b",        //Ignore or set this value as null/empty to use the default gateway.
        "Tender": "CreditCard"
    }
}
```
*Note:* The SetupId is the gateway profile name in PayFabric settings. 
#### PaymentService Sampe codes 

1. Add following code in ConfigurationServices when you use dependency injection 
```csharp
    //configure PayFabricOptions, specify to use HttpClientHandler
    services.Configure<PayFabricOptions>(options =>Configuration.GetSection(nameof(PayFabricOptions)).Bind(options));

    // Add Logger interface for PayFabricService
    services.AddScoped<Microsoft.Extensions.Logging.ILogger>(sp => sp.GetService<ILoggerFactory>().CreateLogger("PayFabric.Net"));

    // Add IPaymentService
    services.AddScoped<IPaymentService, PayFabricPaymentService>(); 

    // Add IWalletService 
    services.AddScoped<IWalletService, PayFabricWalletService>();

```
2. Example: Pre-Authorize the credit card transaction when order is booked 
```csharp
    public async Task PreAuthorize(decimal amount, string currency,Card card, ExtendedInformation extInfo)
    {
        ServiceNetResponse response =  await paymentService.PreAuthorize(amount, currency, card, extInfo);
        if(response.Success)
        {
            TransactionResult transactionResult=response.Transaction;
            string transactionKey=transactionResult.TransactionKey;
            //TODO: save the transactionKey, which we need to capture the transaction when the order is shipped.
        }
    }
```
 3. Example: Capture the previous Pre-Authorized transaction when order is shipped. 
 ```csharp
    public async Task Capture(string transactionKey, decimal amount, ExtendedInformation extInfo)
    {
        ServiceNetResponse response =  await paymentService.Capture(transactionKey, amount, extInfo);
        if(response.Success)
        {
            TransactionResult transactionResult=response.Transaction;
            //TODO: do while the transaction is success. 
        }
    }
 ```

 4. Example: Charge the credit card. 
 ```csharp
    public async Task Charge(decimal amount, string currency, Card card, ExtendedInformation extInfo)
    {
        ServiceNetResponse response =  await paymentService.Charge(amount,currency,card, extInfo);
        if(response.Success)
        {
            TransactionResult transactionResult=response.Transaction;
            string transactionKey= transactionResult.TransactionKey;
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
            TransactionResult transactionResult=response.Transaction;
            //TODO: do while the transaction is success.  
        }
    }
 ```


**Note:** Please see the test project for more samples. https://dev.azure.com/itssco/WebERP/_git/common-library-ssco.paymentservice?path=%2FSSCo.PayFabricService.Test 

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
                }
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
    var chargeResult = await this.paymentService.Charge(amount, currency, card, extInfo);
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
                    Line1 = "218 Esat Avenue",
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
     var chargeResult = await this.paymentService.Charge(105M, "USD", card, extInfo);
     Assert.IsTrue(chargeResult.Success); 
}


//Retrieve all the card from Payfabric wallet service 
Public Task<ICollection<Card>> GetCustomerCards(string customer)
{
    var cards= this.walletService.GetByCustomer(customer);
}

``` 
**Note:** Please see the test project for more samples. https://dev.azure.com/itssco/WebERP/_git/common-library-ssco.paymentservice?path=%2FSSCo.PayFabricService.Test%2FPayFabricWalletServiceTest.cs 



#### Version History:
1.1.0.21103 - 3/10/2021
* Change decimal and datetime properties in ExtendedInformation.LevelTwoData as Nullable type. 

1.1.0.21092 - released on 3/2/2021
* Add payfabric wallet service  

