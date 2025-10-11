using System.Text.Json;
using HttpServer.Shared;

namespace HttpServer;

public class Program
{
    static async Task Main()
    {
        try
        {
            var httpServer = new HttpServer();
            httpServer.Start();
            while (await Task.Run(() => Console.ReadLine() != "/stop")) ;
            httpServer.Stop();
        }
        catch (Exception e) when (e is DirectoryNotFoundException or FileNotFoundException)
        {
            Console.WriteLine("there is no settings.json");
        }
        catch (JsonException e)
        {
            Console.WriteLine("settings.json is incorrect");
        }
        catch (Exception e)
        {
            Console.WriteLine("There is an exception: " + e.Message);
        }
    }
}