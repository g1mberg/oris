using System.Collections;
using System.Globalization;
using System.Reflection;

namespace MiniTemplateEngine.Runtime;

/// <summary>
///     мини-вычислитель выражений
/// </summary>
internal static class Eval
{
    private static readonly object NotFound = new();

    /// <summary>
    ///     разрешает выражение вида "a.b.c" относительно контекста
    ///     поддерживает: скоупы, свойства/поля объекта, IDictionary(string→object).
    ///     спец имена: this - текущий скоуп/элемент, root - корневая модель.
    /// </summary>
    public static object? ResolvePath(string expr, Context ctx)
    {
        if (string.IsNullOrWhiteSpace(expr)) return null; // пустое выражение => null

        var parts = expr.Split('.', StringSplitOptions.RemoveEmptyEntries); // разбиваем по точкам

        object? cur;

        // сначала пытаемся найти первое имя в скоупах
        if (!ctx.TryResolveName(parts[0], out cur))
        {
            // падаем если это одиночное имя, вернуть this
            if (parts.Length == 1 && ctx.TryResolveName("this", out var curThis))
                return curThis;
            
            // пробуем свойство у root
            cur = GetMember(ctx.Root, parts[0]);
            if (cur == NotFound)
            {
                // спец имя root
                if (string.Equals(parts[0], "root", StringComparison.OrdinalIgnoreCase))
                    cur = ctx.Root;
                else
                    return null; // не нашли
            }
        }

        // Проходим остальные части пути
        for (var i = 1; i < parts.Length; i++)
        {
            if (cur == null) return null; // короткое замыкание
            cur = GetMember(cur, parts[i]); // берем следующее свойство/ключ
            if (cur == NotFound) return null; // не нашли =Ю null
        }

        return cur; // возвращаем найденное значение
    }

    /// <summary>возвращает член объекта по имени свойство/поле/ключ словаря</summary>
    private static object GetMember(object target, string name)
    {
        if (target is IDictionary<string, object?> dict) // словарь по строковому ключу
            return dict.TryGetValue(name, out var v) ? v! : NotFound;

        var t = target.GetType(); // тип объекта

        // Свойство public, instance
        var p = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
        if (p != null) return p.GetValue(target)!;

        var f = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
        if (f != null) return f.GetValue(target)!;

        return NotFound;
    }

    /// <summary>преобразование к булеву  true, непустая строка, ненулевое число, непустая коллекция</summary>
    public static bool IsTrue(object? v)
    {
        if (v is bool b) return b; // уже bool
        if (v == null) return false; // null → false
        if (v is string s) return !string.IsNullOrWhiteSpace(s); // непустая строка
        if (v is IEnumerable e) // коллекция: есть ли элементы
        {
            var en = e.GetEnumerator();
            try
            {
                return en.MoveNext();
            }
            finally
            {
                (en as IDisposable)?.Dispose();
            }
        }

        if (v is IConvertible c) // числа/даты → не default
            try
            {
                var d = Convert.ToDecimal(c, CultureInfo.InvariantCulture);
                return d != 0m; // ненулевое число → true
            }
            catch
            {
                return true;
            } // всё остальное считаем true

        return true; // объект по умолчанию true
    }

    /// <summary>
    ///     вычисляет условие  поддерживает  просто путь: "user.IsActive", сравнение: "a.b == 10" или "a == 'x'"
    /// </summary>
    public static bool EvaluateCondition(string expr, Context ctx)
    {
        var idx = expr.IndexOf("==", StringComparison.Ordinal);
        if (idx >= 0)
        {
            var left = expr[..idx].Trim();
            var right = expr[(idx + 2)..].Trim();

            var l = ResolveValue(left, ctx);
            var r = ResolveValue(right, ctx);

            // сравниваем строково инвариантно
            return string.Equals(Convert.ToString(l, CultureInfo.InvariantCulture),
                Convert.ToString(r, CultureInfo.InvariantCulture),
                StringComparison.Ordinal);
        }

        return IsTrue(ResolvePath(expr.Trim(), ctx));
    }

    /// <summary>Разрешает литерал или путь</summary>
    private static object? ResolveValue(string token, Context ctx)
    {
        if ((token.StartsWith("'") && token.EndsWith("'")) ||
            (token.StartsWith("\"") && token.EndsWith("\"")))
            return token[1..^1]; // строковый литерал

        if (decimal.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out var num))
            return num; // числовой литерал

        return ResolvePath(token, ctx);
    }

    /// <summary>пытается получить IEnumerable из выражения источника.</summary>
    public static IEnumerable? ResolveEnumerable(string expr, Context ctx)
    {
        var v = ResolvePath(expr, ctx); // получаем значение
        return v as IEnumerable; // приводим к IEnumerable (строку не разрезаем)
    }
}