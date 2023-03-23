using BookOrderingSystem.DataAccess.Repositories.Interfaces;
using BookOrderingSystem.Domain.Models.ViewModels;
using BookOrderingSystem.Utility.Details;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookOrderingSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticDetail.Role.Admin)]
    public class BookController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IWebHostEnvironment _webHostEnvironment;

        public BookController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }


        #region Razor Calls
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Upsert(int? ID)
        {
            var bookViewModel = new BookViewModel()
            {
                Book = new(),
                CategoryList = _unitOfWork.Category.GetAll(x => !x.IsDeleted, null, null)
                .Select(u => new SelectListItem
                {
                    Text = u.Title,
                    Value = u.Id.ToString()
                }),
                CoverTypeList = _unitOfWork.CoverType.GetAll(x => !x.IsDeleted, null, null)
                .Select(u => new SelectListItem
                {
                    Text = u.Title,
                    Value = u.Id.ToString()
                })
            };

            if (ID == null)
            {
                return View(bookViewModel);
            }

            bookViewModel.Book = _unitOfWork.Book.GetFirstOrDefault(x => x.Id == ID);

            if (bookViewModel.Book == null)
            {
                return NotFound();
            }

            return View(bookViewModel);
        }

        public IActionResult Delete(int ID)
        {
            BookViewModel bookViewModel = new BookViewModel();
            bookViewModel.Book = _unitOfWork.Book.GetFirstOrDefault(x => x.Id == ID) ;

            if (bookViewModel.Book != null)
            {
                return View(bookViewModel);
            }

            return NotFound();
        }
        #endregion

        #region API Calls
        [HttpGet]
        public IActionResult GetAll()
        {
            var books = _unitOfWork.Book.GetAll(x => !x.IsDeleted, null, "Category,CoverType");
            return Json(new { data = books });
        }

        [HttpPost, ActionName("Upsert")]
        public IActionResult UpsertPost(BookViewModel bookViewModel, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;

                if (imageFile != null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(wwwRootPath, @"images\books");
                    var extension = Path.GetExtension(imageFile.FileName);

                    if (bookViewModel.Book.ImageURL != null)
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, bookViewModel.Book.ImageURL.TrimStart('\\'));

                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Exists(oldImagePath);
                        }

                    }

                    using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        imageFile.CopyTo(fileStreams);
                    }

                    bookViewModel.Book.ImageURL = @"\images\books\" + fileName + extension;
                }

                if (bookViewModel.Book.Id == 0)
                {
                    bookViewModel.Book.CreatedAt = DateTime.Now;
                    _unitOfWork.Book.Add(bookViewModel.Book);
                    TempData["Success"] = "Book added successfully";
                }
                else
                {
                    bookViewModel.Book.LastModifiedAt = DateTime.Now;
                    _unitOfWork.Book.Update(bookViewModel.Book);
                    TempData["Info"] = "Book altered successfully";
                }
                _unitOfWork.Save();

                return RedirectToAction("Index");
            }
            return View(bookViewModel);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int id)
        {
            var book = _unitOfWork.Book.GetFirstOrDefault(u => u.Id == id);

            if (book != null)
            {
                _unitOfWork.Book.Delete(book);
                TempData["Delete"] = "Book deleted successfully";
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            else
            {
                return NotFound();
            }
        }
        #endregion
    }
}
