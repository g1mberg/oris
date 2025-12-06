using System.Collections.Specialized;
using System.Globalization;

namespace HttpServer.Models;

public class TourFilter
{
    private decimal MaxCost { get; }
    private string? Search { get; }
    private string? Destination { get; }
    private string? OriginCity { get; }
    private List<string>? Tags { get; }
    private string? SortBy { get; }
    private DateTime? StartDate { get; }
    private DateTime? EndDate { get; }

    public TourFilter(NameValueCollection query)
    {
        MaxCost = decimal.TryParse(query["maxCost"], out var mc) ? mc : 9999999m;
        Search = query["search"]?.Trim();
        Destination = query["destination"];
        OriginCity = query["originCity"];
        Tags = query["tags"]?.Split(',').ToList();
        SortBy = query["sortBy"] ?? "price";
        StartDate = ParseDate(query["startDate"]);
        EndDate = ParseDate(query["endDate"]);
    }

    public IEnumerable<TourCard> Apply(IEnumerable<TourCard> tours)
    {
        return tours.Where(tour =>
            tour.Cost < MaxCost &&
            (string.IsNullOrEmpty(Search) ||
             (tour.Name?.Contains(Search, StringComparison.OrdinalIgnoreCase) ?? false)) &&
            (string.IsNullOrEmpty(Destination) ||
             (tour.Destination?.Contains(Destination, StringComparison.OrdinalIgnoreCase) ?? false)) &&
            (string.IsNullOrEmpty(OriginCity) ||
             (tour.From?.Contains(OriginCity, StringComparison.OrdinalIgnoreCase) ?? false)) &&
            (Tags == null || Tags.All(tag => tour.searilezedTags.Contains(tag))) &&
            (!StartDate.HasValue || tour.DateStart >= StartDate.Value) &&
            (!EndDate.HasValue || tour.DateEnd <= EndDate.Value)
        );
    }

    public IEnumerable<TourCard> OrderByPriceOrDate(IEnumerable<TourCard> tours) =>
        SortBy switch
        {
            "Data" => tours.OrderBy(t => t.DateStart),
            _ => tours.OrderBy(t => t.Cost)
        };


    private static DateTime? ParseDate(string? dateString)
    {
        if (string.IsNullOrWhiteSpace(dateString))
            return null;

        if (DateTime.TryParseExact(dateString, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var date))
            return date >= DateTime.Today ? date : null;

        return null;
    }
}