using BookOrderingSystem.Domain.Models.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookOrderingSystem.Domain.Models
{
    public class CoverType : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }
    }
}
