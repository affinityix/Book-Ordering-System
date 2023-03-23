using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookOrderingSystem.Domain.Models
{
    public class OrderDetail
    {
        [Key]
        public int Id { get; set; }

        public int OrderId { get; set; }

        public int BookId { get; set; }

        public int Count { get; set; }

        public double Price { get; set; }

        [ForeignKey("OrderId")]
        public OrderHeader? OrderHeader { get; set; }

        [ForeignKey("BookId")]
        public Book? Book { get; set; }
    }
}
