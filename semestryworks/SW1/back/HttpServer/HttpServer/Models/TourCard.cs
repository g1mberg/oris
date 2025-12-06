using System.Text.Json;
using MyORM;
using static HttpServer.Framework.Utils.DateNaming;

namespace HttpServer.Models;

public class TourCard
{
    [PrimaryKey]
    public int Id { get; set; }

    [Column("name")]
    public string? Name { get; set; }

    [Column("datestart")]
    public DateTime DateStart { get; set; }

    [Column("dateend")]
    public DateTime DateEnd { get; set; }

    [Column("origin")]
    public string? From { get; set; }

    [Column("status")]
    public string? Status { get; set; }

    [Column("cost")]
    public decimal Cost { get; set; }

    [Column("img")]
    public string? Img { get; set; }

    [Column("tags")]
    public string? Tags { get; set; }
    
    [Column("destination")]
    public string? Destination { get; set; }

    public List<string>? searilezedTags => Tags != null ? JsonSerializer.Deserialize<List<string>>(Tags) : [];

    private string PortDate => DateStart.DayOfYear.Equals(DateEnd.DayOfYear)
        ? "Dia " + GetPortDate(DateStart)
        : "De " + GetPortDate(DateStart) + " a " + GetPortDate(DateEnd); 

    public string Html => $"<li class=\"card\">" +
                          $"<a href=\"/turismo/tour?id={Id}\" class=\"no-underline\">" +
                          $"<img src=\"{Img}\" alt=\"Imagem 1\">" +
                          $"<div class=\"card-content\">" +
                          $"<h3>{Name}</h3>" +
                          $"<p>{PortDate} de {DateStart.Year}\n<br>Saída de <strong>{From}</strong></p>" +
                          $"<p>A partir de 12x <strong>R$ {Cost}/pessoa</strong></p>" +
                          $"</div>" +
                          $"</a>" +
                          $"</li>";
    
    public static void ReadJson(string json, TourCard? tour)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        
        root.TryGetProperty(".tour-header .title", out var res);
        tour.Name = res[0].GetString();
    }
}