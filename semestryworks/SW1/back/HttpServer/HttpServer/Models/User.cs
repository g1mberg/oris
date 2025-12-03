using HttpServer.Framework.Utils;

namespace HttpServer.Models;



public class User
{
    public int id { get; set; }
    
    public string login { get; set; }
    
    public byte[] password { get; set; }
    
    public byte[] salt { get; set; }
    
    public bool isadmin { get; set; }
}