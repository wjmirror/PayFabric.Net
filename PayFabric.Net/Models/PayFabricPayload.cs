﻿using PayFabric.Net.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PayFabric.Net.Models
{
    public class PayFabricPayload
    {
        /// <summary>
        /// PayFabric transaction key. Generated by PayFabric and returned to the client upon creation of the transaction. Omit this field when creating a new transaction.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Transaction amount, only accept 2 decimals.
        /// </summary>
        public string Amount { get; set; }

        /// <summary>
        /// Customer ID
        /// </summary>
        public string Customer { get; set; }

        /// <summary>
        /// Currency code, such as USD.
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Card object. If you are using an existing card, you only need to specify the ID of the card. If using a new card then all fields are required.
        /// </summary>
        public PayFabricCard Card { get; set; }

        /// <summary>
        /// Gateway account profile name. This name is configurable and is defined by the client on the PayFabric web portal.
        /// </summary>
        public string SetupId { get; set; }


        public string ReferenceKey { get; set; }

        
      

        
        public string Type { get; set; }
      
        public PayFabricExtendedInformation Document { get; set; }

    }
}
