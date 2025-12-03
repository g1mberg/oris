using System.Net;
using HttpServer.Framework.core.Abstract.HttpResponse;
using HttpServer.Framework.core.Attributes;
using HttpServer.Framework.Core.HttpResponse;
using HttpServer.Framework.Settings;
using HttpServer.Models;
using MyORM;

namespace HttpServer.Endpoints;

[Endpoint]
internal class TurismoEndpoint : BaseEndpoint
{
    private OrmContext orm = new OrmContext(SettingsManager.Instance.Settings.ConnectionString!);

    [HttpGet("/turismo")]
    public IResponseResult GetMainPage()
    {
        var random = new Random();
        var tours = orm.ReadAll<TourCard>("tours");

        var data = new
        {
            Tours = tours,
            Count = tours.Count,
            Offers = tours.OrderBy(x => random.Next()).Take(3).ToList(),
            Tags = GetPossibleTags(tours)
        };

        return Page("/mainpage/html/index.html", data);
    }

    [HttpGet("/turismo/tour")]
    public IResponseResult GetTourPage(HttpListenerContext context)
    {
        if (!int.TryParse(context.Request.QueryString["id"], out var id))
            return GetMainPage();

        var tour = orm.ReadById<TourCard>(id, "tours");
        if (tour == null) return new NotFoundResult();

        var user = GetUserFromSession(context);
        Console.WriteLine(user?.isadmin);

        var data = new
        {
            Tour = tour,
            IsAdmin = user?.isadmin ?? false
        };

        return Page("/mainpage/html/tour.html", data);
    }

    [HttpGet("/turismo/filter")]
    public IResponseResult GetFilterPage(HttpListenerContext context)
    {
        var tours = GetFilteredTours(context);
        var data = new
        {
            Tours = tours,
            Count = tours.Count
        };

        return StringAnswer(@"$foreach(tour in Tours) ${tour.html} $endfor", data);
    }


    [HttpPost("/admin/save-tour")]
    public void SaveChanges(HttpListenerContext context)
    {
        var request = context.Request;
        using var input = request.InputStream;
        using var reader = new StreamReader(input, request.ContentEncoding);

        var tour = TourInfo.ReadJson(reader.ReadToEnd());
        orm.Update(int.Parse(request.QueryString["id"] ?? string.Empty), tour, "tours");

        context.Response.Close();
    }

    private static List<string> GetPossibleTags(List<TourCard> tours)
    {
        var tags = new HashSet<string>();

        foreach (var tag in tours.SelectMany(tour => tour.searilezedTags))
            tags.Add(tag);
        var res = tags.ToList();
        res.Sort();
        return res;
    }

    private List<TourCard> GetFilteredTours(HttpListenerContext context)
    {
        var tags = context.Request.QueryString["tags"]?.Split(',').ToList();
        var sortBy = context.Request.QueryString["sort"];
        if (!decimal.TryParse(context.Request.QueryString["maxCost"], out var maxCost)) maxCost = 9999999;
        
        var tours = orm.ReadAll<TourCard>("tours");
        var possibleTags = GetPossibleTags(tours);

        tours = tours.Where(x => x.cost < maxCost)
            .Where(x =>
            {
                return tags == null ||
                       tags.All(tag => !possibleTags.Contains(tag) || x.searilezedTags.Contains(tag));
            }).ToList();

        return sortBy switch
        {
            "Data" => tours.OrderBy(x => x.searilezedDateStart).ToList(),
            _ => tours.OrderBy(x => x.cost).ToList()
        };
    }

    public User? GetUserFromSession(HttpListenerContext context)
    {
        if (!int.TryParse(context.Request.Cookies["sessionId"]?.Value, out var sessionId))
            return null;
        
        var session = orm.ReadById<Session>(sessionId, "sessions");
        if (session == null) return null;
        if (session.expiresAt < DateTime.UtcNow)
        {
            orm.Delete(sessionId, "sessions");
            return null;
        }
        var user = orm.ReadById<User>(session.userId, "users");
        return user;
    }
}