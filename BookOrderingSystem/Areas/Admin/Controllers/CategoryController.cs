using BookOrderingSystem.DataAccess.Repositories.Interfaces;
using BookOrderingSystem.Domain.Models;
using BookOrderingSystem.Utility.Details;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookOrderingSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticDetail.Role.Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #region Razor Calls
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Upsert(int? Id)
        {
            Category category = new Category();

            if (Id == null)
            {
                return View(category);
            }

            category = _unitOfWork.Category.GetFirstOrDefault(u => u.Id == Id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        public IActionResult Delete(int Id)
        {
            Category category = _unitOfWork.Category.GetFirstOrDefault(u => u.Id == Id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }
        #endregion

        #region API Calls
        public IActionResult GetAll()
        {
            var categories = _unitOfWork.Category.GetAll(x => !x.IsDeleted, null, null);
            return Json(new { data = categories });
        }

        [HttpPost, ActionName("Upsert")]
        public IActionResult UpsertPost(Category category)
        {
            if (ModelState.IsValid)
            {
                if (category.Id == 0)
                {
                    category.CreatedAt = DateTime.Now;
                    _unitOfWork.Category.Add(category);
                    TempData["Success"] = "Category added successfully";
                }
                else
                {
                    category.LastModifiedAt = DateTime.Now;
                    _unitOfWork.Category.Update(category);
                    TempData["Info"] = "Category altered successfully";
                }
                _unitOfWork.Save();

                return RedirectToAction("Index");
            }
            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int id)
        {
            var category = _unitOfWork.Category.GetFirstOrDefault(u => u.Id == id);

            if (category != null)
            {
                _unitOfWork.Category.Delete(category);
                TempData["Delete"] = "Category deleted successfully";
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
