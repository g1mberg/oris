using System.Globalization;

namespace HttpServer.Framework.Utils;

public static class DateNaming
{
    public static string GetPortDate(string date)
    {
        const string format = "dd.MM.yyyy";
        DateTimeOffset.TryParseExact(date[..10], format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto);

        var ptBr = new CultureInfo("pt-BR");
        var result = dto.ToString("dd 'de' MMMM", ptBr);
        
        return result[..3] + result.Substring(3).ToLower(ptBr);
    }

    public static string GetPortDate(DateTimeOffset date) => GetPortDate(date.ToString());
}