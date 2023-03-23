using BookOrderingSystem.DataAccess.Repositories.Interfaces;
using BookOrderingSystem.Domain.Models;
using BookOrderingSystem.Domain.Models.ViewModels;
using BookOrderingSystem.Utility.Details;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace BookOrderingSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #region Razor Pages
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Detail(int ID)
        {
            OrderViewModel orderViewModel = new OrderViewModel()
            {
                OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == ID, "ApplicationUser"),
                OrderDetails = _unitOfWork.OrderDetail.GetAll(u => u.OrderId == ID, null, "Book")
            };
            return View(orderViewModel);
        }
        #endregion

        #region API Calls
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> orderHeadersList;

            if (User.IsInRole(StaticDetail.Role.Admin) || User.IsInRole(StaticDetail.Role.Employee))
            {
                orderHeadersList = _unitOfWork.OrderHeader.GetAll(null, null, "ApplicationUser");
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                orderHeadersList = _unitOfWork.OrderHeader.GetAll(u => u.UserId == claim.Value, null, "ApplicationUser");
            }

            switch (status)
            {
                case "pending":
                    orderHeadersList = orderHeadersList.Where(u => u.OrderStatus == StaticDetail.Payment.Pending);
                    break;
                case "inprocess":
                    orderHeadersList = orderHeadersList.Where(u => u.OrderStatus == StaticDetail.Order.Processing);
                    break;
                case "completed":
                    orderHeadersList = orderHeadersList.Where(u => u.OrderStatus == StaticDetail.Order.Shipped);
                    break;
                default:
                    break;
            }

            return Json(new { data = orderHeadersList });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = StaticDetail.Role.Admin + ", " + StaticDetail.Role.Employee)]
        public IActionResult UpdateOrderDetails(OrderViewModel orderViewModel)
        {
            var orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderViewModel.OrderHeader.Id);
            orderHeader.Name = orderViewModel.OrderHeader.Name;
            orderHeader.PhoneNumber = orderViewModel.OrderHeader.PhoneNumber;
            orderHeader.City = orderViewModel.OrderHeader.City;
            orderHeader.Region = orderViewModel.OrderHeader.Region;
            
            if(orderViewModel.OrderHeader.Carrier != null)
            {
                orderHeader.Carrier = orderViewModel.OrderHeader.Carrier;
            }
            if (orderViewModel.OrderHeader.TrackingNumber != null)
            {
                orderHeader.TrackingNumber = orderViewModel.OrderHeader.TrackingNumber;
            }

            _unitOfWork.OrderHeader.Update(orderHeader);
            _unitOfWork.Save();
            TempData["Success"] = "Order Details updated successfully";
            return RedirectToAction("Detail", "Order", new { ID = orderHeader.Id});
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = StaticDetail.Role.Admin + ", " + StaticDetail.Role.Employee)]
        public IActionResult StartProcessing(OrderViewModel orderViewModel)
        {
            _unitOfWork.OrderHeader.UpdateStatus(orderViewModel.OrderHeader.Id, StaticDetail.Order.Processing);
            _unitOfWork.Save();
            TempData["Success"] = "Order Status updated successfully";
            return RedirectToAction("Detail", "Order", new { ID = orderViewModel.OrderHeader.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = StaticDetail.Role.Admin + ", " + StaticDetail.Role.Employee)]
		public IActionResult ShipOrder(OrderViewModel orderViewModel)
        {
            var orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderViewModel.OrderHeader.Id);
            orderHeader.TrackingNumber = orderViewModel.OrderHeader.TrackingNumber;
            orderHeader.Carrier = orderViewModel.OrderHeader.Carrier;
            orderHeader.OrderStatus = StaticDetail.Order.Shipped;
            orderHeader.ShippedDate = DateTime.Now;

            if(orderHeader.PaymentStatus == StaticDetail.Payment.Delayed)
            {
                orderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
            }

            _unitOfWork.OrderHeader.Update(orderHeader);
            _unitOfWork.Save();
            TempData["Success"] = "Order shipped successfully";
            return RedirectToAction("Detail", "Order", new { ID = orderViewModel.OrderHeader.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = StaticDetail.Role.Admin + ", " + StaticDetail.Role.Employee)]
		public IActionResult CancelOrder(OrderViewModel orderViewModel)
        {
            var orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderViewModel.OrderHeader.Id);
            
            if(orderHeader.PaymentStatus == StaticDetail.Payment.Approved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId
                };

                var service = new RefundService();
                Refund refund = service.Create(options);

                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, StaticDetail.Order.Cancelled, StaticDetail.Payment.Refunded);
            }
            else
            {
                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, StaticDetail.Order.Cancelled, StaticDetail.Payment.Cancelled);
            }

            _unitOfWork.Save();
            TempData["Success"] = "Order cancelled successfully";
            return RedirectToAction("Detail", "Order", new { ID = orderViewModel.OrderHeader.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = StaticDetail.Role.Admin + ", " + StaticDetail.Role.Employee)]
		public IActionResult DelayedPayment(OrderViewModel orderViewModel)
        {
            orderViewModel.OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderViewModel.OrderHeader.Id, "ApplicationUser");

            orderViewModel.OrderDetails = _unitOfWork.OrderDetail.GetAll(u => u.OrderId == orderViewModel.OrderHeader.Id, null, "Book");

            var domain = "https://localhost:44389/";

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string>()
                    {
                        "card"
                    },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domain + $"Admin/Order/PaymentConfirmation?orderHeaderID={orderViewModel.OrderHeader.Id}",
                CancelUrl = domain + $"Admin/Order/Detail?ID={orderViewModel.OrderHeader.Id}",
            };

            foreach (var item in orderViewModel.OrderDetails)
            {
                var sessionLine = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Book.Title
                        },
                    },
                    Quantity = item.Count,
                };
                options.LineItems.Add(sessionLine);
            }

            var service = new SessionService();
            Session session = service.Create(options);
            _unitOfWork.OrderHeader.UpdatePaymentStatus(orderViewModel.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public IActionResult PaymentConfirmation(int orderHeaderID)
        {
            var orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(i => i.Id == orderHeaderID);

            if (orderHeader.PaymentStatus == StaticDetail.Payment.Delayed)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeader.UpdatePaymentStatus(orderHeader.Id, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, orderHeader.OrderStatus, StaticDetail.Payment.Approved);
                    _unitOfWork.Save();
                }
            }

            return View(orderHeaderID);
        }

        #endregion
    }
}
