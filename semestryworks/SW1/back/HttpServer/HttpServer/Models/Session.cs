namespace HttpServer.Models;

public class Session
{
    public int id { get; set; }
    public int userId { get; set; }
    public DateTime expiresAt { get; set; }
}