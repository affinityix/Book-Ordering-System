using BookOrderingSystem.Domain.Models.Shared;
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
    public class Book : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string ISBN { get; set; }

        [Required]
        public string Author { get; set; }

		[Display(Name = "List Price")]
		public double ListPrice { get; set; }

		[Display(Name = "Price for 50 books")]
		public double Price50 { get; set; }

        [Display(Name = "Price for 100 books")]
		public double Price100 { get; set; }

        [Display(Name = "Image URL")]
		public string? ImageURL { get; set; }
	
        public double Price { get; set; }

        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }
    }
}
