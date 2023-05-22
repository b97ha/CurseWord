using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MovieProfanityDetector.Data.Repository;

namespace MovieProfanityDetector.Data.IRepository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _dbContext;

        public Repository(ApplicationDbContext context) => _dbContext = context;

        public IQueryable<T> Select() => _dbContext.Set<T>().AsQueryable();

        public async Task<List<T>> SelectAll() => await _dbContext.Set<T>().ToListAsync<T>();

        public async Task<T> SelectById(object id) => await _dbContext.Set<T>().FindAsync(id);

        public async Task CreateAsync(T entity)
        {
            await _dbContext.Set<T>().AddAsync(entity);
        }

        public async Task CreateAsync(IEnumerable<T> entity)
        {
            await _dbContext.Set<T>().AddRangeAsync(entity);
        }

        public void Update(T entity)
        {
            _dbContext.Set<T>().Update(entity);
        }
        public void Update(IEnumerable<T> entity)
        {
            _dbContext.Set<T>().UpdateRange(entity);
        }
        public void Delete(T entity) => _dbContext.Set<T>().Remove(entity);
        public void Delete(IEnumerable<T> entity) => _dbContext.Set<T>().RemoveRange(entity);
    }
}