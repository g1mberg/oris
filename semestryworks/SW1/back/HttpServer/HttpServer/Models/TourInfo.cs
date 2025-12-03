using System.Text.Json;

namespace HttpServer.Models;

public class TourInfo
{
    public int id { get; set; }
    public string? name { get; set; }

    public static TourInfo ReadJson(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        
        root.TryGetProperty(".tour-header .title", out var res);
        return new TourInfo() {name = res[0].GetString() };
    }
}