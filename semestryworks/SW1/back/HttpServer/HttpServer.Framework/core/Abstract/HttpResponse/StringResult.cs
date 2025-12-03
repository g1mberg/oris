using System.Net;
using System.Text;
using HttpServer.Framework.Core.HttpResponse;
using MiniTemplateEngine;

namespace HttpServer.Framework.core.Abstract.HttpResponse;

public class StringResult(string str, object data) : IResponseResult
{
    public void Execute(HttpListenerContext context)
    {
        var html = new HtmlTemplateRenderer().RenderFromString(str, data);
        var bytes = Encoding.UTF8.GetBytes(html);

        var resp = context.Response;
        resp.StatusCode = 200;
        resp.ContentType = "text/html; charset=utf-8";
        resp.ContentLength64 = bytes.Length;
        using var s = resp.OutputStream;
        s.Write(bytes, 0, bytes.Length);
    }
}