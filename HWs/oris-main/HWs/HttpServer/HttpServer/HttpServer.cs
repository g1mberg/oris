using System.Net;
using System.Text;
using HttpServer.Shared;

namespace HttpServer;

class HttpServer
{
    private SettingsModel _configs;
    private HttpListener _listener = new ();
    public HttpServer (SettingsModel configs) => _configs = configs;

    public void Start()
    {
        _listener.Prefixes.Add("http://" + _configs.Domain + ":" + _configs.Port + "/");
        _listener.Start();
        Console.WriteLine("Server is started");
        Receive();
        Console.WriteLine("Server is awaiting for request");
    }

    public void Stop()
    {
        _listener.Stop();
        Console.WriteLine("Server is stopped");
    }
    
    private void Receive() => _listener.BeginGetContext(ListenerCallback, _listener);

    private async void ListenerCallback(IAsyncResult result)
    {
        if (!_listener.IsListening) return;
        
        var context = _listener.EndGetContext(result);
        var response = context.Response;

        try
        {
            var responseText = await File.ReadAllTextAsync(_configs.StaticDirectoryPath + "index.html");
            response.Headers.Add("Content-type","text/html");
            var buffer = Encoding.UTF8.GetBytes(responseText);
            response.ContentLength64 = buffer.Length;
            using var output = response.OutputStream;
            await output.WriteAsync(buffer);
            await output.FlushAsync();
        }
        catch (DirectoryNotFoundException e)
        {
            Console.WriteLine("static folder not found");
            Stop();
        }
        catch (FileNotFoundException e)
        {
            Console.WriteLine("index.html is not found in static folder");
            Stop();
        }
        catch (Exception e)
        {
            Console.WriteLine("There is an exception: " + e.Message);
            Stop();
        }
        Receive();
    }

}