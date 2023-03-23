using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BookOrderingSystem.Domain.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string FullName { get; set; }

        [Display(Name = "City")]
        public string? CityAddress { get; set; }

        [Display(Name = "Region")]
        public string? RegionName { get; set; }

        public int? CompanyId { get; set; }

        [ForeignKey("CompanyID")]
        public Company? Company { get; set; }

    }
}
