using Android.App;
using Android.Content.PM;
using Android.OS;
using Plugin.Firebase.Messaging;

namespace MeteoApp;

[Service]
public class MyFirebaseMessagingService : FirebaseMessagingService
{
    public override void OnMessageReceived(RemoteMessage message)
    {
        base.OnMessageReceived(message);
        System.Diagnostics.Debug.WriteLine($"Message received: {message.GetMessageId()}");
    }

    public override void OnNewToken(string token)
    {
        base.OnNewToken(token);
        System.Diagnostics.Debug.WriteLine($"FCM Token: {token}");
    }
}

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
}

