using BookOrderingSystem.DataAccess.Data;
using BookOrderingSystem.DataAccess.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookOrderingSystem.DataAccess.Repositories
{
	public class UnitOfWork : IUnitOfWork
	{
		private readonly ApplicationDbContext _dbContext;

		public UnitOfWork(ApplicationDbContext dbContext)
		{
			_dbContext = dbContext;
			Category = new CategoryRepository(_dbContext);
			CoverType = new CoverTypeRepository(_dbContext);
			Book = new BookRepository(_dbContext);
			Company = new CompanyRepository(_dbContext);
			ApplicationUser = new ApplicationUserRepository(_dbContext);
			ShoppingCart = new ShoppingCartRepository(_dbContext);
			OrderHeader = new OrderHeaderRepository(_dbContext);
			OrderDetail = new OrderDetailRepository(_dbContext);
		}

		public ICategoryRepository Category { get; set; }

		public ICoverTypeRepository CoverType { get; set; }

		public IBookRepository Book { get; set; }

		public ICompanyRepository Company { get; set; }

		public IApplicationUserRepository ApplicationUser { get; set; }

		public IShoppingCartRepository ShoppingCart { get; set; }

		public IOrderHeaderRepository OrderHeader { get; set; }

		public IOrderDetailRepository OrderDetail { get; set; }

		public void Save()
		{
			_dbContext.SaveChanges();
		}
	}
}
