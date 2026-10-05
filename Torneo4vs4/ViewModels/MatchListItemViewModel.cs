using Torneo4vs4.Enums;

namespace Torneo4vs4.ViewModels;

// Rappresenta una partita così come deve essere mostrata
// nella pagina Incontri.
public class MatchListItemViewModel
{
    public int Id { get; set; }

    public string HomeTeamName { get; set; } = string.Empty;

    public string AwayTeamName { get; set; } = string.Empty;

    public DateTime DateTime { get; set; }

    public MatchStatus Status { get; set; }

    public int? HomeGoals { get; set; }

    public int? AwayGoals { get; set; }

    // Data visualizzata nell'interfaccia.
    public string DateText =>
        DateTime.ToString("dd/MM/yyyy");

    // Ora visualizzata nell'interfaccia.
    public string TimeText =>
        DateTime.ToString("HH:mm");

    // Testo principale mostrato al centro della card.
    public string ResultText
    {
        get
        {
            return Status switch
            {
                MatchStatus.Scheduled => TimeText,

                MatchStatus.InProgress =>
                    "IN CORSO",

                MatchStatus.Finished =>
                    $"{HomeGoals ?? 0}  -  {AwayGoals ?? 0}",

                _ => string.Empty
            };
        }
    }

    // Testo dello stato.
    public string StatusText
    {
        get
        {
            return Status switch
            {
                MatchStatus.Scheduled => "PROGRAMMATA",
                MatchStatus.InProgress => "IN CORSO",
                MatchStatus.Finished => "TERMINATA",
                _ => string.Empty
            };
        }
    }
}