using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BookOrderingSystem.Domain.Models
{
    public class Company
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Company Name")]
        public string Name { get; set; }

        [Display(Name = "Street Address")]
        public string StreetAddress { get; set; }

        [Display(Name = "Residential City")]
        public string City { get; set; }

        [Display(Name = "State")]
        public string State { get; set; }

        [Display(Name = "Postal Code")]
        public int PostalCode { get; set; }

        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
