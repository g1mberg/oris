using System.Net;
using HttpServer.Models;
using HttpServer.Repository;

namespace HttpServer.Services;

public class SessionService(OrmRepository<Session> sessions, OrmRepository<User> users)
{
    public User? GetUserFromSession(HttpListenerContext context)
    {
        if (!int.TryParse(context.Request.Cookies["sessionId"]?.Value, out var sessionId))
            return null;

        var session = sessions.GetById(sessionId);
        if (session == null) return null;

        if (session.ExpiresAt >= DateTime.UtcNow)
            return users.GetById(session.UserId);

        sessions.Delete(sessionId);
        return null;
    }

    public void SetSession(HttpListenerContext context, User user, int timeToExpire = 1)
    {
        var expires = DateTime.UtcNow.AddHours(timeToExpire);

        var session = sessions.Add(new Session
        {
            UserId = user.Id,
            ExpiresAt = expires
        });

        context.Response.Cookies.Add(
            new Cookie("sessionId", session.Id.ToString())
        {
            HttpOnly = true,
            Path = "/",
            Expires = expires
        });
    }
}