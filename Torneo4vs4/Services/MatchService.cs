using Torneo4vs4.Models;

namespace Torneo4vs4.Services;

// Gestisce le operazioni relative alle partite.
public class MatchService
{
    private readonly SupabaseService _supabaseService;

    public MatchService(SupabaseService supabaseService)
    {
        _supabaseService = supabaseService;
    }

    // Recupera tutte le partite ordinate per data.
    public async Task<List<Match>> GetMatchesAsync()
    {
        // Inizializza Supabase se necessario.
        await _supabaseService.InitializeAsync();

        // Recupera tutte le partite.
        var response =
            await _supabaseService.Client
                .From<Match>()
                .Get();

        // Ordina localmente per data.
        return response.Models
            .OrderBy(match => match.DateTime)
            .ToList();
    }
}