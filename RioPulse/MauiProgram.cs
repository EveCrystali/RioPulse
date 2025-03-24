using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RioPulse.Core.Services;

namespace RioPulse
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            MauiAppBuilder builder = MauiApp.CreateBuilder();
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            builder.Configuration.AddConfiguration(configuration);


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
                    client.BaseAddress = new Uri(configuration["RaiderIoApi:BaseAddress"]);
                });

            //Add new services
            builder.Services.AddSingleton<CharacterHistoryService>(provider =>
            {
                //Get the base storage path from the configuration
                string baseStoragePath = configuration["DataStorage:BaseStoragePath"];

                //Check that the path exists, otherwise use a default path
                if (string.IsNullOrEmpty(baseStoragePath))
                {
                    // Default path if not configured in appsettings.json
                    baseStoragePath = "./data/characters";
                }
                //Create the directory if it doesn't exist
                Directory.CreateDirectory(baseStoragePath);
                return new CharacterHistoryService(baseStoragePath);
            });
            builder.Services.AddTransient<CharacterOrchestrator>();
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddSingleton<CharacterAnalysisService>();


#if DEBUG
            builder.Logging.SetMinimumLevel(LogLevel.Debug);
#endif

            return builder.Build();
        }
    }
}
