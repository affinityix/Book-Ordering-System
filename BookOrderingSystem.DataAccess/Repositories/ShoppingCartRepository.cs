using BookOrderingSystem.DataAccess.Data;
using BookOrderingSystem.DataAccess.Repositories.Interfaces;
using BookOrderingSystem.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookOrderingSystem.DataAccess.Repositories
{
	public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
	{
		private readonly ApplicationDbContext _dbContext;

		public ShoppingCartRepository(ApplicationDbContext dbContext) : base(dbContext)
		{
			_dbContext = dbContext;
		}

		public int DecrementCount(ShoppingCart cart, int count)
		{
			cart.Count -= count;
			return cart.Count;
		}

		public int IncrementCount(ShoppingCart cart, int count)
		{
			cart.Count += count;
			return cart.Count;
		}
	}
}
