using System.Net;
using System.Text;
using HttpServer.Services;

namespace HttpServer.Endpoints;

[Endpoint]
internal class AuthEndpoint
{
    [HttpGet]
    public void LoginPage(HttpListenerContext ctx)
    {
        ctx.Response.Redirect("/auth/");
    }

    [HttpPost]
    public async Task Login(HttpListenerContext ctx)
    {
        var req = ctx.Request;
        string body;
        using (var r = new StreamReader(req.InputStream, req.ContentEncoding))
        {
            body = await r.ReadToEndAsync();
        }

        var data = body.Split('&', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Split('=')).Select(y=> y[1]).ToList();

        var email = data[0].Replace("%40", "@");
        var password = data[1];
        
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password))
        {
            await Write(ctx, "email and password are required", 400);
            return;
        }

        await EmailService.SendAsync(
            email,
            "Ваши введённые данные",
            $"<h3>Данные из формы</h3><p><b>Email:</b> {email}</p><p><b>Password:</b> {password}</p>"
        );

        await Write(ctx, "{\"status\":\"ok\"}", 200, "application/json; charset=utf-8");
    }


    private static async Task Write(HttpListenerContext c, string s, int status,
        string ct = "text/plain; charset=utf-8")
    {
        var b = Encoding.UTF8.GetBytes(s);
        c.Response.StatusCode = status;
        c.Response.ContentType = ct;
        c.Response.ContentLength64 = b.Length;
        await using var o = c.Response.OutputStream;
        await o.WriteAsync(b, 0, b.Length);
        await o.FlushAsync();
    }
}