using System.Text.Json;
using HttpServer.Shared;

namespace HttpServer;

public class Program
{
    static void Main()
    {
        try
        {
            var httpServer = new HttpServer(SettingsModel.ReadJSON("settings.json"));
            httpServer.Start();
            while (Console.ReadLine() != "/stop");
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