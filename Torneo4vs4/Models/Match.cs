using Torneo4vs4.Enums;

namespace Torneo4vs4.Models;

// Rappresenta una partita del torneo.
public class Match
{
    public int Id { get; set; }
    public int HomeTeamId { get; set; }
    public int AwayTeamId { get; set; }

    public int? HomeGoals { get; set; }
    public int? AwayGoals { get; set; }

    public DateTime DateTime { get; set; }
    public MatchStatus Status { get; set; }

    public string? Referee1 { get; set; }
    public string? Referee2 { get; set; }
}
