using System.Net;
using HttpServer.Framework.core.Abstract.HttpResponse;
using HttpServer.Framework.core.Attributes;
using HttpServer.Framework.Core.HttpResponse;
using HttpServer.Models;
using HttpServer.Repository;
using HttpServer.Services;

namespace HttpServer.Endpoints;

[Endpoint]
internal class TurismoEndpoint : BaseEndpoint
{
    private readonly OrmRepository<TourCard> _tourRepo;
    private readonly SessionService _sessionService;

    public TurismoEndpoint()
    {
        _tourRepo = new OrmRepository<TourCard>("tours");
        var sessionRepo = new OrmRepository<Session>("sessions");
        var userRepo = new OrmRepository<User>("users");
        _sessionService = new SessionService(sessionRepo, userRepo);
    }

    [HttpGet("/turismo")]
    public IResponseResult GetMainPage()
    {
        try
        {
            var random = new Random();
            var tours = _tourRepo.GetAll().ToList();

            var data = new
            {
                Tours = tours,
                Offers = tours.OrderBy(_ => random.Next()).Take(3).ToList(),
                Tags = _tourRepo.GetDistinctValues(tour => tour.searilezedTags ?? []),
                Destinations = _tourRepo.GetDistinctSingleValues(tour => tour.Destination),
                Origins = _tourRepo.GetDistinctSingleValues(tour => tour.From),
            };

            return Page("/mainpage/html/index.html", data);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

    }

    [HttpGet("/turismo/tour")]
    public IResponseResult GetTourPage(HttpListenerContext context)
    {
        if (!int.TryParse(context.Request.QueryString["id"], out var id))
            return new NotFoundResult();

        var tour = _tourRepo.GetById(id);
        if (tour == null) return new NotFoundResult();

        var user = _sessionService.GetUserFromSession(context);

        var data = new
        {
            Tour = tour,
            IsAdmin = user?.IsAdmin ?? false
        };

        return Page("/mainpage/html/tour.html", data);
    }

    [HttpPost("/admin/save-tour")]
    public void SaveChanges(HttpListenerContext context)
    {
        var request = context.Request;
        using var input = request.InputStream;
        using var reader = new StreamReader(input, request.ContentEncoding);

        var body = reader.ReadToEnd();

        if (!int.TryParse(request.QueryString["id"], out var id) || !_tourRepo.TryGetById(id, out var tour))
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.Close();
            return;
        }
        
        TourCard.ReadJson(body, tour);

        _tourRepo.Update(id, tour!);
        context.Response.Close();
    }
}