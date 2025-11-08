using System.Net;
using HttpServer.Shared;

namespace HttpServerApp;

public sealed class HttpServer
{
    private static volatile HttpServer? _instance;
    private static readonly object Lock = new();


    private readonly HttpListener _listener;
    private readonly SettingsModel _settings;

    private HttpServer(SettingsModel settings)
    {
        _settings = settings;
        _listener = new HttpListener();
    }


    public static HttpServer GetInstance(SettingsModel settings)
    {
        if (_instance is null)
            lock (Lock)
            {
                if (_instance is null)
                    _instance = new HttpServer(settings);
            }

        return _instance;
    }

    public void Start()
    {
        var prefix = $"{_settings.Domain}:{_settings.Port}/";
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
        catch (ObjectDisposedException)
        {
        }
        catch (HttpListenerException)
        {
        }
    }
 
    private async void ListenerCallback(IAsyncResult result)
    {
        if (_listener.IsListening)
        {
            var context = _listener.EndGetContext(result);
            Handler staticFilesHandler = new StaticFilesHandler(_settings.StaticDirectoryPath);
            Handler endpointsHandler = new EndpointsHandler();
            staticFilesHandler.Successor = endpointsHandler;
            staticFilesHandler.HandleRequest(context);

            if (_listener.IsListening)
                Receive();
        }
    }
}
