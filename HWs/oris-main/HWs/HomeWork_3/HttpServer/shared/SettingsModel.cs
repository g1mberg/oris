using System.Text.Json;

namespace HttpServer.Shared;

public class SettingsModel
{
    public string StaticDirectoryPath { get; set; }
    public string Domain { get; set; }
    public string Port { get; set; }
    public Dictionary<string, string> MimeTypes { get; set; } = new();

    public static SettingsModel ReadJSON(string path) =>
        JsonSerializer.Deserialize<SettingsModel>(File.ReadAllText(path));
}