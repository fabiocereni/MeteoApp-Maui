using Microsoft.Extensions.Logging;
using MeteoApp.Services;
using Plugin.LocalNotification;

#if IOS
using Plugin.Firebase.Core.Platforms.iOS;
#elif ANDROID
using Plugin.Firebase.Core.Platforms.Android;
#endif

namespace MeteoApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseLocalNotification()
            .UseMauiMaps()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddSingleton<AppwriteService>();

#if DEBUG
        builder.Logging.AddDebug();
        builder.Services.AddBlazorWebViewDeveloperTools();
#endif

#if IOS
        CrossFirebase.Initialize();
#elif ANDROID
        CrossFirebase.Initialize(() => Microsoft.Maui.ApplicationModel.Platform.CurrentActivity);
#endif

        return builder.Build();
    }
}