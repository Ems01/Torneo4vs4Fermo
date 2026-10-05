using Torneo4vs4.Configuration;

namespace Torneo4vs4.Services;

public class SupabaseService
{
    // Contiene il client Supabase utilizzato da tutta l'applicazione.
    private readonly Supabase.Client _client;
    private bool _isInitialized;

    // Costruttore.
    public SupabaseService()
    {
        Supabase.SupabaseOptions options =
            new Supabase.SupabaseOptions
            {
                // Permette al client di aggiornare automaticamente il token dell'amministratore quando sarà autenticato.
                AutoRefreshToken = true,
                AutoConnectRealtime = false
            };

        // Crea il client utilizzando URL e Publishable Key del nostro progetto.
        _client =
            new Supabase.Client(
                SupabaseConfig.Url,
                SupabaseConfig.PublishableKey,
                options);
    }

    // Espone il client Supabase agli altri servizi che in futuro dovranno effettuare query.
    public Supabase.Client Client
    {
        get
        {
            return _client;
        }
    }

    // Inizializza il client Supabase.
    public async Task InitializeAsync()
    {
        if (_isInitialized)
        {
            return;
        }

        await _client.InitializeAsync();

        _isInitialized = true;
    }
}