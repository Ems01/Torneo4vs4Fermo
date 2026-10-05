using Torneo4vs4.Models;

namespace Torneo4vs4.Services;

public class PlayerService
{
    private readonly SupabaseService _supabaseService;

    public PlayerService(SupabaseService supabaseService)
    {
        _supabaseService = supabaseService;
    }

    // Recupera tutti i giocatori.
    public async Task<List<Player>> GetPlayersAsync()
    {
        await _supabaseService.InitializeAsync();

        var response =
            await _supabaseService.Client
                .From<Player>()
                .Get();

        return response.Models
            .OrderBy(player => player.LastName)
            .ThenBy(player => player.FirstName)
            .ToList();
    }
}