namespace Torneo4vs4.ViewModels;

public class StandingRowViewModel
{
    public int Position { get; set; }

    public int TeamId { get; set; }

    public string TeamName { get; set; } = string.Empty;

    public int Played { get; set; }

    public int Wins { get; set; }

    public int Draws { get; set; }

    public int Losses { get; set; }

    public int GoalsFor { get; set; }

    public int GoalsAgainst { get; set; }

    // Totale cartellini gialli nel girone.
    public int YellowCards { get; set; }

    // Totale cartellini rossi nel girone.
    public int RedCards { get; set; }

    // Utilizzato temporaneamente durante
    // il calcolo degli scontri diretti.
    public int HeadToHeadPoints { get; set; }

    public int GoalDifference =>
        GoalsFor - GoalsAgainst;

    public int Points =>
        Wins * 3 + Draws;

    public string GoalDifferenceText =>
        GoalDifference > 0
            ? $"+{GoalDifference}"
            : GoalDifference.ToString();
}