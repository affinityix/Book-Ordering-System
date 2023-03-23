using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookOrderingSystem.Domain.Models.ViewModels
{
    public class ShoppingCartViewModel
    {
        public IEnumerable<ShoppingCart> CartList { get; set; }
        
        public OrderHeader OrderHeader { get; set; }
    }
}
