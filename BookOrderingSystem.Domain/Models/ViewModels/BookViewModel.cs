using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookOrderingSystem.Domain.Models.ViewModels
{
    public class BookViewModel
    {
        public Book Book { get; set; }

        public IEnumerable<SelectListItem>? CategoryList { get; set; }

        public IEnumerable<SelectListItem>? CoverTypeList { get; set; }
    }
}
