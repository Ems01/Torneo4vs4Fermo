using Torneo4vs4.Models;

namespace Torneo4vs4.Services;

public class MatchPresenceService
{
    private readonly SupabaseService _supabaseService;

    public MatchPresenceService(
        SupabaseService supabaseService)
    {
        _supabaseService = supabaseService;
    }

    public async Task<List<MatchPresence>>
        GetMatchPresencesAsync()
    {
        await _supabaseService.InitializeAsync();

        var response =
            await _supabaseService.Client
                .From<MatchPresence>()
                .Get();

        return response.Models.ToList();
    }
}