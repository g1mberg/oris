using System.Net;
using HttpServer.Framework.Core.HttpResponse;

namespace HttpServer.Framework.core.Abstract.HttpResponse;

public abstract class BaseEndpoint
{
    protected HttpListenerContext Context { get; private set; }

    internal void SetContext(HttpListenerContext context)
    {
        Context = context;
    }

    protected IResponseResult Page(string pathTemplate, object data)
    {
        return new PageResult(pathTemplate, data);
    }

    protected IResponseResult StringAnswer(string answer, object data) => new StringResult(answer, data);
}