using System;
using System.Collections.Generic;
using System.Text;

namespace PayFabric.Net.Models
{
    public partial class Document
    {
        /// <summary>
        /// An array of key-value pairs. Usually, the key-value pairs represent the level 2 fields to submit to the gateway.
        /// </summary>
        public ICollection<NameValue> Head { get; set; }

        /// <summary>
        /// An array of columns object.The columns objects represent the columns that belong to a specific line. Usually the columns objects represent the level 3 fields that you can submit to the gateway. Columns object is an array of key-value pairs. The key-value pairs represent the level 3 fields to submit to the gateway.
        /// </summary>
        public ICollection<DocumentLine> Lines { get; set; }

        /// <summary>
        /// Up to 50 key value pairs can be stored in this object. Note: Adding DisableEmailReceipt under UserDefined to disable PayFabric's payment receipt to be sent out for the processed transaction, the possible values are True and False. If the value is True, this will disable the payment receipt to be sent out for the processed transaction. If the value is False or empty or the field is not submitted, this will enable the payment receipt to be sent out for the processed transaction based on the configuration of Payment Receipt.
        /// </summary>
        public ICollection<NameValue> UserDefined { get; set; }


        /// <summary>
        /// Address object. no restriction to these address fields, partially input is acceptable.
        /// </summary>
        public Address DefaultBillTo { get; set; }
    }
}
