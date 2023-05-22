using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using MovieProfanityDetector.Data.Repository;

namespace MovieProfanityDetector.Data.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly ApplicationDbContext _context;


        public IMoviesRepository Movies { get; }

        public UnitOfWork(
            ApplicationDbContext context, IMoviesRepository books)
        {
            _context = context;
            Movies = books;
        }

        public int Complete() => _context.SaveChanges();

        public async Task<IdentityResult> CompleteAsync()
        {
            var save = await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }

       

      
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;
            _context.Dispose();
        }
    }
}