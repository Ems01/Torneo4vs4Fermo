using Torneo4vs4.Models;

namespace Torneo4vs4.DTOs;

// Rappresenta tutti i dati definitivi di una partita terminata.
public class CompletedMatchData
{
    public int MatchId { get; set; }
    public int HomeTeamId { get; set; }
    public int AwayTeamId { get; set; }
    public int HomeGoals { get; set; }
    public int AwayGoals { get; set; }

    public string Referee1 { get; set; } = string.Empty;
    public string Referee2 { get; set; } = string.Empty;

    public List<MatchPresence> Presences { get; set; } = new();
    public List<MatchEvent> Events { get; set; } = new();
}