﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PayFabric.Net.Models
{
    public partial class Address
    {
        /// <summary>
        /// Unique identifier for this record
        /// </summary>
        public Guid? ID { get; set; }

        /// <summary>
        /// ID for this customer. This is generated by the client upon creation of the customer.
        /// </summary>
        [MaxLength(128)]
        public string Customer { get; set; }

        /// <summary>
        /// Country name
        /// </summary>
        [MaxLength(64)]
        public string Country { get; set; }

        /// <summary>
        /// State name
        /// </summary>
        [MaxLength(64)]
        public string State { get; set; }

        /// <summary>
        /// City name
        /// </summary>
        [MaxLength(64)]
        public string City { get; set; }

        /// <summary>
        /// Street line 1
        /// </summary>
        [MaxLength(128)]
        public string Line1 { get; set; }

        /// <summary>
        /// Street line 2
        /// </summary>
        [MaxLength(128)]
        public string Line2 { get; set; }

        /// <summary>
        /// Street line 3
        /// </summary>
        [MaxLength(128)]
        public string Line3 { get; set; }

        /// <summary>
        /// Email address
        /// </summary>
        [MaxLength(128)]
        public string Email { get; set; }

        /// <summary>
        /// Phone number
        /// </summary>
        [MaxLength(16)]
        public string Phone { get; set; }

        /// <summary>
        /// Timestamp indicating when this record was last modified. It's format should like "3/23/2015 11:16:19 PM".
        /// </summary>
        public string ModifiedOn { get; set; }

        /// <summary>
        /// Zip code
        /// </summary>
        [MaxLength(16)]
        public string Zip { get; set; }
    }
}