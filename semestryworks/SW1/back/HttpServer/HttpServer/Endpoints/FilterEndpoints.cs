using System.Net;
using HttpServer.Framework.core.Abstract.HttpResponse;
using HttpServer.Framework.core.Attributes;
using HttpServer.Framework.Core.HttpResponse;
using HttpServer.Models;
using HttpServer.Repository;

namespace HttpServer.Endpoints;

[Endpoint]
public class FilterEndpoints : BaseEndpoint
{
    private readonly OrmRepository<TourCard> _tourRepo = new("tours");

    [HttpGet("/turismo/filter")]
    public IResponseResult GetFilterPage(HttpListenerContext context)
    {
        var tours = GetFilteredTours(context);
        var data = new
        {
            Tours = tours
        };

        return StringAnswer(@"$foreach(tour in Tours) ${tour.Html} $endfor", data);
    }

    private List<TourCard> GetFilteredTours(HttpListenerContext context)
    {
        var tours = _tourRepo.GetAll();
        var filter = new TourFilter(context.Request.QueryString);

        var filtered = filter.Apply(tours);
        var sorted = filter.OrderByPriceOrDate(filtered);

        return sorted.ToList();
    }
}