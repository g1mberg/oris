using Npgsql;
using System.Reflection;

namespace MyORM;

public class OrmContext(string connectionString)
{
    private static IEnumerable<(PropertyInfo Property, string ColumnName)> GetColumns<T>()
    {
        return typeof(T).GetProperties()
            .Where(p => p.GetCustomAttribute<ColumnAttribute>() != null)
            .Select(p => (
                Property: p,
                ColumnName: p.GetCustomAttribute<ColumnAttribute>()!.Name != null
                    ? p.GetCustomAttribute<ColumnAttribute>()!.Name!.ToLower()
                    : p.Name.ToLower()
            ))
            .ToList();
    }

    private static PropertyInfo? GetPrimaryKey<T>()
    {
        return typeof(T).GetProperties()
            .FirstOrDefault(p => p.GetCustomAttribute<PrimaryKeyAttribute>() != null);
    }

    public T Create<T>(T entity, string tableName) where T : class, new()
    {
        try
        {
            using var dataSource = NpgsqlDataSource.Create(connectionString);
            var columns = GetColumns<T>().ToList(); // только [Column]

            var columnNames = string.Join(",", columns.Select(c => c.ColumnName));
            var parameters = string.Join(",", columns.Select(c => "@" + c.ColumnName));

            var cmd = dataSource.CreateCommand(
                $"INSERT INTO {tableName.ToLower()} ({columnNames}) VALUES ({parameters}) RETURNING *;");

            foreach (var (prop, columnName) in columns)
            {
                var value = prop.GetValue(entity);
                if (value is DateTimeOffset dto) value = dto.UtcDateTime;
                cmd.Parameters.AddWithValue(columnName, value ?? DBNull.Value);
            }

            using var r = cmd.ExecuteReader();
            return r.Read() ? MapRecord<T>(r) : entity;
        }
        catch (Exception e)
        {
            Console.WriteLine("ORM ERROR CREATE: " + e);
            throw;
        }
    }

    public T? ReadById<T>(int id, string tableName) where T : class, new()
    {
        try
        {
            using var dataSource = NpgsqlDataSource.Create(connectionString);
            var sql = $"SELECT * FROM {tableName.ToLower()} WHERE id = @id LIMIT 1";
            var cmd = dataSource.CreateCommand(sql);
            cmd.Parameters.AddWithValue("@id", id);

            using var r = cmd.ExecuteReader();
            return r.Read() ? MapRecord<T>(r) : null;
        }
        catch (Exception e)
        {
            Console.WriteLine("ORM ERROR READBYID: " + e);
            throw;
        }
    }

    public void Update<T>(int id, T entity, string tableName)
    {
        try
        {
            using var dataSource = NpgsqlDataSource.Create(connectionString);
            var columns = GetColumns<T>().ToList(); 

            var sets = string.Join(",", columns.Select(c => $"{c.ColumnName}=@{c.ColumnName}"));
            var sql = $"UPDATE {tableName.ToLower()} SET {sets} WHERE id=@id";
            var cmd = dataSource.CreateCommand(sql);

            foreach (var (prop, columnName) in columns)
            {
                var value = prop.GetValue(entity);
                if (value is DateTimeOffset dto) value = dto.UtcDateTime;
                cmd.Parameters.AddWithValue(columnName, value ?? DBNull.Value);
            }

            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
        catch (Exception e)
        {
            Console.WriteLine("ORM ERROR UPDATE: " + e);
            throw;
        }
    }



    public void Delete(int id, string tableName)
    {
        try
        {
            using var dataSource = NpgsqlDataSource.Create(connectionString);
            var sql = $"DELETE FROM {tableName.ToLower()} WHERE id = @id";
            var cmd = dataSource.CreateCommand(sql);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
        catch (Exception e)
        {
            Console.WriteLine("ORM ERROR DELETE: " + e);
            throw;
        }
    }

    private static T MapRecord<T>(NpgsqlDataReader r) where T : class, new()
    {
        var obj = new T();
        var columns = GetColumns<T>().ToDictionary(c => c.ColumnName, c => c.Property);

        // Добавляем PrimaryKey для маппинга id
        var primaryKey = GetPrimaryKey<T>();
        if (primaryKey != null)
        {
            var pkColumnName = "id"; // предполагаем, что PK всегда "id"
            columns[pkColumnName] = primaryKey;
        }

        for (var i = 0; i < r.FieldCount; i++)
        {
            var columnName = r.GetName(i).ToLower();
            if (!columns.TryGetValue(columnName, out var prop)) continue;

            var val = r.IsDBNull(i) ? null : r.GetValue(i);
            var t = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
            prop.SetValue(obj, val == null ? null : Convert.ChangeType(val, t));
        }

        return obj;
    }

    public List<T> ReadAll<T>(string tableName) where T : class, new()
    {
        try
        {
            using var dataSource = NpgsqlDataSource.Create(connectionString);
            var cmd = dataSource.CreateCommand($"SELECT * FROM {tableName.ToLower()}");
            using var r = cmd.ExecuteReader();
            var list = new List<T>();
            while (r.Read()) list.Add(MapRecord<T>(r));
            return list;
        }
        catch (Exception e)
        {
            Console.WriteLine("ORM ERROR READALL: " + e);
            throw;
        }
    }
}