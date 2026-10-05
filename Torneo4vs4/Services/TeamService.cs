using Torneo4vs4.Models;

namespace Torneo4vs4.Services;

// Gestisce tutte le operazioni relative alle squadre.
public class TeamService
{
    // Servizio utilizzato per comunicare con Supabase.
    private readonly SupabaseService _supabaseService;

    // Riceve SupabaseService tramite Dependency Injection.
    public TeamService(SupabaseService supabaseService)
    {
        _supabaseService = supabaseService;
    }

    // Recupera tutte le squadre presenti nel database.
    public async Task<List<Team>> GetTeamsAsync()
    {
        // Garantisce che il client Supabase sia inizializzato
        // prima di eseguire qualsiasi query.
        await _supabaseService.InitializeAsync();

        // Esegue:
        //
        // SELECT * FROM teams
        //
        // rispettando automaticamente RLS e permessi
        // associati al ruolo anon/authenticated.
        var response =
            await _supabaseService.Client
                .From<Team>()
                .Get();

        // Ordina le squadre in base all'ID e restituisce la lista.
        return response.Models
            .OrderBy(team => team.Id)
            .ToList();
    }
}