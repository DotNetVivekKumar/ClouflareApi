using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CloudflareAPI.Models
{
    public class IPAddressModel
    {
        [Required(ErrorMessage = "Please enter at least one IP address")]
        [RegularExpression(@"^(\d{1,3}\.){3}\d{1,3}(,\s*(\d{1,3}\.){3}\d{1,3})*$", ErrorMessage = "Please enter valid comma-separated IP addresses")]
        public string IPAddress { get; set; }
    }
}