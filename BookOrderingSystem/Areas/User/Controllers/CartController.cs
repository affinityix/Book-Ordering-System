using BookOrderingSystem.DataAccess.Repositories.Interfaces;
using BookOrderingSystem.Domain.Models;
using BookOrderingSystem.Domain.Models.ViewModels;
using BookOrderingSystem.Utility.Details;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe.Checkout;
using System.Security.Claims;

namespace BookOrderingSystem.Areas.Template.Controllers
{
    [Area("User")]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IWebHostEnvironment _webHostEnvironment;

        private readonly IEmailSender _emailSender;

        public CartController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment, IEmailSender emailSender)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
            _emailSender = emailSender;
        }

        #region Razor Page
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;

            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartViewModel cartViewModel = new ShoppingCartViewModel()
            {
                CartList = _unitOfWork.ShoppingCart.GetAll(u => u.UserId == claim.Value, null, "Book"),
                OrderHeader = new OrderHeader()
            };

            foreach (var item in cartViewModel.CartList)
            {
                item.Price = GetPriceByQuantity(item.Count, item.Book.Price, item.Book.Price50, item.Book.Price100);
                cartViewModel.OrderHeader.OrderTotal += (item.Price * item.Count);
            }

            return View(cartViewModel);
        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;

            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartViewModel cartViewModel = new ShoppingCartViewModel()
            {
                CartList = _unitOfWork.ShoppingCart.GetAll(u => u.UserId == claim.Value, null, "Book"),
                OrderHeader = new OrderHeader()
            };

            cartViewModel.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);
            cartViewModel.OrderHeader.Name = cartViewModel.OrderHeader.ApplicationUser.FullName;
            cartViewModel.OrderHeader.PhoneNumber = cartViewModel.OrderHeader.ApplicationUser.PhoneNumber;
            cartViewModel.OrderHeader.City = cartViewModel.OrderHeader.ApplicationUser.CityAddress;
            cartViewModel.OrderHeader.Region = cartViewModel.OrderHeader.ApplicationUser.RegionName;

            foreach (var item in cartViewModel.CartList)
            {
                item.Price = GetPriceByQuantity(item.Count, item.Book.Price, item.Book.Price50, item.Book.Price100);
                cartViewModel.OrderHeader.OrderTotal += (item.Price * item.Count);
            }

            return View(cartViewModel);
        }

        #endregion

        #region API Calls
        public IActionResult Add(int cartID)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartID);
            _unitOfWork.ShoppingCart.IncrementCount(cart, 1);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Substract(int cartID)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartID);

            if (cart.Count <= 1)
            {
                _unitOfWork.ShoppingCart.Remove(cart);
                _unitOfWork.Save();
            }
            else
            {
                _unitOfWork.ShoppingCart.DecrementCount(cart, 1);
                _unitOfWork.Save();
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartID)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartID);
            _unitOfWork.ShoppingCart.Remove(cart);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ActionName("Summary")]
        [ValidateAntiForgeryToken]
        public IActionResult SummaryPost(ShoppingCartViewModel cartViewModel)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            cartViewModel.CartList = _unitOfWork.ShoppingCart.GetAll(u => u.UserId == claim.Value, null, "Book");
            cartViewModel.OrderHeader.OrderedDate = DateTime.Now;
            cartViewModel.OrderHeader.UserId = claim.Value;

            foreach (var item in cartViewModel.CartList)
            {
                item.Price = GetPriceByQuantity(item.Count, item.Book.Price, item.Book.Price50, item.Book.Price100);
                cartViewModel.OrderHeader.OrderTotal += (item.Price * item.Count);
            }

            var applicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);

            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                cartViewModel.OrderHeader.PaymentStatus = StaticDetail.Payment.Pending;
                cartViewModel.OrderHeader.OrderStatus = StaticDetail.Order.Pending;
            }
            else
            {
                cartViewModel.OrderHeader.PaymentStatus = StaticDetail.Payment.Pending;
                cartViewModel.OrderHeader.OrderStatus = StaticDetail.Order.Approved;
            }

            _unitOfWork.OrderHeader.Add(cartViewModel.OrderHeader);
            _unitOfWork.Save();

            foreach (var item in cartViewModel.CartList)
            {
                OrderDetail orderDetail = new OrderDetail()
                {
                    OrderId = cartViewModel.OrderHeader.Id,
                    BookId = item.BookId,
                    Price = item.Price,
                    Count = item.Count
                };

                _unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.Save();
            }

            if(applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                var domain = "https://localhost:44389/";

                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string>()
                    {
                        "card"
                    },
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                    SuccessUrl = domain + $"Template/Cart/OrderConfirmation?id={cartViewModel.OrderHeader.Id}",
                    CancelUrl = domain + $"Template/Cart/Index",
                };

                foreach (var item in cartViewModel.CartList)
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
                _unitOfWork.OrderHeader.UpdatePaymentStatus(cartViewModel.OrderHeader.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.Save();

                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
            } else
            {
                return RedirectToAction("OrderConfirmation", "Cart", new { id = cartViewModel.OrderHeader.Id });
            }
        }

        public IActionResult OrderConfirmation(int ID)
        {
            var orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(i => i.Id == ID);

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var applicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);

            orderHeader.ApplicationUser.Email = applicationUser.Email;

            if (orderHeader.PaymentStatus != StaticDetail.Payment.Delayed)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeader.UpdatePaymentStatus(orderHeader.Id, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, StaticDetail.Order.Approved, StaticDetail.Payment.Approved);
                    _unitOfWork.Save();
                }
            }

            _emailSender.SendEmailAsync(orderHeader.ApplicationUser.Email, "Order Confirmation", "New Order Created");

            var cartList = _unitOfWork.ShoppingCart.GetAll(u => u.UserId == orderHeader.UserId, null, null).ToList();
            _unitOfWork.ShoppingCart.RemoveRange(cartList);
            _unitOfWork.Save();

            return View(ID);
        }

        #endregion

        public double GetPriceByQuantity(int quantity, double price, double price50, double price100)
        {
            if (quantity <= 50)
            {
                return price;
            }
            else if (quantity <= 100)
            {
                return price50;
            }
            else
            {
                return price100;
            }
        }
    }
}
