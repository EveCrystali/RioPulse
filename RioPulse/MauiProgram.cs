using Microsoft.Extensions.Logging;
using RioPulse.Core.Services;
namespace RioPulse
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            MauiAppBuilder builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
            builder
                .Services.AddLogging()
                .AddHttpClient<RaiderIoService>(client =>
                {
                    client.BaseAddress = new Uri("https://raider.io/api/v1/");
                });


#if DEBUG
            builder.Logging.SetMinimumLevel(LogLevel.Debug);
#endif

            return builder.Build();
        }
    }
}