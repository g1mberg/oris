using System.Text;
using HttpServer.Framework.Settings;

namespace HttpServer.Framework.Utils;

public static class BufferManager
{
    public static byte[] GetBytesFromFile(string path) => 
        File.ReadAllBytes(Path.HasExtension(path) ? TryGetFile(path) : TryGetFile(path + "/index.html"));

    public static byte[] GetBytesFromJson(string jsonString) => Encoding.UTF8.GetBytes(jsonString);
    
    public static byte[] GetBytesFromString(string str) => Encoding.UTF8.GetBytes(str);
    

    private static string TryGetFile(string path)
    {
        try
        {
            var targetPath = Path.Combine(path.Split("/"));

            if (SettingsManager.Instance.Settings.StaticDirectoryPath != null)
            {
                var found = Directory.EnumerateFiles(SettingsManager.Instance.Settings.StaticDirectoryPath,
                        $"{Path.GetFileName(path)}", SearchOption.AllDirectories)
                    .FirstOrDefault(f => f.EndsWith(targetPath, StringComparison.OrdinalIgnoreCase));

                return found ?? throw new FileNotFoundException(path);
            }
        }
        catch (DirectoryNotFoundException)
        {
            Console.WriteLine("Директория не найдена");
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine("Файл не найден");
        }
        catch (Exception)
        {
            Console.WriteLine("Ошибка при извлечении текста");
        }

        return Path.Combine(SettingsManager.Instance.Settings.StaticDirectoryPath, "404.html");
    }
}