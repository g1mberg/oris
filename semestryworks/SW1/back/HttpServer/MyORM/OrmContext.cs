using Npgsql;

namespace MyORM;

public class OrmContext(string connectionString)
{
    public T Create<T>(T entity, string tableName) where T : class, new()
    {
        using var dataSource = NpgsqlDataSource.Create(connectionString);
        var properties = typeof(T).GetProperties().ToList();
        var columns = properties.Skip(1).Select(p => p.Name.ToLower());

        var cmd = dataSource.CreateCommand(
            $"INSERT INTO {tableName.ToLower()} ( {string.Join(",", columns)} ) VALUES ( {string.Join(",", columns.Select(c => "@" + c))} ) RETURNING *;");

        foreach (var p in properties.Skip(1))
            cmd.Parameters.AddWithValue(p.Name.ToLower(), p.GetValue(entity) ?? DBNull.Value);

        try
        {
            using var r = cmd.ExecuteReader();
            return r.Read() ? MapRecord<T>(r) : entity;
        }
        catch (Exception e)
        {
            Console.WriteLine("ORM ERROR: " + e);
            throw;
        }
    }

    public T? ReadById<T>(int id, string tableName) where T : class, new()
    {
        using var dataSource = NpgsqlDataSource.Create(connectionString);

        var sql = $"SELECT * FROM {tableName.ToLower()} WHERE id = @id LIMIT 1";
        var cmd = dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@id", id);

        using var r = cmd.ExecuteReader();
        return r.Read() ? MapRecord<T>(r) : null;
    }

    public void Update<T>(int id, T entity, string tableName)
    {
        using var dataSource = NpgsqlDataSource.Create(connectionString);

        var properties = typeof(T).GetProperties().ToList();
        var sets = string.Join(",", properties.Skip(1).Select(p => $"{p.Name.ToLower()}=@{p.Name.ToLower()}"));
        var sql = $"UPDATE {tableName.ToLower()} SET {sets} WHERE id=@id";
        var cmd = dataSource.CreateCommand(sql);

        foreach (var p in properties.Skip(1))
            cmd.Parameters.AddWithValue(p.Name.ToLower(), p.GetValue(entity) ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@id", id);
        try
        {
            cmd.ExecuteNonQuery();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public void Delete(int id, string tableName)
    {
        using var dataSource = NpgsqlDataSource.Create(connectionString);
        var sql = $"DELETE FROM {tableName.ToLower()} WHERE id = @id";
        var cmd = dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@id", id);
        try
        {
            cmd.ExecuteNonQuery();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private static T MapRecord<T>(NpgsqlDataReader r) where T : class, new()
    {
        var obj = new T();
        var properties = typeof(T).GetProperties().ToList();
        var columns = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < r.FieldCount; i++) columns[r.GetName(i)] = i;

        foreach (var p in properties)
        {
            var key = p.Name;
            if (!columns.TryGetValue(key, out var idx) && !columns.TryGetValue(key.ToLower(), out idx)) continue;
            var val = r.IsDBNull(idx) ? null : r.GetValue(idx);
            var t = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
            p.SetValue(obj, val == null ? null : Convert.ChangeType(val, t));
        }

        return obj;
    }

    public List<T> ReadAll<T>(string tableName) where T : class, new()
    {
        using var dataSource = NpgsqlDataSource.Create(connectionString);
        var cmd = dataSource.CreateCommand($"SELECT * FROM {tableName.ToLower()}");
        try
        {
            using var r = cmd.ExecuteReader();
            var list = new List<T>();
            while (r.Read()) list.Add(MapRecord<T>(r));
            return list;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}