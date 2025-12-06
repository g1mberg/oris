using MyORM;

namespace HttpServer.Models;

public class User
{
    [PrimaryKey]
    public int Id { get; set; }
    
    [Column("login")]
    public string? Login { get; set; }
    
    [Column("password")]
    public byte[]? Password { get; set; }
    
    [Column("salt")]
    public byte[]? Salt { get; set; }
    
    [Column("isadmin")]
    public bool IsAdmin { get; set; }
}