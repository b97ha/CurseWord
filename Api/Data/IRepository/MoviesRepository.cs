using MovieProfanityDetector.Data.Repository;
using MovieProfanityDetector.Models.Entities;

namespace MovieProfanityDetector.Data.IRepository
{

    public class MoviesRepository : Repository<Movie>, IMoviesRepository
    {
        public MoviesRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
