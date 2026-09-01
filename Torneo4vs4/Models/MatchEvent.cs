using Torneo4vs4.Enums;

namespace Torneo4vs4.Models;

// Rappresenta un singolo evento avvenuto durante una partita.
public class MatchEvent
{
    public int Id { get; set; }
    public int MatchId { get; set; }
    public MatchEventType Type { get; set; }

    public int PlayerId { get; set; }
    public int? AssistPlayerId { get; set; }
    public int Order { get; set; }
}
