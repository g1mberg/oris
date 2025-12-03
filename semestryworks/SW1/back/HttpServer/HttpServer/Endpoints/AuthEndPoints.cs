using System.Net;
using System.Text;
using System.Text.Json;
using HttpServer.Framework.core.Attributes;
using HttpServer.Framework.Settings;
using HttpServer.Framework.Utils;
using HttpServer.Models;
using HttpServer.Services;
using Microsoft.VisualBasic.CompilerServices;
using MyORM;

namespace HttpServer.Endpoints;

[Endpoint]
internal class AuthEndpoint
{
    private OrmContext orm = new OrmContext(SettingsManager.Instance.Settings.ConnectionString!);
    
    [HttpPost("/auth/register")]
    public void Register(HttpListenerContext context)
    {
        var (login, password) = GetLoginPassword(context);
        
        if (orm.ReadAll<User>("users").Any(u => u.login.Equals(login)))
            context.Response.StatusCode = 409;
        else
        {
            var (hash, salt) = PasswordHasher.HashPassword(password);

            orm.Create(new User
            {
                password = hash,
                login = login,
                salt = salt,
                isadmin = false
            }, "users");
        }

        context.Response.Close();
    }
    
    
    [HttpPost("/auth/login")]
    public void Login(HttpListenerContext context)
    {
        var (login, password) = GetLoginPassword(context);
        
        var user = orm.ReadAll<User>("users").FirstOrDefault(x => x.login.Equals(login));
        
        if (user == null || !PasswordHasher.VerifyPassword(password, user.password, user.salt))
            context.Response.StatusCode = 401;
        else
        {
            orm.Create(new Session { expiresAt = DateTime.UtcNow.AddHours(1), userId = user.id }, "sessions");
            context.Response.Headers.Add("Set-Cookie", $"sessionId={orm.ReadAll<Session>("sessions").First(x => x.userId == user.id).id}; HttpOnly; Path=/; Max-Age=3600");
        }

        context.Response.Close();
    }

    private static (string, string) GetLoginPassword(HttpListenerContext context)
    {
        var request = context.Request;
        using var input = request.InputStream;
        using var reader = new StreamReader(input, request.ContentEncoding);
        
        using var doc = JsonDocument.Parse(reader.ReadToEnd());
        var root = doc.RootElement;

        return (root.GetProperty("username").GetString(), root.GetProperty("password").GetString());
    }
}