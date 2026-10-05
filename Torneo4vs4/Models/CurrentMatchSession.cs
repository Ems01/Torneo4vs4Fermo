namespace Torneo4vs4.Models;

public class CurrentMatchSession
{
    public int MatchId { get; set; }
    public int HomeTeamId { get; set; }
    public int AwayTeamId { get; set; }

    public string HomeTeamName { get; set; } = string.Empty;
    public string AwayTeamName { get; set; } = string.Empty;

    public string? Referee1 { get; set; }
    public string? Referee2 { get; set; }

    public int HomeGoals { get; set; }
    public int AwayGoals { get; set; }

    public List<Player> Players { get; set; } = new();
    public List<MatchPresence> Presences { get; set; } = new();
    public List<MatchEvent> Events { get; set; } = new();
}