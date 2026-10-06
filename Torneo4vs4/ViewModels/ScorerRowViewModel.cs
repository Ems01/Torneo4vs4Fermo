namespace Torneo4vs4.ViewModels;

public class ScorerRowViewModel
{
    public int PlayerId { get; set; }

    public string FirstName { get; set; } =
        string.Empty;

    public string LastName { get; set; } =
        string.Empty;

    public string PlayerName =>
        $"{LastName} {FirstName}";

    public string TeamName { get; set; } =
        string.Empty;

    public int Appearances { get; set; }

    public int Goals { get; set; }

    public int Assists { get; set; }
}