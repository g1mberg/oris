using System.Net;
using System.Text.Json;
using HttpServer.Framework.core.Attributes;
using HttpServer.Framework.Utils;
using HttpServer.Models;
using HttpServer.Repository;
using HttpServer.Services;

namespace HttpServer.Endpoints;

[Endpoint]
internal class AuthEndpoint
{
    private readonly OrmRepository<User> _userRepo;
    private readonly SessionService _sessionService;

    public AuthEndpoint()
    {
        _userRepo = new OrmRepository<User>("users");
        OrmRepository<Session> sessionRepo = new("sessions");
        _sessionService = new SessionService(sessionRepo, _userRepo);
    }
    
    [HttpPost("/auth/register")]
    public void Register(HttpListenerContext context)
    {
        var (login, password) = GetLoginPassword(context);
        
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password) ||
            _userRepo.GetAll().Any(u => u.Login == login))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Conflict; // 409
            context.Response.Close();
            return;
        }

        var (hash, salt) = PasswordHasher.HashPassword(password);

        _userRepo.Add(new User
        {
            Login = login,
            Password = hash,
            Salt = salt,
            IsAdmin = false
        });

        context.Response.StatusCode = (int)HttpStatusCode.OK;
        context.Response.Close();
    }
    
    
    [HttpPost("/auth/login")]
    public void Login(HttpListenerContext context)
    {
        var (login, password) = GetLoginPassword(context);

        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            context.Response.Close();
            return;
        }

        var user = _userRepo.GetAll().FirstOrDefault(x => x.Login == login);

        if (user == null || !PasswordHasher.VerifyPassword(password, user.Password!, user.Salt!))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            context.Response.Close();
            return;
        }

        _sessionService.SetSession(context, user);

        context.Response.StatusCode = (int)HttpStatusCode.OK;
        context.Response.Close();
    }

    private static (string?, string?) GetLoginPassword(HttpListenerContext context)
    {
        var request = context.Request;
        using var input = request.InputStream;
        using var reader = new StreamReader(input, request.ContentEncoding);
        
        using var doc = JsonDocument.Parse(reader.ReadToEnd());
        var root = doc.RootElement;

        return (root.GetProperty("username").GetString(),
                root.GetProperty("password").GetString());
    }
}
