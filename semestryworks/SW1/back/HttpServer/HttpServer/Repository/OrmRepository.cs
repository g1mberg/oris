using HttpServer.Framework.Settings;
using HttpServer.Models;
using MyORM;

namespace HttpServer.Repository;

public class OrmRepository<T>(string tableName) : IRepository<T>
    where T : class, new()
{
    private readonly OrmContext _orm = new OrmContext(SettingsManager.Instance.Settings.ConnectionString!);
    public IEnumerable<T> GetAll() =>
        _orm.ReadAll<T>(tableName);

    public T? GetById(int id) =>
        _orm.ReadById<T>(id, tableName);

    public bool TryGetById(int id, out T? entity)
    {
        entity = _orm.ReadById<T>(id, tableName);
        return entity != null;
    }

    public T Add(T entity) =>
        _orm.Create(entity, tableName);

    public void Update(int id, T entity) =>
        _orm.Update(id, entity, tableName);

    public void Delete(int id) =>
        _orm.Delete(Convert.ToInt32(id), tableName);
    
    public List<string> GetDistinctValues(Func<T, IEnumerable<string>> selector)
    {
        return GetAll()
            .SelectMany(selector)
            .Distinct()
            .OrderBy(x => x)
            .ToList();
    }

    public List<string?> GetDistinctSingleValues(Func<T, string?> selector)
    {
        return GetAll()
            .Select(selector)
            .Where(x => x != null)
            .Distinct()
            .OrderBy(x => x!)
            .ToList();
    }
}
