using BookOrderingSystem.DataAccess.Repositories.Interfaces;
using BookOrderingSystem.Domain.Models;
using BookOrderingSystem.Domain.Models.ViewModels;
using BookOrderingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BookOrderingSystem.Areas.Template.Controllers
{
    [Area("User")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        #region Razor Pages
        public IActionResult Index()
        {
            IEnumerable<Book> books = _unitOfWork.Book.GetAll(x => !x.IsDeleted, null, "Category,CoverType");
            return View(books);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Details(int bookID)
        {
            ShoppingCart cart = new ShoppingCart()
            {
                Count = 1,
                BookId = bookID,
                Book = _unitOfWork.Book.GetFirstOrDefault(x => x.Id == bookID, includeProperties: "Category,CoverType")
            };

            return View(cart);

        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        #endregion

        #region API Calls
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            
            shoppingCart.UserId = claim.Value;

            ShoppingCart cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u =>
                u.UserId == claim.Value && u.BookId == shoppingCart.BookId);

            if(cart == null)
            {
                _unitOfWork.ShoppingCart.Add(shoppingCart);
            } else
            {
                _unitOfWork.ShoppingCart.IncrementCount(cart, shoppingCart.Count);
            }

            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }
        #endregion
    }
}