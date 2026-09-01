namespace Torneo4vs4.Enums;

// Definisce tutti i possibili stati in cui può trovarsi una partita.
public enum MatchStatus
{
    // La partita è programmata ma non è ancora iniziata.
    Scheduled,

    // La partita è attualmente in corso.
    InProgress,

    // La partita è terminata e i dati sono stati confermati.
    Finished
}
