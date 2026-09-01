namespace Torneo4vs4.Models;

// Rappresenta un giocatore partecipante al torneo.
public class Player
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int TeamId { get; set; }
}
