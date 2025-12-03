namespace HttpServer.Framework.Utils;

public static class ContentType
{
    private static readonly Dictionary<string, string> FileTypes = new()
    {
        { ".html", "text/html" },
        { ".css", "text/css" },
        { ".js", "application/javascript" },
        { ".png", "image/png" },
        { ".jpg", "image/jpeg" },
        { ".jpeg", "image/jpeg" },
        { ".webp", "image/webp" },
        { ".ico", "image/x-icon" },
        { ".svg", "image/svg+xml" },
        { ".json", "application/json" },
        { ".woff2", "font/woff2" }
    };

    public static bool TryGetContentType(string path, out string extension) => FileTypes.TryGetValue(Path.GetExtension(path).ToLower(), out extension);
}