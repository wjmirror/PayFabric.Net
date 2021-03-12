using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PayFabric.Net.Models
{
    public class CardHolder
    {
        /// <summary>
        /// Driver license
        /// </summary>
        [MaxLength(32)]
        public string DriverLicense { get; set; }

        /// <summary>
        /// First name
        /// </summary>
        [MaxLength(64)]
        public string FirstName { get; set; }

        /// <summary>
        /// Last name
        /// </summary>
        [MaxLength(64)]
        public string LastName { get; set; }

        /// <summary>
        /// Middle name
        /// </summary>
        [MaxLength(64)]
        public string MiddleName { get; set; }

        /// <summary>
        /// Social security number
        /// </summary>
        [MaxLength(16)]
        public string SSN { get; set; }
    }
}
