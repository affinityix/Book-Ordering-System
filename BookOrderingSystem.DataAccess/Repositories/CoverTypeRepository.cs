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
    public class CoverTypeRepository : Repository<CoverType>, ICoverTypeRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public CoverTypeRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public override void Add(CoverType coverType)
        {
            coverType.CreatedAt = DateTime.Now;
            base.Add(coverType);
        }

        public void Delete(CoverType coverType)
        {
            coverType.IsDeleted = true;
            _dbContext.CoverTypes.Update(coverType);
        }

        public void Update(CoverType coverType)
        {
            coverType.LastModifiedAt = DateTime.Now;
            _dbContext.CoverTypes.Update(coverType);
        }
    }
}
