using HttpServer.Models;

namespace HttpServer.Repository;

public interface IRepository<T> where T : class, new()
{
    IEnumerable<T> GetAll();
    T? GetById(int id);
    T Add(T entity);
    void Update(int id, T entity);
    void Delete(int id);
}