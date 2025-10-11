using System.Text.Json;

namespace HttpServer.Shared;

public class SettingsModelSingleton
{
    private static Lazy<SettingsModel> _instance =
        new (() =>
            {
                try
                {
                    return SettingsModel.ReadJSON("settings.json");
                }
                catch (Exception e)
                {
                    throw new JsonException();
                }
            },
            LazyThreadSafetyMode.PublicationOnly);
    public static SettingsModel Instance => _instance.Value;
}