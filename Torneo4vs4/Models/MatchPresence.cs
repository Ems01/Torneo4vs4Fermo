namespace Torneo4vs4.Models;

// Rappresenta la presenza o l'assenza di un giocatore in una determinata partita.
public class MatchPresence
{
    public int Id { get; set; }
    public int MatchId { get; set; }
    public int PlayerId { get; set; }
    public bool IsPresent { get; set; }
}
