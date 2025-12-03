using System.Net;
using HttpServer.Framework.Core.HttpResponse;

namespace HttpServer.Framework.core.Abstract.HttpResponse;

public class NotFoundResult : IResponseResult
{
    public void Execute(HttpListenerContext context)
    {
        context.Response.StatusCode = 404;
        context.Response.Close();
    }
}