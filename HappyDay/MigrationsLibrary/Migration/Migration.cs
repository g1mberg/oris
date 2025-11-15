using System.Data;
using System.Reflection;
using HttpServer.Framework.core.Attributes;
using HttpServer.Framework.Settings;
using Microsoft.Data.SqlClient;
using Npgsql;
using ColumnAttribute = System.ComponentModel.DataAnnotations.Schema.ColumnAttribute;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

// using System.ComponentModel.DataAnnotations.Schema;

namespace Migration;

[Endpoint]
public class MigrationEndpoints
{
    public void CreateDB()
    {
        String str;
        SqlConnection myConn = new SqlConnection(SettingsManager.Instance.Settings.ConnectionString);

        str = "CREATE DATABASE Migration ON PRIMARY " +
              "(NAME = MyDatabase_Data, " +
              "FILENAME = 'C:\\MyDatabaseData.mdf', " +
              "SIZE = 2MB, MAXSIZE = 10MB, FILEGROWTH = 10%)" +
              "LOG ON (NAME = MyDatabase_Log, " +
              "FILENAME = 'C:\\MyDatabaseLog.ldf', " +
              "SIZE = 1MB, " +
              "MAXSIZE = 5MB, " +
              "FILEGROWTH = 10%)";

        SqlCommand myCommand = new SqlCommand(str, myConn);
        try
        {
            myConn.Open();
            myCommand.ExecuteNonQuery();
        }
        catch (System.Exception ex)
        {
        }
        finally
        {
            if (myConn.State == ConnectionState.Open)
            {
                myConn.Close();
            }
        }
    }

    [HttpGet("/migrate/create")]
    public static void CreateMigration()
    {
        var assembly = Assembly.GetExecutingAssembly();

        var classesWithTable = assembly.GetTypes()
            .Where(t => t.GetCustomAttribute<TableAttribute>() != null)
            .Select(t => new
            {
                Type = t,
                TableName = t.GetCustomAttribute<TableAttribute>()!.Name
            })
            .ToList();

        List<Type> matched = new();
        List<Type> unmatched = new();

        foreach (var item in classesWithTable)
        {
            Console.WriteLine(item.TableName);
            if (Find(item.TableName))
                matched.Add(item.Type);
            else
                unmatched.Add(item.Type);
        }

        Console.WriteLine(matched.Count);

        CreateMissingTables(unmatched);
        UpdateExistingTables(matched);
    }

    private static bool Find(string name)
    {
        string connString = SettingsManager.Instance.Settings.ConnectionString;

        using (var conn = new NpgsqlConnection(connString))
        {
            conn.Open();

            string sql =
                @"SELECT table_name
                  FROM information_schema.tables
                  WHERE table_schema = 'public'";

            using (var cmd = new NpgsqlCommand(sql, conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                    if (reader.GetString(0).Equals(name))
                        return true;
            }

            return false;
        }
    }

    public static void CreateMissingTables(List<Type> unmatched)
    {
        string connString = SettingsManager.Instance.Settings.ConnectionString;

        using var conn = new NpgsqlConnection(connString);
        conn.Open();

        foreach (var type in unmatched)
        {
            string tableName = type.GetCustomAttribute<TableAttribute>()!.Name;
            
            var props = type.GetProperties()
                .Where(p =>
                    p.IsDefined(typeof(PrimaryKeyAttribute)) ||
                    p.IsDefined(typeof(ColumnAttribute)))
                .ToList();

            if (props.Count == 0)
            {
                Console.WriteLine($"⚠ Класс {type.Name} не содержит ни одного свойства с PrimaryKey/Column.");
                continue;
            }

            List<string> columns = new();

            foreach (var prop in props)
            {
                string colName = prop.Name;
                string sqlType = MapType(prop.PropertyType);

                bool isPrimaryKey = prop.IsDefined(typeof(PrimaryKeyAttribute));

                string sql = $"{colName} {sqlType}";
                if (isPrimaryKey)
                    sql += " PRIMARY KEY";

                columns.Add(sql);
            }

            string createSql =
                $"CREATE TABLE IF NOT EXISTS {tableName} (\n    {string.Join(",\n    ", columns)}\n);";

            using var cmd = new NpgsqlCommand(createSql, conn);
            cmd.ExecuteNonQuery();

            Console.WriteLine($"✔ Таблица '{tableName}' создана.");
        }
    }

    public static void UpdateExistingTables(List<Type> matched)
    {
        string connString = SettingsManager.Instance.Settings.ConnectionString;

        using var conn = new NpgsqlConnection(connString);
        conn.Open();

        foreach (var type in matched)
        {
            var tableAttr = type.GetCustomAttribute<TableAttribute>();

            string tableName = tableAttr.Name;
            
            var existingColumns = new Dictionary<string, string>();
            string colSql = @"SELECT column_name, data_type
                          FROM information_schema.columns
                          WHERE table_name = @table";
            using (var cmd = new NpgsqlCommand(colSql, conn))
            {
                cmd.Parameters.AddWithValue("table", tableName);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    existingColumns[reader.GetString(0)] = reader.GetString(1);
            }

            // Свойства класса с атрибутами
            var props = type.GetProperties()
                .Where(p => p.IsDefined(typeof(PrimaryKeyAttribute)) || p.IsDefined(typeof(ColumnAttribute)))
                .ToList();

            var classColumns = new Dictionary<string, string>();
            string primaryKey = null;
            foreach (var prop in props)
            {
                string colName = prop.Name.ToLower();
                string sqlType = MapType(prop.PropertyType).ToLower();
                classColumns[colName] = sqlType;
                if (prop.IsDefined(typeof(PrimaryKeyAttribute)))
                    primaryKey = colName;
            }
            
            foreach (var col in existingColumns.Keys)
            {
                if (!classColumns.ContainsKey(col))
                {
                    using var cmdDrop = new NpgsqlCommand($"ALTER TABLE {tableName} DROP COLUMN {col} CASCADE;", conn);
                    cmdDrop.ExecuteNonQuery();
                    Console.WriteLine($"❌ Колонка '{col}' удалена из таблицы '{tableName}'.");
                }
            }

            // Добавляем или изменяем колонки
            foreach (var kvp in classColumns)
            {
                string col = kvp.Key;
                string typeInClass = kvp.Value;

                if (existingColumns.TryGetValue(col, out string typeInDb))
                {
                    if (!string.Equals(typeInDb, typeInClass, StringComparison.OrdinalIgnoreCase))
                    {
                        using var cmdAlter =
                            new NpgsqlCommand(
                                $"ALTER TABLE {tableName} ALTER COLUMN {col} TYPE {typeInClass} USING {col}::{typeInClass};",
                                conn);
                        cmdAlter.ExecuteNonQuery();
                        Console.WriteLine(
                            $"🔧 Колонка '{col}' в таблице '{tableName}' изменена с '{typeInDb}' на '{typeInClass}'.");
                    }
                }
                else
                {
                    // Добавляем колонку
                    using var cmdAdd =
                        new NpgsqlCommand($"ALTER TABLE {tableName} ADD COLUMN {col} {typeInClass};", conn);
                    cmdAdd.ExecuteNonQuery();
                    Console.WriteLine($"⚡ Колонка '{col}' добавлена в таблицу '{tableName}'.");
                }
            }

            // Обновляем первичный ключ
            if (!string.IsNullOrEmpty(primaryKey))
            {
                string pkNameSql = @"SELECT constraint_name
                                 FROM information_schema.table_constraints
                                 WHERE table_name=@table AND constraint_type='PRIMARY KEY'";
                using var cmdPk = new NpgsqlCommand(pkNameSql, conn);
                cmdPk.Parameters.AddWithValue("table", tableName);
                var existingPk = cmdPk.ExecuteScalar() as string;

                if (!string.IsNullOrEmpty(existingPk))
                {
                    using var cmdDropPk =
                        new NpgsqlCommand($"ALTER TABLE {tableName} DROP CONSTRAINT {existingPk};", conn);
                    cmdDropPk.ExecuteNonQuery();
                }

                using var cmdAddPk =
                    new NpgsqlCommand(
                        $"ALTER TABLE {tableName} ADD CONSTRAINT {tableName}_pk PRIMARY KEY ({primaryKey});", conn);
                cmdAddPk.ExecuteNonQuery();
                Console.WriteLine($"🔑 Первичный ключ установлен на колонку '{primaryKey}' в таблице '{tableName}'.");
            }
        }
    }


    private static string MapType(Type t)
    {
        if (t == typeof(int))
            return "INTEGER";

        if (t == typeof(string))
            return "TEXT";

        return "TEXT"; // fallback
    }
}