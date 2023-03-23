using BookOrderingSystem.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookOrderingSystem.DataAccess.Repositories.Interfaces
{
    public interface IOrderHeaderRepository : IRepository<OrderHeader>
    {
        void Update(OrderHeader orderHeader);

        void UpdateStatus(int Id, string orderStatus, string? paymentStatus = null);

        void UpdatePaymentStatus(int Id, string sessionID, string paymentIntentID);
    }
}
