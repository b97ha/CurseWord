using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using MovieProfanityDetector.Data.Repository;

namespace MovieProfanityDetector.Data.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
         IMoviesRepository Movies { get; }

        int Complete();
        Task<IdentityResult> CompleteAsync();

    }
}
