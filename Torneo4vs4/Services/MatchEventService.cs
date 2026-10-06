using Torneo4vs4.Models;

namespace Torneo4vs4.Services;

public class MatchEventService
{
    private readonly SupabaseService _supabaseService;

    public MatchEventService(SupabaseService supabaseService)
    {
        _supabaseService = supabaseService;
    }

    public async Task<List<MatchEvent>> GetMatchEventsAsync()
    {
        await _supabaseService.InitializeAsync();

        var response =
            await _supabaseService.Client
                .From<MatchEvent>()
                .Get();

        return response.Models.ToList();
    }
}