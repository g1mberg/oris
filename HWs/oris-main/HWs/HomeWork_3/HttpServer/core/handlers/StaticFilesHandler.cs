using System.Net;
using HttpServer.Shared;

namespace HttpServer.core.handlers
{
    internal class StaticFilesHandler : Handler
    {
        private readonly string _root;
        private readonly string _defaultFile;

        public StaticFilesHandler(string staticRoot, string defaultFile = "index.html")
        {
            if (string.IsNullOrWhiteSpace(staticRoot))
                throw new ArgumentException("Static root must be provided", nameof(staticRoot));

            _root = Path.GetFullPath(staticRoot);
            _defaultFile = defaultFile;
        }

        public override async void HandleRequest(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            var path = _root + request.Url.LocalPath;
            if (path[^1] == '/') path += "index.html";

            if (!File.Exists(path))
            {
                Successor?.HandleRequest(context);
                return;
            }

            var responseText = await File.ReadAllBytesAsync(path);

            var type = ContentType.GetContentType(path);
            response.ContentType = type ?? "text/html";
            response.ContentLength64 = responseText.Length;

            using var output = response.OutputStream;
            await output.WriteAsync(responseText, 0, responseText.Length);
            await output.FlushAsync();
        }
        
    }
}