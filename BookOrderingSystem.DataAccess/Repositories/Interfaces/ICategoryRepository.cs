using BookOrderingSystem.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookOrderingSystem.DataAccess.Repositories.Interfaces
{
    public interface ICategoryRepository : IRepository<Category>
    {
        void Update(Category category);

        void Delete(Category category);
    }
}
