using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookOrderingSystem.Domain.Models
{
    public class ShoppingCart
    {
        [Key]
        public int Id { get; set; }

        public int BookId { get; set; }

        [Range(1, 1000, ErrorMessage = "Enter a value from 1 to 1000")]
        public int Count { get; set; }

        public string UserId { get; set; }

        [ForeignKey("BookId")]
        public Book? Book { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser? ApplicationUser { get; set; }

        [NotMapped]
        public double Price { get; set; }
    }
}
