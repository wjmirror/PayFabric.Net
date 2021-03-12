using PayFabric.Net.Models;
using SSCo.PaymentService;
using SSCo.PaymentService.Models;

namespace PayFabric.Net.Mapper
{
    public class PayFabricPayloadMapper
    {

        public PayFabricPayload MapToPayFabricPayload(decimal amount, string currency, Card card, ExtendedInformation extendInfo,
             string TransType, PayFabricOptions options)
        {

            PayFabricPayload pf = new PayFabricPayload { Amount = amount.ToString() , Type= TransType, Currency= currency ,SetupId= options.SetupId };
            if (string.IsNullOrWhiteSpace(pf.SetupId))
                pf.SetupId = null;

            if (!string.IsNullOrWhiteSpace(card.Customer))
                pf.Customer = card.Customer;

            PayFabricCardMapper pfCardMapper = new PayFabricCardMapper();
            pf.Card = pfCardMapper.MapToPayFabricCard(card);

            if (pf.Card != null && string.IsNullOrWhiteSpace(pf.Card.ID))
            {
                pf.Card.Tender = options.Tender ;
            }

            if (extendInfo != null)
            {
                if (!string.IsNullOrWhiteSpace(extendInfo.ReferenceTransactionKey))
                    pf.ReferenceKey = extendInfo.ReferenceTransactionKey;

                if (string.IsNullOrWhiteSpace(pf.Customer) && !string.IsNullOrWhiteSpace(extendInfo.Customer))
                    pf.Customer = extendInfo.Customer;

                pf.Document = new PayFabricExtendedInformation()
                {
                    Head = new System.Collections.Generic.List<PayFabricNameValue>()
                };

                var head = pf.Document.Head;

                if (!string.IsNullOrWhiteSpace(extendInfo.InvoiceNumber))
                    head.Add(new PayFabricNameValue()
                    {
                        Name = "InvoiceNumber",
                        Value = extendInfo.InvoiceNumber
                    });

                if (extendInfo.LevelTwoData != null)
                {
                    var lvl2 = extendInfo.LevelTwoData;

                    if (!string.IsNullOrWhiteSpace(lvl2.PurchaseOrder))
                        head.Add(new PayFabricNameValue()
                        {
                            Name = "PONumber",
                            Value = lvl2.PurchaseOrder
                        });

                    if (lvl2.DiscountAmount != null)
                        head.Add(new PayFabricNameValue()
                        {
                            Name = "DiscountAmount",
                            Value = lvl2.DiscountAmount.Value.ToString()
                        });

                    if (lvl2.DutyAmount != null)
                        head.Add(new PayFabricNameValue()
                        {
                            Name = "DutyAmount",
                            Value = lvl2.DutyAmount.Value.ToString()
                        });

                    if (lvl2.FreightAmount != null)
                        head.Add(new PayFabricNameValue()
                        {
                            Name = "FreightAmount",
                            Value = lvl2.FreightAmount.Value.ToString()
                        });

                    if (lvl2.HandlingAmount != null)
                        head.Add(new PayFabricNameValue()
                        {
                            Name = "HandlingAmount",
                            Value = lvl2.HandlingAmount.Value.ToString()
                        });

                    if (lvl2.IsTaxExempt.GetValueOrDefault())
                        head.Add(new PayFabricNameValue()
                        {
                            Name = "TaxExempt",
                            Value = "Y"
                        });

                    if (lvl2.TaxAmount != null)
                        head.Add(new PayFabricNameValue()
                        {
                            Name = "TaxAmount",
                            Value = lvl2.TaxAmount.Value.ToString()
                        });

                    if (!string.IsNullOrWhiteSpace(lvl2.ShipFromZip))
                        head.Add(new PayFabricNameValue()
                        {
                            Name = "ShipFromZip",
                            Value = lvl2.ShipFromZip
                        });

                    if (!string.IsNullOrWhiteSpace(lvl2.ShipToZip))
                        head.Add(new PayFabricNameValue()
                        {
                            Name = "ShipToZip",
                            Value = lvl2.ShipToZip
                        });

                    if (lvl2.OrderDate != null)
                        head.Add(new PayFabricNameValue()
                        {
                            Name = "OrderDate",
                            Value = lvl2.OrderDate.Value.ToString("MM/dd/yyyy")
                        });


                    if (lvl2.VATTaxAmount != null)
                        head.Add(new PayFabricNameValue()
                        {
                            Name = "VATTaxAmount",
                            Value = lvl2.VATTaxAmount.Value.ToString()
                        });

                    if (lvl2.VATTaxRate != null)
                        head.Add(new PayFabricNameValue()
                        {
                            Name = "VATTaxRate",
                            Value = lvl2.VATTaxRate.Value.ToString()
                        });
                }

                if (pf.Document.Head.Count == 0)
                    pf.Document.Head = null;

            }

            return pf;
        }
    }
}
