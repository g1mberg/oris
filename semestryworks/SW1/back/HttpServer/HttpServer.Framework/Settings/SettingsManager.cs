using System.Text.Json;

namespace HttpServer.Framework.Settings;

public class SettingsManager
{
    private static SettingsManager _instance;

    private static readonly object _lock = new();

    private SettingsManager()
    {
        LoadSettings();
    }

    public SettingsModel Settings { get; private set; }

    public static SettingsManager Instance
    {
        get
        {
            if (_instance != null) return _instance;
            
            lock (_lock)
            {
                _instance ??= new SettingsManager();
            }

            return _instance;
        }
    }

    private void LoadSettings()
    {
        try
        {
            var env = Environment.GetEnvironmentVariable("APP_ENV");
            var fileName = env == "Docker" && File.Exists("settings.docker.json")
                ? "settings.docker.json"
                : "settings.json";

            Console.WriteLine($"ENV = {env}, settings file = {fileName}");
            var settingsFile = File.ReadAllText(fileName);
            Settings = JsonSerializer.Deserialize<SettingsModel>(settingsFile)
                       ?? throw new InvalidOperationException("Десериализация провалилась");

            // проверки как у тебя
            if (string.IsNullOrEmpty(Settings.StaticDirectoryPath))
                throw new InvalidOperationException("Поле 'StaticDirectoryPath' не было заполнено из settings.json");
            if (string.IsNullOrEmpty(Settings.Domain))
                throw new InvalidOperationException("Поле 'Domain' не было заполнено из settings.json");
            if (string.IsNullOrEmpty(Settings.Port))
                throw new InvalidOperationException("Поле 'Port' не было заполнено из settings.json");
            if (string.IsNullOrEmpty(Settings.ConnectionString))
                throw new InvalidOperationException("Поле 'ConnectionString' не было заполнено из settings.json");
            
            var envCs = Environment.GetEnvironmentVariable("CONNECTION_STRING");
            if (!string.IsNullOrWhiteSpace(envCs))
                Settings.ConnectionString = envCs;

            Console.WriteLine("Настройки упешно загружены");
        }
        catch (FileNotFoundException)
        {
            throw new FileNotFoundException("Файл settings.json не был найден");
        }
        catch (DirectoryNotFoundException)
        {
            throw new DirectoryNotFoundException("Директория с файлом settings.json не была найдена");
        }
    }

}