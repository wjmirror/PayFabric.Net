﻿using Newtonsoft.Json;
using PayFabric.Net.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PayFabric.Net.Models
{
    public class Transaction
    {
        /// <summary>
        /// PayFabric transaction key. Generated by PayFabric and returned to the client upon creation of the transaction. Omit this field when creating a new transaction.
        /// </summary>
        [MaxLength(64)]
        public string Key { get; set; }

        /// <summary>
        /// Transaction amount, only accept 2 decimals.
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        /// Customer ID
        /// </summary>
        [MaxLength(128)]
        public string Customer { get; set; }

        /// <summary>
        /// Currency code, such as USD.
        /// </summary>
        [MaxLength(16)]
        public string Currency { get; set; }

        /// <summary>
        /// Card object. If you are using an existing card, you only need to specify the ID of the card. If using a new card then all fields are required.
        /// </summary>
        public Card Card { get; set; }

        /// <summary>
        /// Gateway account profile name. This name is configurable and is defined by the client on the PayFabric web portal.
        /// </summary>
        [MaxLength(64)]
        public string SetupId { get; set; }


        /// <summary>
        /// Tender type. Valid values are CreditCard, ECheck.
        /// </summary>
        [MaxLength(64)]
        public string Tender { get; set; }

        /// <summary>
        /// Transaction type. Valid values are Sale,Book,Ship,Void,Credit, Force. 
        /// For more information on PayFabric Transaction Types, click <see cref="PayFabricTransactionType"/>
        /// </summary>
        [MaxLength(64)]
        public string Type { get; set; }

        /// <summary>
        /// Batch number name. For saving this transaction into a PayFabric batch. Merchant can process the batch on PayFabric portal. For Verify transaction type, the value in this attribute will be removed automatically.
        /// </summary>
        [MaxLength(64)]
        public string BatchNumber { get; set; }

        /// <summary>
        /// Timestamp indicating when this transaction was last modified. It's format should like "3/23/2015 11:16:19 PM".
        /// </summary>
        public DateTime? ModifiedOn { get; set; }

        /// <summary>
        /// Ship to Address
        /// </summary>
        public Address Shipto { get; set; }

        /// <summary>
        /// Authorization Code, Required for Force transactions.
        /// </summary>
        [MaxLength(64)]
        public string ReqAuthCode { get; set; }

        /// <summary>
        /// The original transaction key if this transaction is a reference transaction
        /// </summary>
        [MaxLength(64)]
        public string ReferenceKey { get; set; }

        /// <summary>
        /// Required by FirstData for Void,Ship and reference Credit transactions.
        /// </summary>
        [MaxLength(64)]
        public string ReqTrxTag { get; set; }

        /// <summary>
        /// Transaction response from Payment Gateway.
        /// </summary>
        public TransactionResponse TrxResponse { get; set; }

        /// <summary>
        /// Level 2/3 transaction fields, as well as User Defined fields.
        /// </summary>
        public Document Document { get; set; }

        /// <summary>
        /// Array of a <see cref="SimpleTransaction"/> which represents the original transactions. Value is Set if this transaction is a reference transaction.
        /// </summary>
        public ICollection<SimpleTransaction> ReferenceTrxs { get; set; }

        /// <summary>
        /// User Defined field 1
        /// </summary>
        [MaxLength(256)]
        public string TrxUserDefine1 { get; set; }

        /// <summary>
        /// User Defined field 2
        /// </summary>
        [MaxLength(256)]
        public string TrxUserDefine2 { get; set; }

        /// <summary>
        /// User Defined field 3
        /// </summary>
        [MaxLength(256)]
        public string TrxUserDefinee { get; set; }

        /// <summary>
        /// User Defined field 4
        /// </summary>
        [MaxLength(256)]
        public string TrxUserDefine4 { get; set; }

        /// <summary>
        /// GUID of gateway account profile for this transaction. Developer can utilize this field
        /// </summary>
        public Guid? MSO_EngineGUID { get; set; }

        /// <summary>
        /// A future date to process this transaction. In another word, this transaction won't be processed right away by setting this field. It's format should like "3/23/2015". For Verify transaction type, the value in this attribute will be removed automatically.
        /// </summary>
        public DateTime? PayDate { get; set; }


        /// <summary>
        /// The authorization type of the transaction, valid values are Reauthorization, Resubmission, Incremental or NotSet
        /// </summary>
        [MaxLength(25)]
        public string AuthorizationType { get; set; }

        /// <summary>
        /// The type authorization of transaction to be processed, valid values are Unscheduled, ScheduledInstallment, ScheduledRecurring or NotSet
        /// </summary>
        [MaxLength(25)]
        public string TrxSchedule { get; set; }


        /// <summary>
        /// The entity that initiated the transaction, valid values are Merchant, Customer or NotSet
        /// </summary>
        [MaxLength(25)]
        public string TrxInitiation { get; set; }

        /// <summary>
        /// The identifier that specifies whether the card used on the transaction is a stored credential or newly entered, valid values are Entered or Stored
        /// </summary>
        [MaxLength(25)]
        public string CCEntryIndicator { get; set; }
    }
}
