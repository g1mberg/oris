using System.Net;
using System.Text;
using HttpServer.Framework.Core.HttpResponse;
using HttpServer.Framework.Settings;
using MiniTemplateEngine;

namespace HttpServer.Framework.core.Abstract.HttpResponse;

internal class PageResult(string pathTemplate, object data) : IResponseResult
{
    public void Execute(HttpListenerContext context)
    {
        var root = SettingsManager.Instance.Settings.StaticDirectoryPath!;
        var input = Path.Combine(root, pathTemplate.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        var html = new HtmlTemplateRenderer().RenderFromFile(input, data);
        var bytes = Encoding.UTF8.GetBytes(html);

        var resp = context.Response;
        resp.StatusCode = 200;
        resp.ContentType = "text/html; charset=utf-8";
        resp.ContentLength64 = bytes.Length;
        using var s = resp.OutputStream;
        s.Write(bytes, 0, bytes.Length);
    }
}