using System;
using System.Threading.Tasks;

namespace MovieProfanityDetector.Data
{
    public class ContextSeed
    {
        private readonly ApplicationDbContext _dbContext;

        public ContextSeed(ApplicationDbContext dbContext
        )
        {
            _dbContext = dbContext;
        }

        public async Task SeedAsync()
        {
            try
            {
                await _dbContext.DisposeAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}