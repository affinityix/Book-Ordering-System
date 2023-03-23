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
	public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
	{
		private readonly ApplicationDbContext _dbContext;

		public OrderHeaderRepository(ApplicationDbContext dbContext) : base(dbContext)
		{
			_dbContext = dbContext;
		}

		public void Update(OrderHeader orderHead)
		{
			_dbContext.OrderHeaders.Update(orderHead);
		}

		public void UpdateStatus(int Id, string orderStatus, string? paymentStatus)
		{
			var order = _dbContext.OrderHeaders.FirstOrDefault(u => u.Id == Id);

			if (order != null)
			{
				order.OrderStatus = orderStatus;

				if (paymentStatus != null)
				{
					order.PaymentStatus = paymentStatus;
				}
			}
		}

		public void UpdatePaymentStatus(int Id, string sessionID, string paymentIntentID)
		{
			var order = _dbContext.OrderHeaders.FirstOrDefault(u => u.Id == Id);

			if (order != null)
			{
				order.PaymentDate = DateTime.Now;
				order.SessionId = sessionID;
				order.PaymentIntentId = paymentIntentID;
			}
		}
	}
}
