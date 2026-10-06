using Microsoft.Extensions.Logging;
using Torneo4vs4.Services;
using Torneo4vs4.ViewModels;

namespace Torneo4vs4
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // MAUI creerà una sola istanza di MatchSessionService e riutilizzerà sempre quella (Singleton).
            builder.Services.AddSingleton<MatchSessionService>();

            // Registra il servizio che gestisce il collegamento remoto con Supabase.
            builder.Services.AddSingleton<SupabaseService>();

            // Gestisce le operazioni sulle squadre.
            builder.Services.AddSingleton<TeamService>();

            builder.Services.AddSingleton<MatchService>();

            builder.Services.AddTransient<MatchesViewModel>();

            builder.Services.AddSingleton<PlayerService>();

            builder.Services.AddTransient<TeamsViewModel>();

            builder.Services.AddSingleton<MatchEventService>();

            builder.Services.AddTransient<StandingsViewModel>();

            builder.Services.AddSingleton<MatchPresenceService>();

            builder.Services.AddTransient<ScorersViewModel>();

            builder.Services.AddSingleton<RuleService>();

            builder.Services.AddTransient<RulesViewModel>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
