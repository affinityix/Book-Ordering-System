using BookOrderingSystem.DataAccess.Repositories.Interfaces;
using BookOrderingSystem.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using BookOrderingSystem.Utility.Details;
using Microsoft.AspNetCore.Authorization;

namespace BookOrderingSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticDetail.Role.Admin)]
    public class CoverTypeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CoverTypeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #region Razor Calls
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Upsert(int? ID)
        {
            CoverType coverType = new CoverType();

            if (ID == null)
            {
                return View(coverType);
            }

            coverType = _unitOfWork.CoverType.GetFirstOrDefault(u => u.Id == ID);

            if (coverType == null)
            {
                return NotFound();
            }

            return View(coverType);
        }

        public IActionResult Delete(int ID)
        {
            CoverType coverType = _unitOfWork.CoverType.GetFirstOrDefault(u => u.Id == ID);

            if (coverType == null)
            {
                return NotFound();
            }

            return View(coverType);
        }
        #endregion

        #region API Calls
        [HttpGet]
        public IActionResult GetAll()
        {
            var coverTypes = _unitOfWork.CoverType.GetAll(x => !x.IsDeleted, null, null);
            return Json(new { data = coverTypes });
        }

        [HttpPost, ActionName("Upsert")]
        public IActionResult UpsertPost(CoverType coverType)
        {
            if (ModelState.IsValid)
            {
                if (coverType.Id == 0)
                {
                    coverType.CreatedAt = DateTime.Now;
                    _unitOfWork.CoverType.Add(coverType);
                    TempData["Success"] = "Cover Type added successfully";
                }
                else
                {
                    coverType.LastModifiedAt = DateTime.Now;
                    _unitOfWork.CoverType.Update(coverType);
                    TempData["Info"] = "Cover Type altered successfully";
                }
                _unitOfWork.Save();

                return RedirectToAction("Index");
            }
            return View(coverType);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int id)
        {
            var coverType = _unitOfWork.CoverType.GetFirstOrDefault(u => u.Id == id);

            if (coverType != null)
            {
                _unitOfWork.CoverType.Delete(coverType);
                TempData["Delete"] = $"{coverType.Title} deleted successfully";
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
