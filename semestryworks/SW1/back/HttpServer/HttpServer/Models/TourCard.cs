using System.Text.Json;
using static HttpServer.Framework.Utils.DateNaming;

namespace HttpServer.Models;

public class TourCard
{
    public int id { get; set; }
    public string? name { get; set; }
    public string? dateStart{ get; set; }
    public string? dateEnd { get; set; }
    public string? from { get; set; }
    public string? status { get; set; }

    public decimal cost { get; set; }

    public string? img { get; set; }
    public string? tags{ get; set; }
    
    public List<string> searilezedTags => tags != null ? JsonSerializer.Deserialize<List<string>>(tags) : [];
    public DateTimeOffset? searilezedDateStart => DateTimeOffset.Parse(dateStart);

    private string suka => dateStart[..5].Equals(dateEnd[..5])
        ? "Dia " + GetPortDate(dateStart)
        : "De " + GetPortDate(dateStart) + " a " + GetPortDate(dateEnd); 
    public string html => $"<li class=\"card\">" +
                                $"<a href=\"/turismo/tour?id={id}\" class=\"no-underline\">" +
                                    $"<img src=\"{img}\" alt=\"Imagem 1\">" +
                                    $"<div class=\"card-content\">" +
                                        $"<h3>{name}</h3>" +
                                        $"<p>{suka} de {dateStart[6..10]}\n<br>Saída de <strong>{from}</strong></p>" +
                                        $"<p>A partir de 12x <strong>R$ {cost}/pessoa</strong></p>" +
                                    $"</div>" +
                                $"</a>" +
                            $"</li>";
    
}