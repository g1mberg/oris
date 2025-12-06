using System.Net;
using HttpServer.Framework.core.Abstract;
using HttpServer.Framework.core.handlers;
using HttpServer.Framework.Settings;
using HttpServer.Framework.Utils;

namespace HttpServer.Framework.Server;

public sealed class HttpServer
{
    private readonly HttpListener _listener = new();
    private readonly SettingsManager _settingsManager = SettingsManager.Instance;

    public void Start()
    {
        var prefix = $"{_settingsManager.Settings.Domain}:{_settingsManager.Settings.Port}/";
        _listener.Prefixes.Add(prefix);
        _listener.Start();
        Console.WriteLine($"{prefix}");
        Console.WriteLine("Сервер ожидает...");
        Receive();
    }

    public void Stop()
    {
        _listener.Stop();
        Console.WriteLine("Сервер остановлен...");
    }

    private void Receive()
    {
        try
        {
            _listener.BeginGetContext(ListenerCallback, _listener);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private void ListenerCallback(IAsyncResult result)
    {
        try
        {
            if (!_listener.IsListening) return;
        
            var context = _listener.EndGetContext(result);
        
            Handler staticFilesHandler = new StaticFilesHandler();
            Handler endpointsHandler = new EndpointsHandler();
        
            staticFilesHandler.Successor = endpointsHandler;
            staticFilesHandler.HandleRequest(context);

            if (_listener.IsListening) Receive();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public static void SendStaticResponse(HttpListenerContext context, HttpStatusCode statusCode, string path)
    {
        
        var response = context.Response;
        var request = context.Request;
        byte[] buffer;
        if (ContentType.TryGetContentType(path, out var contentType))
        {
            response.ContentType = contentType;
            response.StatusCode = (int)statusCode;
            buffer = BufferManager.GetBytesFromFile(path);
        }
        else
        {
            response.StatusCode = 404;
            response.ContentType = "text/html; charset=utf-8";
            buffer = BufferManager.GetBytesFromString("<html><body><h1>404 Not Found</h1></body></html>");
        }
        
        response.ContentLength64 = buffer.Length;

        using var output = response.OutputStream;
        output.Write(buffer, 0, buffer.Length);


        Console.WriteLine(
            response.StatusCode == 200
                ? $"Запрос обработан: {request.Url?.AbsolutePath} {request.HttpMethod} - Status: {response.StatusCode}"
                : $"Ошибка запроса: {request.Url?.AbsolutePath} {request.HttpMethod} - Status: {response.StatusCode}");
    }
}