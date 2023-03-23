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
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public CategoryRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public override void Add(Category category)
        {
            category.CreatedAt = DateTime.Now;
            base.Add(category);
        
        }
        public void Delete(Category category)
        {
            category.LastModifiedAt = DateTime.Now;
            _dbContext.Categories.Update(category);
        }

        public void Update(Category category)
        {
            category.IsDeleted = true;
            _dbContext.Categories.Update(category);
        }
    }
}
