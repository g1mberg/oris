using System.Net;
using System.Text;
using HttpServer.Shared;

namespace HttpServer;

class HttpServer
{
    private SettingsModel _configs = SettingsModelSingleton.Instance;
    private HttpListener _listener = new();


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
        Console.WriteLine("Server is waiting for request");

        var context = _listener.EndGetContext(result);
        var request = context.Request;
        var response = context.Response;
        if (request.HttpMethod.Equals("get", StringComparison.OrdinalIgnoreCase))
        {
            
        }
        var path = _configs.StaticDirectoryPath + request.Url.LocalPath;
        path += path[^1] == '/' ? "index.html" : "";
        try
        {
            _configs.MimeTypes.TryGetValue(Path.GetExtension(path), out string mimeType);
            response.ContentType = mimeType ?? "text/html";
            await using var fileStream = File.OpenRead(path);
            response.ContentLength64 = fileStream.Length;
            await fileStream.CopyToAsync(response.OutputStream);
        }
        catch (DirectoryNotFoundException e)
        {
            Console.WriteLine("static folder not found");
            response.StatusCode = 404;
        }
        catch (FileNotFoundException e)
        {
            Console.WriteLine(path + " is not found");
            response.StatusCode = 404;
        }
        catch (Exception e)
        {
            Console.WriteLine("There is an exception: " + e.Message + " " + e.StackTrace);
            response.StatusCode = 404;
        }
        finally
        {
            response.OutputStream.Close();
        }
        Console.WriteLine((response.StatusCode == 404 ? "Request Denied: "
            : "Request processed: ") + request.Url);
        Receive();
    }
}