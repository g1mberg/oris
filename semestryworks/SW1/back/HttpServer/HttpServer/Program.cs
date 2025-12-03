try
{
    var server = new HttpServer.Framework.Server.HttpServer();
    server.Start();

    Console.WriteLine("напишите /stop для завершения.\n");

    while (Console.ReadLine()?.Trim().ToLower() != "/stop")
    {
    }

    server.Stop();
}
catch (Exception e)
{
    Console.WriteLine("Ошибка: " + e.Message);
}