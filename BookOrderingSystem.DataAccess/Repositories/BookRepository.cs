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
    public class BookRepository : Repository<Book>, IBookRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public BookRepository(ApplicationDbContext dbContext) : base(dbContext) 
        {
            _dbContext = dbContext;
        }

        public override void Add(Book book)
        {
            book.CreatedAt = DateTime.Now;
            base.Add(book);
        }

        public void Delete(Book book)
        {
            book.IsDeleted = true;
            _dbContext.Books.Update(book);
        }

        public void Update(Book book)
        {
            book.LastModifiedAt = DateTime.Now;
            _dbContext.Books.Update(book);
        }
    }
}
