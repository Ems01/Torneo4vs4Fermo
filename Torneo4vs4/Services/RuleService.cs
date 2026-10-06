using Torneo4vs4.Models;

namespace Torneo4vs4.Services;

public class RuleService
{
    private readonly SupabaseService _supabaseService;

    public RuleService(
        SupabaseService supabaseService)
    {
        _supabaseService = supabaseService;
    }

    public async Task<List<RuleSection>>
        GetRulesAsync()
    {
        await _supabaseService.InitializeAsync();

        var response =
            await _supabaseService.Client
                .From<RuleSection>()
                .Get();

        return response.Models
            .OrderBy(rule => rule.DisplayOrder)
            .ToList();
    }
}