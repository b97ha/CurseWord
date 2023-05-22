using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MovieProfanityDetector.Data.Repository
{
    public interface IRepository<T> where T : class
    {
        IQueryable<T> Select();

        Task<List<T>> SelectAll();

        Task<T> SelectById(object id);

        Task CreateAsync(T entity);

        Task CreateAsync(IEnumerable<T> entity);

        void Update(T entity);

        void Update(IEnumerable<T> entity);

        void Delete(T entity);
        void Delete(IEnumerable<T> entity);
    }
}
