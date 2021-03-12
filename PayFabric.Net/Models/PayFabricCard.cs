﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PayFabric.Net.Models
{

    public  class PayFabricCard
    {
        /// <summary>
        /// Unique identifier for this record. This ID is generated by PayFabric upon successful creation of a new card. The client cannot set or modify this value.
        /// </summary>
        public Guid? ID { get; set; }

        /// <summary>
        /// Tender type. Valid options are CreditCard or ECheck.
        /// </summary>
        public WalletEntryTypeEnum? Tender { get; set; }

        /// <summary>
        /// Customer ID as specified by the client upon creation of the customer.
        /// </summary>
        public string Customer { get; set; }

        /// <summary>
        /// The number of the credit card, or the eCheck/ACH account. When creating a new Card this attribute must be provided by the client in plaintext. When a client retrieves a card PayFabric always returns this attribute in masked format. Ignore this attribute when update a existing card.
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// Expiration date of the credit card in the format of MMYY. Only valid for credit cards.
        /// </summary>
        [MaxLength(4)]
        public string ExpDate { get; set; }

        /// <summary>
        /// Check number. Only valid for eChecks, and required for specific Processors (TeleCheck, Encompass).
        /// </summary>
        [MaxLength(128)]
        public string CheckNumber { get; set; }

        /// <summary>
        /// eCheck account type. Only valid for eCheck accounts.
        /// </summary>
        public string AccountType { get; set; }

        /// <summary>
        /// Bank Routing Number. Only valid for eChecks.
        /// </summary>
        public string Aba { get; set; }

        /// <summary>
        /// Type of credit card: Visa, Mastercard, Discover,JCB,AmericanExpress,DinersClub. Only valid for credit cards.
        /// </summary>
        public string CardName { get; set; }

        /// <summary>
        /// Indicates whether this is the primary card of the customer. Default value is False.
        /// </summary>
        public bool? IsDefaultCard { get; set; }

        /// <summary>
        /// Indicates whether the card is locked. Default value is False.
        /// </summary>
        public bool? IsLocked { get; set; }

        /// <summary>
        /// Indicates whether to save this card in the customer's wallet. This attribute is only valid and should only be included in the object when using Create and Process a Transaction. And it will be set to false automatically for Verify transaction.
        /// </summary>
        public bool? IsSaveCard { get; set; }

        /// <summary>
        /// This is a response field. Timestamp indicating when this record was last modified. It's format should like "3/23/2015 11:16:19 PM".
        /// </summary>
        public string ModifiedOn { get; set; }

        /// <summary>
        /// Cardholder object.
        /// </summary>
        public CardHolder CardHolder { get; set; }

        /// <summary>
        /// Address object.
        /// </summary>
        public PayFabricAddress Billto { get; set; }

        /// <summary>
        /// A client-defined identifier for this card. Developer can send a flag value to identify this card
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        /// User-defined field 1. Developer can store additional data in this field.
        /// </summary>
        [MaxLength(256)]
        public string UserDefine1 { get; set; }

        /// <summary>
        /// User-defined field 2. Developer can store additional data in this field.
        /// </summary>
        [MaxLength(256)]
        public string UserDefine2 { get; set; }

        /// <summary>
        /// User-defined field 3. Developer can store additional data in this field.
        /// </summary>
        [MaxLength(256)]
        public string UserDefine3 { get; set; }

        /// <summary>
        /// User-defined field 4. Developer can store additional data in this field.
        /// </summary>
        [MaxLength(256)]
        public string UserDefine4 { get; set; }

        /// <summary>
        /// The gateway name defined by PayFabric such as FirstDataGGe4, PayflowPro or Paymentech. This field will be set only if this card is a tokenized value for a specific gateway, such as FirstData or Paypal
        /// </summary>
        public string Connector { get; set; }

        /// <summary>
        /// Gateway token. PayFabric send this value to gateway for processing a transaction
        /// </summary>
        public string GatewayToken { get; set; }

        /// <summary>
        /// This field is required for UK debit cards
        /// </summary>
        public string IssueNumber { get; set; }

        /// <summary>
        /// This field is required for UK debit cards, format is MMYY.
        /// </summary>
        public string StartDate { get; set; }

        /// <summary>
        /// This field is used to submit new customer number for updating this record's customer field.
        /// </summary>
        public string NewCustomerNumber { get; set; }

        /// <summary>
        /// This is a response field, the possible value is 'Credit', 'Debit' or 'Prepaid' for credit card, and it is blank for eCheck.
        /// </summary>
        public string CardType { get; set; }

    }

  

    
}
